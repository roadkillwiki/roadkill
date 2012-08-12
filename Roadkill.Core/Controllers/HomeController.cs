using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.Web.Management;
using System.Data.SqlClient;
using Roadkill.Core.Converters;
using Roadkill.Core.Search;
using Roadkill.Core.Localization.Resx;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides functionality that is common through the site.
	/// </summary>
	public class HomeController : ControllerBase
	{
		/// <summary>
		/// Display the homepage/mainpage. If no page has been tagged with the 'homepage' tag,
		/// then a dummy PageSummary is put in its place.
		/// </summary>
		public ActionResult Index()
		{
			PageManager manager = new PageManager();

			// Get the first locked homepage
			PageSummary summary = manager.FindByTag("homepage").FirstOrDefault(h => h.IsLocked);
			if (summary == null)
			{
				// Look for a none-locked page as a fallback
				summary = manager.FindByTag("homepage").FirstOrDefault();
			}

			if (summary == null)
			{
				summary = new PageSummary();
				summary.Title = SiteStrings.NoMainPage_Title;
				summary.Content = SiteStrings.NoMainPage_Label;
				summary.CreatedBy = "";
				summary.CreatedOn = DateTime.Now;
				summary.Tags = "homepage";
				summary.ModifiedOn = DateTime.Now;
				summary.ModifiedBy = "";
			}

			RoadkillContext.Current.Page = summary;
			return View(summary);
		}

		/// <summary>
		/// Searches the lucene index using the search string provided.
		/// </summary>
		public ActionResult Search(string q)
		{
			ViewData["search"] = q;

			List<SearchResult> results = SearchManager.Current.SearchIndex(q);
			return View(results);
		}

		/// <summary>
		/// Returns Javascript 'constants' for the site. If the user is logged in, 
		/// additional variables are returned that are used by the edit page.
		/// </summary>
		[OutputCache(Duration=3600)]
		public ActionResult GlobalJsVars()
		{
			Response.ContentType = "text/javascript";
			return PartialView();
		}
	}
}
