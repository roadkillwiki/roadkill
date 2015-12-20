using System;
using System.IO;
using System.Runtime.Caching;
using Roadkill.Core.Attachments;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Domain.Export;
using Roadkill.Core.Email;
using Roadkill.Core.Import;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.WebViewPages;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;
using Roadkill.Core.Services;
using StructureMap;
using StructureMap.Building;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using StructureMap.Pipeline;
using StructureMap.TypeRules;
using StructureMap.Web;
using WebGrease.Css.Extensions;
using UserController = Roadkill.Core.Mvc.Controllers.UserController;

namespace Roadkill.Core.DependencyResolution.StructureMap
{
	public class RoadkillRegistry : Registry
	{
		private class AbstractClassConvention<T> : IRegistrationConvention
		{
			public void ScanTypes(TypeSet types, Registry registry)
			{
				types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).ForEach(type =>
				{
					if (type.CanBeCastTo<T>())
					{
						registry.For(typeof(T)).LifecycleIs(new UniquePerRequestLifecycle()).Add(type);
					}
				});
			}
		}

		public ApplicationSettings ApplicationSettings { get; set; }

		public RoadkillRegistry(ConfigReaderWriter configReader)
		{
			ApplicationSettings = configReader.GetApplicationSettings();

			Scan(ScanTypes);
			ConfigureInstances(configReader);
		}

		private static void CopyPlugins(ApplicationSettings applicationSettings)
		{
			string pluginsDestPath = applicationSettings.PluginsBinPath;
			if (!Directory.Exists(pluginsDestPath))
				Directory.CreateDirectory(pluginsDestPath);

			PluginFileManager.CopyPlugins(applicationSettings);
		}

		private void ScanTypes(IAssemblyScanner scanner)
		{
			scanner.TheCallingAssembly();
			scanner.AssembliesFromApplicationBaseDirectory(assembly => assembly.FullName.Contains("Roadkill"));
			scanner.SingleImplementationsOfInterface();
			scanner.WithDefaultConventions();

			// Scan plugins: this includes everything e.g repositories, UserService, FileService TextPlugins
			CopyPlugins(ApplicationSettings);
			foreach (string subDirectory in Directory.GetDirectories(ApplicationSettings.PluginsBinPath))
			{
				scanner.AssembliesFromPath(subDirectory);
			}

			// Plugins
			scanner.With(new AbstractClassConvention<TextPlugin>());
			scanner.With(new AbstractClassConvention<SpecialPagePlugin>());
            scanner.AddAllTypesOf<IPluginFactory>();

			// Config, context
			scanner.AddAllTypesOf<ApplicationSettings>();
			scanner.AddAllTypesOf<IUserContext>();

			// Repositories
			scanner.AddAllTypesOf<ISettingsRepository>();
			scanner.AddAllTypesOf<IUserRepository>();
			scanner.AddAllTypesOf<IPageRepository>();

			// Services
			scanner.With(new AbstractClassConvention<UserServiceBase>());
			scanner.AddAllTypesOf<IPageService>();
			scanner.AddAllTypesOf<ISearchService>();
			scanner.AddAllTypesOf<ISettingsService>();
			scanner.AddAllTypesOf<IActiveDirectoryProvider>();
			scanner.AddAllTypesOf<IFileService>();
			scanner.AddAllTypesOf<IInstallationService>();

			// Text parsers
			scanner.AddAllTypesOf<MarkupConverter>();
			scanner.AddAllTypesOf<CustomTokenParser>();

			// MVC Related
			scanner.AddAllTypesOf<UserViewModel>();
			scanner.AddAllTypesOf<SettingsViewModel>();
			scanner.AddAllTypesOf<AttachmentRouteHandler>();
			scanner.AddAllTypesOf<ISetterInjected>();
			scanner.AddAllTypesOf<IAuthorizationAttribute>();
			scanner.AddAllTypesOf<RoadkillLayoutPage>();
			scanner.AddAllTypesOf(typeof(RoadkillViewPage<>));
			scanner.ConnectImplementationsToTypesClosing(typeof(RoadkillViewPage<>));

			// Emails
			scanner.AddAllTypesOf<SignupEmail>();
			scanner.AddAllTypesOf<ResetPasswordEmail>();

			// Cache
			scanner.AddAllTypesOf<ListCache>();
			scanner.AddAllTypesOf<PageViewModelCache>();

			// Export
			scanner.AddAllTypesOf<WikiExporter>();

			// Controllers
			scanner.AddAllTypesOf<IRoadkillController>();
			scanner.AddAllTypesOf<ControllerBase>();
			scanner.AddAllTypesOf<ApiControllerBase>();
			scanner.AddAllTypesOf<ConfigurationTesterController>();
		}

