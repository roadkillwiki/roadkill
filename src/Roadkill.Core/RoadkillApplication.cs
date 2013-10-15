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
using Roadkill.Core.Mvc;
using System.IO;
using Roadkill.Core.DI;
using Roadkill.Core.Mvc;

namespace Roadkill.Core
{
	/// <summary>
	/// The entry point application (Global.asax) for Roadkill.
	/// </summary>
	public class RoadkillApplication : HttpApplication
	{
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

			// Filters
			GlobalFilters.Filters.Add(new HandleErrorAttribute());

			// Areas aren't used but kept in for future usage
			AreaRegistration.RegisterAllAreas();

			// Register routers and JS/CSS bundles
			Routing.Register(RouteTable.Routes);
			Bundles.Register();

			// Custom view engine registration (to add new search paths)
			ExtendedRazorViewEngine.Register();

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
	}
}
