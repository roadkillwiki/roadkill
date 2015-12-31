using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Roadkill.Core.Mvc.WebApi;
using Swashbuckle.Application;

namespace Roadkill.Core.Mvc.Setup
{
	public class Routing
	{
		public static void Register(RouteCollection routes)
		{
			// Additional routing can be found in SiteSettingsAreaRegistration

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

		public static void RegisterWebApi(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();

			// Adds support for the webapi 1 style methods, e.g. /api/Users/ 
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			RegisterSwashBuckle(config);

			config.EnsureInitialized();
		}

		private static void RegisterSwashBuckle(HttpConfiguration config)
		{
			var applyApiKeySecurity = new SwashbuckleApplyApiKeySecurity(
				key: ApiKeyAuthorizeAttribute.APIKEY_HEADER_KEY,
				name: ApiKeyAuthorizeAttribute.APIKEY_HEADER_KEY,
				description: "API key",
				@in: "header"
				);

			config
				.EnableSwagger(c =>
				{
					c.ApiKey("apiKey")
						.Description("API Key Authentication")
						.Name("ApiKey")
						.In("header");

					c.SingleApiVersion("3.0", "Roadkill Web API");
					applyApiKeySecurity.Apply(c);
				})
				.EnableSwaggerUi();
		}
	}
}
