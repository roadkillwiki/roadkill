using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Owin;
using Roadkill.Core.Configuration;
using Roadkill.Core.DI;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc;

namespace Roadkill.Core
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			// Get the settings from the web.config
			ConfigReaderWriter configReader = new FullTrustConfigReaderWriter("");
			ApplicationSettings applicationSettings = configReader.GetApplicationSettings();

			// Configure StructureMap dependencies
			var iocSetup = new DependencyManager(applicationSettings);
			iocSetup.Configure();
			iocSetup.ConfigureMvc();

			// Logging
			Log.ConfigureLogging(applicationSettings);

			// Filters
			GlobalFilters.Filters.Add(new HandleErrorAttribute());

			// Areas are used for:
			// - Site settings (for a cleaner view structure)
			// This should be called before the other routes, for some reason.
			AreaRegistration.RegisterAllAreas();

			// Register routes and JS/CSS bundles
			Routing.RegisterApi(GlobalConfiguration.Configuration);
			Routing.Register(RouteTable.Routes);

			// Custom view engine registration (to add directory search paths for Theme views)
			ExtendedRazorViewEngine.Register();

			app.UseWebApi(new HttpConfiguration());

			Log.Information("Application started");
		}
	}
}
