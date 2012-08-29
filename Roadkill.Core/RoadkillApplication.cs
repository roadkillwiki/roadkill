using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Roadkill.Core.Search;
using System;
using System.IO;
using Roadkill.Core.Files;

namespace Roadkill.Core
{
	/// <summary>
	/// The entry point application (Global.asax) for Roadkill.
	/// </summary>
	public class RoadkillApplication : HttpApplication
	{
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

		protected void Application_Start()
		{
			AttachmentRouteHandler.Register();
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			ConfigureForPlasma();
			SetupNHibernate();
		}

		/// <summary>
		/// Initializes the NHibernate sessionfactory, creating the schema if necessary.
		/// </summary>
		/// <remarks>This method is virtual for mocking</remarks>
		public virtual void SetupNHibernate()
		{
			if (RoadkillSettings.Installed)
			{
				NHibernateRepository.Current.Configure(RoadkillSettings.DatabaseType, RoadkillSettings.ConnectionString, false, RoadkillSettings.CachedEnabled);
			}
		}

		/// <summary>
		/// Switches the database and sets the instance to installed for the Plasma tests.
		/// </summary>
		private void ConfigureForPlasma()
		{
#if RELEASE
			RoadkillSection.Current.Installed = true;
			RoadkillSection.Current.DatabaseType = "SQLite";
			RoadkillSettings.ConnectionString = @"Data Source=|DataDirectory|\roadkill.plasma.sqlite";
#endif
		}
	}
}
