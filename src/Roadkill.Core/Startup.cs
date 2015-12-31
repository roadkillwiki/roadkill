using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Owin;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.Setup;
using Roadkill.Core.Owin;
using Roadkill.Core.Services;

namespace Roadkill.Core
{
	public class Startup
	{
		// See LocatorStartup for lots of pre-startup IoC setup that's performed.

		public void Configuration(IAppBuilder app)
		{
			var appSettings = LocatorStartup.Locator.GetInstance<ApplicationSettings>();
			app.Use<InstallCheckMiddleware>(appSettings);

			// Register the "/Attachments/" route handler. This needs to be called before the other routing setup.
			if (appSettings.Installed)
			{
				// InstallService.Install also performs this
				var fileService = LocatorStartup.Locator.GetInstance<IFileService>();
				AttachmentRouteHandler.RegisterRoute(appSettings, RouteTable.Routes, fileService);
			}

			// Filters
			GlobalFilters.Filters.Add(new HandleErrorAttribute());

			// Areas are used for Site settings (for a cleaner view structure)
			// This should be called before the other routes, for some reason.
			AreaRegistration.RegisterAllAreas();

			// Register WebApi/MVC routes, including Swashbuckle
			Routing.RegisterWebApi(GlobalConfiguration.Configuration);
			Routing.Register(RouteTable.Routes);

			// Custom view engine registration (to add directory search paths for Theme views)
			ExtendedRazorViewEngine.Register();

			// WebApi 
			app.UseWebApi(new HttpConfiguration());

			Log.Information("Roadkill started");
		}
	}
}
