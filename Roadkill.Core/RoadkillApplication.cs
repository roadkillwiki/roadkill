using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Roadkill.Core
{
	public class RoadkillApplication : HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			// Sets Index() as the default action when no action is provided
			//routes.MapRoute(
			//    "ControllerDefault",
			//    "{controller}/{id}",
			//    new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			//);

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			Page.Configure(RoadkillSettings.ConnectionString);
		}
	}
}