		private void ConfigureInstances(ConfigReaderWriter configReader)
		{
			// Appsettings and reader - these need to go first
			For<ConfigReaderWriter>().HybridHttpOrThreadLocalScoped().Use(configReader);
			For<ApplicationSettings>()
				.HybridHttpOrThreadLocalScoped()
				.Use(x => x.TryGetInstance<ConfigReaderWriter>().GetApplicationSettings());

			// Repositories
			ConfigureRepositories();

			// Work around for controllers that use RenderAction() needing to be unique
			// See https://github.com/webadvanced/Structuremap.MVC5/issues/3
			For<HomeController>().AlwaysUnique();
			For<UserController>().AlwaysUnique();
			For<ConfigurationTesterController>().AlwaysUnique();
			For<WikiController>().AlwaysUnique();

			//For<InstallController>().Use("InstallController", x =>
			//{
			//	ApplicationSettings appSettings = x.GetInstance<ApplicationSettings>();
			//	ConfigReaderWriter readerWriter = x.GetInstance<ConfigReaderWriter>();
			//	IRepositoryFactory factory = x.GetInstance<IRepositoryFactory>("Installer-IRepositoryFactory");

			//	return new InstallController();
			//});

			// Plugins
			For<IPluginFactory>().Singleton().Use<PluginFactory>();

			// Screwturn importer
			For<IWikiImporter>().Use<ScrewTurnImporter>();

			// Emails
			For<SignupEmail>().Use<SignupEmail>();
			For<ResetPasswordEmail>().Use<ResetPasswordEmail>();

			// Cache
			For<ObjectCache>().Singleton().Use(new MemoryCache("Roadkill"));
			For<ListCache>().Singleton();
			For<SiteCache>().Singleton();
			For<PageViewModelCache>().Singleton();
			For<IPluginCache>().Use<SiteCache>();

			// Services
			For<IPageService>().HybridHttpOrThreadLocalScoped().Use<PageService>();
			For<IInstallationService>().HybridHttpOrThreadLocalScoped().Use<InstallationService>();

			// Security
			For<IAuthorizationProvider>().Use<AuthorizationProvider>();
			For<IUserContext>().HybridHttpOrThreadLocalScoped();
#if !MONO
			For<IActiveDirectoryProvider>().Use<ActiveDirectoryProvider>();
#endif
			// User service
			ConfigureUserService();

			// File service
			ConfigureFileService();

			// Setter injected classes
			ConfigureSetterInjection();
		}

		private void ConfigureRepositories()
		{
			// TODO: All services should take an IRepositoryFactory, no injection should be needed for IXYZRepository
			For<IRepositoryFactory>()
				.Singleton()
				.Use<RepositoryFactory>("IRepositoryFactory", x =>
				{
					ApplicationSettings appSettings = x.GetInstance<ApplicationSettings>();
					return new RepositoryFactory(appSettings.DatabaseName, appSettings.ConnectionString);
				});

			For<IRepositoryFactory>()
				.Singleton()
				.Add("IRepositoryFactory For the Installer", x => new RepositoryFactory("installer", "installer"))
				.Named("Installer-IRepositoryFactory");

			For<ISettingsRepository>()
				.HybridHttpOrThreadLocalScoped()
				.Use("ISettingsRepository", x =>
				{
					ApplicationSettings appSettings = x.GetInstance<ApplicationSettings>();
					return x.TryGetInstance<IRepositoryFactory>()
						.GetSettingsRepository(appSettings.DatabaseName, appSettings.ConnectionString);
				});

			For<IUserRepository>()
				.HybridHttpOrThreadLocalScoped()
				.Use("IUserRepository", x =>
				{
					ApplicationSettings appSettings = x.GetInstance<ApplicationSettings>();
					return x.TryGetInstance<IRepositoryFactory>()
						.GetUserRepository(appSettings.DatabaseName, appSettings.ConnectionString);
				});

			For<IPageRepository>()
				.HybridHttpOrThreadLocalScoped()
				.Use("IPageRepository", x =>
				{
					ApplicationSettings appSettings = x.GetInstance<ApplicationSettings>();
					return x.TryGetInstance<IRepositoryFactory>()
						.GetPageRepository(appSettings.DatabaseName, appSettings.ConnectionString);
				});
		}

		private void ConfigureSetterInjection()
		{
			Policies.SetAllProperties(x => x.OfType<ISetterInjected>());
			Policies.SetAllProperties(x => x.OfType<IAuthorizationAttribute>());
			Policies.SetAllProperties(x => x.TypeMatches(t => t == typeof (RoadkillViewPage<>)));
			Policies.SetAllProperties(x => x.TypeMatches(t => t == typeof (RoadkillLayoutPage)));

			// Setter inject the *internal* properties for the text plugins
			For<TextPlugin>().OnCreationForAll("set plugin cache", (ctx, plugin) => plugin.PluginCache = ctx.GetInstance<IPluginCache>());
			For<TextPlugin>().OnCreationForAll("set plugin repository", (ctx, plugin) => plugin.Repository = ctx.GetInstance<ISettingsRepository>());
		}

		private void ConfigureFileService()
		{
			if (ApplicationSettings.UseAzureFileStorage)
			{
				For<IFileService>()
					.HybridHttpOrThreadLocalScoped()
					.Use<AzureFileService>();
			}
			else
			{
				For<IFileService>()
					.HybridHttpOrThreadLocalScoped()
					.Use<LocalFileService>();
			}
		}

		private void ConfigureUserService()
		{
			// Windows authentication, custom or the default FormsAuth
			string userServiceTypeName = ApplicationSettings.UserServiceType;

			if (ApplicationSettings.UseWindowsAuthentication)
			{
#if !MONO
				For<UserServiceBase>()
					.HybridHttpOrThreadLocalScoped()
					.Use<ActiveDirectoryUserService>();
#endif
			}
			else if (!string.IsNullOrEmpty(userServiceTypeName))
			{
				try
				{
					Type userServiceType = Type.GetType(userServiceTypeName, false, false);
					if (userServiceType == null)
						throw new IoCException(null, "Unable to find UserService type {0}. Make sure you use the AssemblyQualifiedName.", userServiceTypeName);

					For<UserServiceBase>()
						.Use("Inject custom UserService", context =>
						{					
							return (UserServiceBase) context.GetInstance(userServiceType);
						});
				}
				catch (StructureMapBuildException)
				{
					throw new IoCException(null, "Unable to find UserService type {0}", userServiceTypeName);
				}
			}
			else
			{
				For<UserServiceBase>()
					.HybridHttpOrThreadLocalScoped()
					.Use<FormsAuthUserService>();
			}
		}
	}
}