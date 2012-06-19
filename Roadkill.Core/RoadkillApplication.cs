using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Roadkill.Core.Search;

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
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
			SetupNHibernate();
			RecreateSearchIndex();
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
		/// Attempts to recreated the lucene search index, which becomes out of sync when the app shuts down.
		/// </summary>
		public virtual void RecreateSearchIndex()
		{
			SearchManager.Current.CreateIndex();
		}
	}
}
