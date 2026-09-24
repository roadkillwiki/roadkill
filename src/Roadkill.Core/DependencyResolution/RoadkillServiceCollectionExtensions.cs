using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Caching;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using Roadkill.Core.Attachments;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Domain.Export;
using Roadkill.Core.Email;
using Roadkill.Core.Import;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc;
using Roadkill.Core.Mvc.Setup;
using Roadkill.Core.Mvc.WebApi;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.DependencyResolution
{
	/// <summary>
	/// Registers the Roadkill services, MVC and authentication with the ASP.NET Core dependency injection container
	/// (this replaces the StructureMap RoadkillRegistry).
	/// </summary>
	public static class RoadkillServiceCollectionExtensions
	{
		/// <summary>
		/// Adds all Roadkill services, MVC (controllers and views), cookie authentication and the REST api.
		/// </summary>
		/// <param name="services">The service collection.</param>
		/// <param name="contentRootPath">The web application's content root, containing App_Data, Themes etc.</param>
		/// <param name="configFilePath">The path to the JSON settings file. If empty, appsettings.json in the content root is used.</param>
		public static IServiceCollection AddRoadkill(this IServiceCollection services, string contentRootPath, string configFilePath = "")
		{
			if (string.IsNullOrEmpty(configFilePath))
				configFilePath = Path.Combine(contentRootPath, JsonConfigReaderWriter.DefaultFilename);

			var configReaderWriter = new JsonConfigReaderWriter(configFilePath, contentRootPath);
			ApplicationSettings startupSettings = configReaderWriter.GetApplicationSettings();

			services.AddHttpContextAccessor();

			// Settings - the ApplicationSettings are re-read per request, so the installer's changes are picked up.
			services.AddSingleton<ConfigReaderWriter>(configReaderWriter);
			services.AddScoped<ApplicationSettings>(sp => sp.GetRequiredService<ConfigReaderWriter>().GetApplicationSettings());

			AddRepositories(services);
			AddCaching(services);
			AddServices(services);
			AddPlugins(services, startupSettings);
			AddMvc(services);
			AddAuthentication(services);

			if (startupSettings.IsRestApiEnabled)
				AddSwagger(services);

			return services;
		}

		private static void AddRepositories(IServiceCollection services)
		{
			services.AddSingleton<IRepositoryFactory, RepositoryFactory>();
			services.AddTransient<IDatabaseTester, DatabaseTester>();

			// These are null until Roadkill is installed (there is no connection string).
			services.AddScoped<ISettingsRepository>(sp =>
			{
				ApplicationSettings settings = sp.GetRequiredService<ApplicationSettings>();
				return sp.GetRequiredService<IRepositoryFactory>().GetSettingsRepository(settings.DatabaseName, settings.ConnectionString);
			});

			services.AddScoped<IUserRepository>(sp =>
			{
				ApplicationSettings settings = sp.GetRequiredService<ApplicationSettings>();
				return sp.GetRequiredService<IRepositoryFactory>().GetUserRepository(settings.DatabaseName, settings.ConnectionString);
			});

			services.AddScoped<IPageRepository>(sp =>
			{
				ApplicationSettings settings = sp.GetRequiredService<ApplicationSettings>();
				return sp.GetRequiredService<IRepositoryFactory>().GetPageRepository(settings.DatabaseName, settings.ConnectionString);
			});
		}

		private static void AddCaching(IServiceCollection services)
		{
			// The cached data lives in the singleton ObjectCache; the cache classes read the (per request) settings.
			services.AddSingleton<ObjectCache>(new MemoryCache("Roadkill"));
			services.AddScoped<ListCache>();
			services.AddScoped<SiteCache>();
			services.AddScoped<PageViewModelCache>();
			services.AddScoped<IPluginCache>(sp => sp.GetRequiredService<SiteCache>());
		}

		private static void AddServices(IServiceCollection services)
		{
			// Security
			services.AddScoped<UserServiceBase>(CreateUserService);
			services.AddScoped<IUserContext, UserContext>();
			services.AddScoped<IAuthorizationProvider, AuthorizationProvider>();

			// Services
			services.AddScoped<SettingsService>();
			services.AddScoped<ISettingsService>(sp => sp.GetRequiredService<SettingsService>());
			services.AddScoped<SearchService>();
			services.AddScoped<ISearchService>(sp => sp.GetRequiredService<SearchService>());
			services.AddScoped<PageHistoryService>();
			services.AddScoped<PageService>();
			services.AddScoped<IPageService>(sp => sp.GetRequiredService<PageService>());
			services.AddScoped<IInstallationService, InstallationService>();
			services.AddScoped<IFileService>(sp =>
			{
				ApplicationSettings settings = sp.GetRequiredService<ApplicationSettings>();
				if (settings.UseAzureFileStorage)
					return ActivatorUtilities.CreateInstance<AzureFileService>(sp);

				return ActivatorUtilities.CreateInstance<LocalFileService>(sp);
			});

			// Text parsing
			services.AddScoped<MarkupConverter>();
			services.AddTransient<CustomTokenParser>();

			// Emails
			services.AddTransient<IEmailClient, EmailClient>();
			services.AddTransient<SignupEmail>();
			services.AddTransient<ResetPasswordEmail>();

			// Import/export
			services.AddTransient<IWikiImporter, ScrewTurnImporter>();
			services.AddTransient<WikiExporter>();
		}

		/// <summary>
		/// Creates the <see cref="UserServiceBase"/>: a custom type (from the userServiceType setting) or the default <see cref="FormsAuthUserService"/>.
		/// </summary>
		private static UserServiceBase CreateUserService(IServiceProvider serviceProvider)
		{
			ApplicationSettings settings = serviceProvider.GetRequiredService<ApplicationSettings>();
			string userServiceTypeName = settings.UserServiceType;

			if (!string.IsNullOrEmpty(userServiceTypeName))
			{
				Type userServiceType = Type.GetType(userServiceTypeName, false, false);
				if (userServiceType == null)
					throw new IoCException(null, "Unable to find UserService type {0}. Make sure you use the AssemblyQualifiedName.", userServiceTypeName);

				return (UserServiceBase)ActivatorUtilities.CreateInstance(serviceProvider, userServiceType);
			}

			// The repositories are null until Roadkill is installed, so they're passed explicitly.
			return new FormsAuthUserService(settings, serviceProvider.GetService<IUserRepository>(), serviceProvider.GetService<IPageRepository>());
		}

		private static void AddPlugins(IServiceCollection services, ApplicationSettings startupSettings)
		{
			services.AddSingleton(PluginTypeRegistry.Create(startupSettings));
			services.AddScoped<IPluginFactory, PluginFactory>();
		}

		private static void AddMvc(IServiceCollection services)
		{
			services.Configure<RouteOptions>(options =>
			{
				options.ConstraintMap[Routing.LowercaseTransformerName] = typeof(LowercaseParameterTransformer);
			});

			services.AddControllersWithViews()
				.AddApplicationPart(typeof(RoadkillServiceCollectionExtensions).Assembly)
				.AddJsonOptions(options =>
				{
					// The javascript (and the REST api clients) expect PascalCase property names, as with ASP.NET MVC 5.
					options.JsonSerializerOptions.PropertyNamingPolicy = null;
				})
				.AddRazorOptions(options =>
				{
					options.ViewLocationExpanders.Add(new RoadkillViewLocationExpander());
				})
				// Custom themes (Themes/xyz/Theme.cshtml) and plugin views can be added without recompiling Roadkill.
				.AddRazorRuntimeCompilation();
		}

		private static void AddAuthentication(IServiceCollection services)
		{
			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.Cookie.Name = ".ROADKILLAUTH";
					options.Cookie.HttpOnly = true;
					options.Cookie.SameSite = SameSiteMode.Lax;
					options.LoginPath = "/user/login";
					options.LogoutPath = "/user/logout";
					options.AccessDeniedPath = "/user/login";
					options.ReturnUrlParameter = "ReturnUrl";
					options.ExpireTimeSpan = TimeSpan.FromDays(14);
					options.SlidingExpiration = true;
				});
		}

		private static void AddSwagger(IServiceCollection services)
		{
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo() { Title = "Roadkill Web API", Version = "3.0" });

				// Only document the REST api controllers, not the MVC ones.
				options.DocInclusionPredicate((documentName, apiDescription) =>
					apiDescription.RelativePath != null && apiDescription.RelativePath.StartsWith("api/", StringComparison.OrdinalIgnoreCase));

				options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
				{
					Type = SecuritySchemeType.ApiKey,
					In = ParameterLocation.Header,
					Name = ApiKeyAuthorizeAttribute.APIKEY_HEADER_KEY,
					Description = "API key"
				});

				options.AddSecurityRequirement(document => new OpenApiSecurityRequirement()
				{
					[new OpenApiSecuritySchemeReference("ApiKey", document)] = new List<string>()
				});
			});
		}

		/// <summary>
		/// Adds the Roadkill middleware and routes: static files, the installer redirect, attachments,
		/// authentication, localization and MVC.
		/// </summary>
		public static WebApplication UseRoadkill(this WebApplication app)
		{
			HttpContextHolder.Accessor = app.Services.GetRequiredService<IHttpContextAccessor>();

			using (IServiceScope scope = app.Services.CreateScope())
			{
				ApplicationSettings settings = scope.ServiceProvider.GetRequiredService<ApplicationSettings>();
				ConfigureLogging(settings);
				AttachmentMiddleware.ValidateRoute(settings);

				if (settings.IsRestApiEnabled)
				{
					app.UseSwagger();
					app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Roadkill Web API"));
				}
			}

			UseRoadkillStaticFiles(app);

			app.UseStatusCodePagesWithReExecute("/wiki/notfound");
			app.UseMiddleware<UiCultureMiddleware>();
			app.UseMiddleware<InstallCheckMiddleware>();
			app.UseMiddleware<AttachmentMiddleware>();

			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.MapRoadkillRoutes();

			Log.Information("Roadkill started");
			return app;
		}

		/// <summary>
		/// Serves the static files from wwwroot, and the /Assets, /Themes and /Plugins folders of the content root
		/// (the .cshtml files in these folders aren't served, as they have no content type mapping).
		/// </summary>
		private static void UseRoadkillStaticFiles(WebApplication app)
		{
			app.UseStaticFiles();

			string contentRoot = app.Environment.ContentRootPath;
			foreach (string folder in new[] { "Assets", "Themes", "Plugins" })
			{
				string path = Path.Combine(contentRoot, folder);
				if (!Directory.Exists(path))
					continue;

				app.UseStaticFiles(new StaticFileOptions()
				{
					FileProvider = new PhysicalFileProvider(path),
					RequestPath = "/" + folder
				});
			}
		}

		private static void ConfigureLogging(ApplicationSettings settings)
		{
			try
			{
				Log.ConfigureLogging(settings);
			}
			catch (ConfigurationException ex)
			{
				Console.Error.WriteLine("Unable to configure the Roadkill logging: " + ex.Message);
			}
		}
	}
}
