using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Roadkill.Core.Search;
using System;
using System.IO;
using Roadkill.Core.Files;
using StructureMap;
using Roadkill.Core.Domain;
using StructureMap.Pipeline;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using System.Reflection;

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
			AttachmentRouteHandler.Register();
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

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
		/// The configuration is only used for its ApplicationSettings object.
		/// </summary>
		public static void SetupIoC(IConfigurationContainer config = null, IRepository repository = null, IRoadkillContext context = null)
		{
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
					scanner.WithDefaultConventions();
				});

				// The order of the calls is important as the default concrete types have a dependency order:
				// - Repository relies on RoadkillSettings
				// - Container relies on Repository
				// - Context relies on ServiceContainer

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

			ObjectFactory.Configure(x =>
			{
				x.IncludeConfigurationFromConfigFile = true;
			});
		}
		
		public UserManager LoadFromType(string typeName)
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
