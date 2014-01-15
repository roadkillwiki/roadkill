using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Roadkill.Core.Mvc
{
	public class Routing
	{
		public static void Register(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("favicon.ico");

			RegisterSpecialRoutes(routes);

			// For the jQuery ajax file manager
			routes.MapLowercaseRoute(
				"FileFolder",
				"Files/Folder/{dir}",
				new { controller = "Files", action = "Folder", dir = UrlParameter.Optional }
			);

			// 404 error
			routes.MapLowercaseRoute(
				"NotFound",
				"wiki/notfound",
				new { controller = "Wiki", action = "NotFound", id = UrlParameter.Optional }
			);

			// 500 error
			routes.MapLowercaseRoute(
				"ServerError",
				"wiki/servererror",
				new { controller = "Wiki", action = "ServerError", id = UrlParameter.Optional }
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

			// Be explicit for the help controller, as it gets confused with the WebAPI one
			routes.MapRoute(
				"Roadkill.Core.Mvc.Controllers.HelpController",
				"help/{action}/{id}",
				new { controller = "Help", action = "Index", id = UrlParameter.Optional },
				null,
				new string[] { "Roadkill.Core.Mvc.Controllers" }
			);

			// Default
			routes.MapLowercaseRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		private static void RegisterSpecialRoutes(RouteCollection routes)
		{
			// /Wiki/Special:{id} urls
			routes.MapRoute(
				"SpecialPages",
				"Wiki/Special:{id}",
				new { controller = "SpecialPages", action = "Index" }
			);

			// /Wiki/Help:About
			routes.MapRoute(
				"Help:About",
				"Wiki/Help:About",
				new { controller = "Help", action = "About" },
				null,
				new string[] { "Roadkill.Core.Mvc.Controllers" }
			);

			// /Wiki/Help:Cheatsheet
			routes.MapRoute(
				"Help:CheatSheet",
				"Wiki/Help:Cheatsheet",
				new { controller = "Help", action = "Index" },
				null,
				new string[] { "Roadkill.Core.Mvc.Controllers" }
			);
		}

		public static void RegisterApi(System.Web.Http.HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();

			// Adds support for the webapi 1 style methods, e.g. /api/Users/ 
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			config.EnsureInitialized();
		}
	}
}
