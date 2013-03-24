using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Roadkill.Core.Converters;
using Roadkill.Core.Search;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Configuration;
using System.Diagnostics;
using System.Web;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides functionality that is common through the site.
	/// </summary>
	[OptionalAuthorization]
	public class HomeController : ControllerBase
	{
		private PageManager _pageManager;
		private SearchManager _searchManager;
		private MarkupConverter _markupConverter;

		public HomeController(IConfigurationContainer configuration, UserManager userManager, MarkupConverter markupConverter,
			PageManager pageManager, SearchManager searchManager, IRoadkillContext context)
			: base(configuration, userManager, context) 
		{
			_markupConverter = markupConverter;
			_pageManager = pageManager;
			_searchManager = searchManager;
		}

		/// <summary>
		/// Display the homepage/mainpage. If no page has been tagged with the 'homepage' tag,
		/// then a dummy PageSummary is put in its place.
		/// </summary>
		public ActionResult Index()
		{
			// Get the first locked homepage
			PageSummary summary = _pageManager.FindHomePage();

			if (summary == null)
			{
				summary = new PageSummary();
				summary.Title = SiteStrings.NoMainPage_Title;
				summary.Content = SiteStrings.NoMainPage_Label;
				summary.ContentAsHtml = _markupConverter.ToHtml(SiteStrings.NoMainPage_Label);
				summary.CreatedBy = "";
				summary.CreatedOn = DateTime.Now;
				summary.RawTags = "homepage";
				summary.ModifiedOn = DateTime.Now;
				summary.ModifiedBy = "";
			}

			Context.Page = summary;

			if (!Context.IsLoggedIn)
			{
				Response.Cache.SetCacheability(HttpCacheability.Public);
				Response.Cache.SetExpires(DateTime.Now.AddSeconds(2));
				Response.Cache.SetLastModified(summary.ModifiedOn.ToUniversalTime());
				Response.StatusCode = HttpContext.ApplicationInstance.Context.GetStatusCodeForCache(summary.ModifiedOn.ToUniversalTime());

				if (Response.StatusCode == 304)
					return new HttpStatusCodeResult(304, "Not Modified");
			}

			return View(summary);
		}

		/// <summary>
		/// Searches the lucene index using the search string provided.
		/// </summary>
		public ActionResult Search(string q)
		{
			ViewData["search"] = q;

			List<SearchResult> results = _searchManager.SearchIndex(q).ToList();
			return View(results);
		}

		/// <summary>
		/// Returns Javascript 'constants' for the site. If the user is logged in, 
		/// additional variables are returned that are used by the edit page.
		/// </summary>
		public ActionResult GlobalJsVars()
		{
			Response.ContentType = "text/javascript";
			return PartialView();
		}
	}
}