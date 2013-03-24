using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System;
using Roadkill.Core.Files;
using StructureMap;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using System.Web.Optimization;

namespace Roadkill.Core
{
	/// <summary>
	/// The entry point application (Global.asax) for Roadkill.
	/// </summary>
	public class RoadkillApplication : HttpApplication
	{
		public static DateTime StartTime { get; private set; }

		protected void Application_Start()
		{
			Log.UseUdpLogging();
			Log.UseXmlLogging();

			// Configure StructureMap dependencies
			IoCSetup iocSetup = new IoCSetup();
			iocSetup.Run();
			iocSetup.RegisterMvcFactoriesAndRouteHandlers();

			// All other routes
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			// Filters
			GlobalFilters.Filters.Add(new HandleErrorAttribute());

			// Used for caching of views, when needed
			StartTime = DateTime.UtcNow;

			// Bundle all CSS/JS files into a single file
			StyleBundle cssBundle = new StyleBundle("~/Assets/CSS/bundle.css");
			cssBundle.IncludeDirectory("~/Assets/CSS/","*.css");

			// Ignore scripts that require a login
			BundleTable.Bundles.IgnoreList.Ignore("roadkill.edit.js");
			BundleTable.Bundles.IgnoreList.Ignore("roadkill.settings.js");
			BundleTable.Bundles.IgnoreList.Ignore("roadkill.files.js");
			BundleTable.Bundles.IgnoreList.Ignore("roadkill.wysiwyg.js");

			ScriptBundle jsBundle = new ScriptBundle("~/Assets/Scripts/bundle.js");
			jsBundle.Include("~/Assets/Scripts/*.js");
			
			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(jsBundle);
			BundleTable.EnableOptimizations = true;
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("favicon.ico");

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

		protected void Application_EndRequest(object sender, EventArgs e)
		{
			try
			{
				IoCSetup.DisposeRepository();
			}
			catch (Exception ex)
			{
				Log.Error("Error calling IoCSetup.DisposeRepository: {0}", ex.ToString());
			}
		}
	}
}
