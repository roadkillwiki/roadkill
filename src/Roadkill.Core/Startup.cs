using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Owin;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc;

namespace Roadkill.Core
{
	public class Startup
	{
		// See LocatorStartup for lots of pre-startup IoC setup that's performed.

		public void Configuration(IAppBuilder app)
		{
			// Filters
			GlobalFilters.Filters.Add(new HandleErrorAttribute());

			// Areas are used for Site settings (for a cleaner view structure)
			// This should be called before the other routes, for some reason.
			AreaRegistration.RegisterAllAreas();

			// Register routes
			Routing.RegisterApi(GlobalConfiguration.Configuration);
			Routing.Register(RouteTable.Routes);

			// Custom view engine registration (to add directory search paths for Theme views)
			ExtendedRazorViewEngine.Register();

			app.UseWebApi(new HttpConfiguration());

			Log.Information("Application started");
		}
	}
}
