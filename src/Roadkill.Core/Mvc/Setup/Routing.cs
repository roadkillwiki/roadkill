using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Roadkill.Core.Mvc.Setup
{
	/// <summary>
	/// The Roadkill MVC routes. Generated controller and action names are lowercased (like the previous LowercaseRoute),
	/// but not the other route values (e.g. the base64 username for pages/byuser).
	/// </summary>
	public static class Routing
	{
		/// <summary>
		/// The name of the route constraint/transformer that lowercases controller and action names in generated urls.
		/// </summary>
		public static readonly string LowercaseTransformerName = "lowercase";

		public static void MapRoadkillRoutes(this IEndpointRouteBuilder routes)
		{
			// The REST api (attribute routed)
			routes.MapControllers();

			// /Wiki/Special:{id} urls
			routes.MapControllerRoute("SpecialPages", "wiki/special:{id}", new { controller = "SpecialPages", action = "Index" });

			// /Wiki/Help:About and /Wiki/Help:Cheatsheet
			routes.MapControllerRoute("Help:About", "wiki/help:about", new { controller = "Help", action = "About" });
			routes.MapControllerRoute("Help:CheatSheet", "wiki/help:cheatsheet", new { controller = "Help", action = "Index" });

			// 404 and 500 errors
			routes.MapControllerRoute("NotFound", "wiki/notfound", new { controller = "Wiki", action = "NotFound" });
			routes.MapControllerRoute("ServerError", "wiki/servererror", new { controller = "Wiki", action = "ServerError" });

			// The default way of getting to a page: "/wiki/123/page-title"
			routes.MapControllerRoute("Wiki", "wiki/{id}/{title?}", new { controller = "Wiki", action = "Index" });

			// Pages by user use Base64, so these values aren't lowercased
			routes.MapControllerRoute("Pages", "pages/byuser/{id}/{encoded?}", new { controller = "Pages", action = "ByUser" });

			// Site settings area: "/Settings" and "/SiteSettings/{controller}/{action}/{id}"
			routes.MapAreaControllerRoute("SiteSettings_Default", "SiteSettings", "settings", new { controller = "Settings", action = "Index" });
			routes.MapAreaControllerRoute("SiteSettings_Controller", "SiteSettings",
				"sitesettings/{controller:lowercase=Settings}/{action:lowercase=Index}/{id?}");

			// Default
			routes.MapControllerRoute("Default", "{controller:lowercase=Home}/{action:lowercase=Index}/{id?}");
		}
	}

	/// <summary>
	/// Lowercases the controller and action names in generated urls.
	/// </summary>
	public class LowercaseParameterTransformer : IOutboundParameterTransformer
	{
		public string TransformOutbound(object value)
		{
			return value?.ToString()?.ToLowerInvariant();
		}
	}
}
