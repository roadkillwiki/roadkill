using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System;
using Roadkill.Core.Attachments;
using StructureMap;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using System.Web.Optimization;
using Roadkill.Core.Logging;

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
			// Get the settings from the web.config
			ConfigFileManager configManager = new ConfigFileManager();
			ApplicationSettings applicationSettings = configManager.GetApplicationSettings();

			// Configure StructureMap dependencies
			DependencyContainer iocSetup = new DependencyContainer(applicationSettings);
			iocSetup.RegisterTypes();
			iocSetup.RegisterMvcFactoriesAndRouteHandlers();

			// All other routes
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			// Filters
			GlobalFilters.Filters.Add(new HandleErrorAttribute());

			// Used for caching of views (not currently implemented, can be removed)
			StartTime = DateTime.UtcNow;

			// Bundle all CSS/JS files into a single file
			StyleBundle cssBundle = new StyleBundle("~/Assets/CSS/bundle.css");
			cssBundle.IncludeDirectory("~/Assets/CSS/","*.css");

			// Ignore scripts that require a login
			BundleTable.Bundles.IgnoreList.Ignore("adminsetup.js");
			BundleTable.Bundles.IgnoreList.Ignore("editpage.js");
			BundleTable.Bundles.IgnoreList.Ignore("wysiwygeditor.js");
			BundleTable.Bundles.IgnoreList.Ignore("fileManager.js");
			BundleTable.Bundles.IgnoreList.Ignore("constants.js"); // a dummy for Typescript

			ScriptBundle jsBundle = new ScriptBundle("~/Assets/Scripts/bundle.js");
			jsBundle.Include("~/Assets/Scripts/*.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/*.js");
			
			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(jsBundle);
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
				DependencyContainer.DisposeRepository();
			}
			catch (Exception ex)
			{
				Log.Error("Error calling IoCSetup.DisposeRepository: {0}", ex.ToString());
			}
		}
	}
}
