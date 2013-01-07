using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System;
using Roadkill.Core.Files;
using StructureMap;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	/// <summary>
	/// The entry point application (Global.asax) for Roadkill.
	/// </summary>
	public class RoadkillApplication : HttpApplication
	{
		protected void Application_Start()
		{
			SetupIoC();
			AttachmentRouteHandler.Register(ObjectFactory.GetInstance<IConfigurationContainer>());

			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			// MVC Object factories for view models that require IOC, and all controllers
			ModelBinders.Binders.Add(typeof(UserSummary),new UserSummaryModelBinder());
			ModelBinders.Binders.Add(typeof(SettingsSummary),new SettingsSummaryModelBinder());
			ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			// For the jQuery ajax file manager
			routes.MapLowercaseRoute(
				"FileFolder",
				"Files/Folder/{dir}",
				new { controller = "Files", action = "Folder", dir = UrlParameter.Optional }
			);

			// The default way of getting to a page: "/wiki/123/page-title"
			routes.MapLowercaseRoute(
				"Wiki",
				"Wiki/{id}/{title}",
				new { controller = "Wiki", action = "Index", title = UrlParameter.Optional }
			);

			// Don't lowercase pages that use Base64
			routes.MapRoute(
				"Pages",
				"pages/byuser/{id}/{encoded}",
				new { controller = "Pages", action = "ByUser", title = UrlParameter.Optional }
			);

			// Default
			routes.MapLowercaseRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		/// <summary>
		/// Initializes the Structuremap IoC containers for the Services, Configuration and IRepository,
		/// and registering the defaults for each.
		/// No settings are loaded from the database in this method, or config file settings loaded. The
		/// <see cref="ApplicationSettings.UserManagerType"/> is used.
		/// </summary>
		/// <param name="config">If null, then a new per-thread/http request <see cref="RoadkillSettings"/> class is used for the configuration.</param>
		/// <param name="context">If null, then a new per-thread/http request <see cref="RoadkillContext"/> is used.</param>
		/// <param name="repository">If null, then a new per-thread/http request <see cref="NHibernateRepository"/> is used.</param>
		public static void SetupIoC(IConfigurationContainer config = null, IRepository repository = null, IRoadkillContext context = null)
		{
			if (config != null && config.ApplicationSettings == null)
				throw new IoCException("The ApplicationSettings of the configuration settings is null - " + ObjectFactory.WhatDoIHave(), null);

			ObjectFactory.Initialize(x =>
			{
				x.Scan(scanner =>
				{
					scanner.AddAllTypesOf<IConfigurationContainer>();
					scanner.AddAllTypesOf<ControllerBase>();
					scanner.AddAllTypesOf<ServiceBase>();
					scanner.AddAllTypesOf<IRoadkillContext>();
					scanner.AddAllTypesOf<IRepository>();
					scanner.AddAllTypesOf<MarkupConverter>();
					scanner.AddAllTypesOf<CustomTokenParser>();
					scanner.WithDefaultConventions();
				});

				// Models that require IConfigurationContainer or IRoadkillContext in their constructors
				x.For<UserSummary>().Use<UserSummary>();
				x.For<SettingsSummary>().Use<SettingsSummary>();

				// The order of the calls is important as the default concrete types have a dependency order:
				// - IRepository relies on IConfigurationContainer
				// - IRoadkillContext relies on UserManager, which relies on IRepository

				if (config == null)
				{
					x.For<IConfigurationContainer>().HybridHttpOrThreadLocalScoped().Use<RoadkillSettings>();
				}
				else
				{
					x.For<IConfigurationContainer>().HybridHttpOrThreadLocalScoped().Use(config);
				}

				if (repository == null)
				{
					x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use<NHibernateRepository>();
				}
				else
				{
					x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(repository);
				}

				if (context == null)
				{
					x.For<IRoadkillContext>().HybridHttpOrThreadLocalScoped().Use<RoadkillContext>();
				}
				else
				{
					x.For<IRoadkillContext>().HybridHttpOrThreadLocalScoped().Use(context);
				}

				x.For<IActiveDirectoryService>().HybridHttpOrThreadLocalScoped().Use<DefaultActiveDirectoryService>();				
			});

			ObjectFactory.Configure(x =>
			{
				x.IncludeConfigurationFromConfigFile = true;

				if (config == null)
				{
					config = ObjectFactory.GetInstance<IConfigurationContainer>();
				}

				string userManagerTypeName = config.ApplicationSettings.UserManagerType;
				if (string.IsNullOrEmpty(userManagerTypeName))
				{
					if (config.ApplicationSettings.UseWindowsAuthentication)
					{
						x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use<ActiveDirectoryUserManager>();
					}
					else
					{
						x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use<SqlUserManager>();
					}
				}
				else
				{
					// Load UserManager type from config
					x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use(LoadFromType(userManagerTypeName));
				}
			});
		}
		
		private static UserManager LoadFromType(string typeName)
		{
			// Attempt to load the type
			Type userManagerType = typeof(UserManager);
			Type reflectedType = Type.GetType(typeName);
			
			if (reflectedType.IsSubclassOf(userManagerType))
			{
				return (UserManager)reflectedType.Assembly.CreateInstance(reflectedType.FullName);
			}
			else
			{
				throw new SecurityException(null, "The type {0} specified in the userManagerType web.config setting is not an instance of a UserManager class", typeName);
			}
		}
	}
}
