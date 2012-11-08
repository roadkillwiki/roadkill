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

namespace Roadkill.Core
{
	/// <summary>
	/// The entry point application (Global.asax) for Roadkill.
	/// </summary>
	public class RoadkillApplication : HttpApplication
	{
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

		protected void Application_Start()
		{
			SetupIoC();
			AttachmentRouteHandler.Register();
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
		}

		/// <summary>
		/// Initializes the Structuremap IoC containers for the Services and Configuration.
		/// </summary>
		public virtual void SetupIoC()
		{
			ObjectFactory.Initialize(x =>
			{
				x.Scan(scanner =>
				{
					scanner.AddAllTypesOf<IConfigurationContainer>();
					scanner.AddAllTypesOf<IServiceContainer>();
					scanner.AddAllTypesOf<IRoadkillContext>();
					scanner.AddAllTypesOf<IRepository>();
					scanner.WithDefaultConventions();
				});

				//x.For<IRepository>().LifecycleIs(new HybridSessionLifecycle()).Use<NHibernateRepository>();
				//x.For<IConfigurationContainer>().Use<RoadkillSettings>();
				//x.For<IServiceContainer>().LifecycleIs(new HybridSessionLifecycle());
				//x.For<IRoadkillContext>().LifecycleIs(new HybridSessionLifecycle());

				x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use<NHibernateRepository>();
				x.For<IConfigurationContainer>().HybridHttpOrThreadLocalScoped().Use<RoadkillSettings>();
				x.For<IServiceContainer>().HybridHttpOrThreadLocalScoped().Use<ServiceContainer>();
				x.For<IRoadkillContext>().HybridHttpOrThreadLocalScoped().Use<RoadkillContext>();
			});

			ObjectFactory.Configure(x =>
			{
				x.IncludeConfigurationFromConfigFile = true;
			});
		}
	}
}
