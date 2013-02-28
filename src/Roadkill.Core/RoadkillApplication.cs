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
			// Configure StructureMap dependencies
			IoCSetup iocSetup = new IoCSetup();
			iocSetup.Run();

			// Register the ~/attachments/ route
			AttachmentRouteHandler.Register(ObjectFactory.GetInstance<IConfigurationContainer>());

			// All other routes
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			// Some view models are new'd up by a custom object factory so dependencies are injected into them
			ModelBinders.Binders.Add(typeof(UserSummary),new UserSummaryModelBinder());
			ModelBinders.Binders.Add(typeof(SettingsSummary),new SettingsSummaryModelBinder());

			// *All* Roadkill MVC controllers are new'd up by a controller factory so dependencies are injected into them
			ControllerBuilder.Current.SetControllerFactory(new ControllerFactory());
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

		protected void Application_EndRequest(object sender, EventArgs e)
		{
			IoCSetup.DisposeRepository();
		}
	}
}
