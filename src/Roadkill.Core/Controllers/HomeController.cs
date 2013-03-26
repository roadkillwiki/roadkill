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
using System.Web.UI;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides functionality that is common through the site.
	/// </summary>
	[OptionalAuthorization]
	public class HomeController : ControllerBase
	{
		public PageManager PageManager { get; private set; }
		private SearchManager _searchManager;
		private MarkupConverter _markupConverter;

		public HomeController(IConfigurationContainer configuration, UserManager userManager, MarkupConverter markupConverter,
			PageManager pageManager, SearchManager searchManager, IRoadkillContext context)
			: base(configuration, userManager, context) 
		{
			_markupConverter = markupConverter;
			PageManager = pageManager;
			_searchManager = searchManager;
		}

		/// <summary>
		/// Display the homepage/mainpage. If no page has been tagged with the 'homepage' tag,
		/// then a dummy PageSummary is put in its place.
		/// </summary>
		[BrowserCache]
		public ActionResult Index()
		{
			// Get the first locked homepage
			PageSummary summary = PageManager.FindHomePage();

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