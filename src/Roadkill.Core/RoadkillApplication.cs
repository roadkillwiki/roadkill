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
using Roadkill.Core.MVC;
using System.IO;
using Roadkill.Core.DI;

namespace Roadkill.Core
{
	/// <summary>
	/// The entry point application (Global.asax) for Roadkill.
	/// </summary>
	public class RoadkillApplication : HttpApplication
	{
		public static string BundleCssFilename { get; private set; }
		public static string BundleJsFilename { get; private set; }

		protected void Application_Start()
		{
			// Get the settings from the web.config
			ConfigReader configReader = ConfigReaderFactory.GetConfigReader();
			ApplicationSettings applicationSettings = configReader.GetApplicationSettings();

			// Configure StructureMap dependencies
			DependencyManager iocSetup = new DependencyManager(applicationSettings);
			iocSetup.Configure();
			iocSetup.ConfigureMvc();

			// Logging
			Log.ConfigureLogging(applicationSettings);

			// All other routes
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			// Filters
			GlobalFilters.Filters.Add(new HandleErrorAttribute());

			// CSS/JS Bundles
			RegisterBundles();

			// Custom view engine registration (to add new search paths)
			RegisterViewEngine();

			Log.Information("Application started");
		}

		protected void Application_Error()
		{
			// Log ASP.NET errors (404, 500)
			HttpException exception = new HttpException(null, HttpContext.Current.Server.GetLastError());
			Log.Error("An ASP.NET based error occurred - ({0}) - {1}", 
						exception.GetHttpCode(),
						exception.ToString());
		}

		protected void Application_EndRequest(object sender, EventArgs e)
		{
			try
			{
				// Finish the current Unit of Work
				RepositoryManager.DisposeRepository();
			}
			catch (Exception ex)
			{
				Log.Error("Error calling IoCSetup.DisposeRepository: {0}", ex.ToString());
			}
		}

		private void RegisterBundles()
		{
			BundleCssFilename = string.Format("roadkill{0}.css", ApplicationSettings.ProductVersion);
			BundleJsFilename = string.Format("roadkill{0}.js", ApplicationSettings.ProductVersion);

			// Bundle all CSS/JS files into a single file		
			StyleBundle cssBundle = new StyleBundle("~/Assets/CSS/" + BundleCssFilename);
			cssBundle.IncludeDirectory("~/Assets/CSS/", "*.css");

			ScriptBundle defaultJsBundle = new ScriptBundle("~/Assets/Scripts/" + BundleJsFilename);
			defaultJsBundle.Include("~/Assets/Scripts/*.js");
			defaultJsBundle.Include("~/Assets/Scripts/jquery/*.js");
			defaultJsBundle.Include("~/Assets/Scripts/roadkill/*.js");
			defaultJsBundle.Include("~/Assets/Scripts/roadkill/filemanager/*.js");

			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(defaultJsBundle);
		}

		private void RegisterViewEngine()
		{
			// Add a search path for /Dialogs, via a custom view engine.
			ViewEngines.Engines.Clear();

			ExtendedRazorViewEngine engine = new ExtendedRazorViewEngine();
			engine.AddPartialViewLocationFormat("~/Views/Shared/Dialogs/{0}.cshtml");

			ViewEngines.Engines.Add(engine);
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
	}
}
