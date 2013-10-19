using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Roadkill.Core.Converters;
using Roadkill.Core.Localization;
using Roadkill.Core.Configuration;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Text;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides functionality that is common through the site.
	/// </summary>
	[OptionalAuthorization]
	public class HomeController : ControllerBase
	{
		public PageService PageService { get; private set; }
		private SearchService _searchService;
		private MarkupConverter _markupConverter;

		public HomeController(ApplicationSettings settings, UserManagerBase userManager, MarkupConverter markupConverter,
			PageService pageService, SearchService searchService, IUserContext context, SettingsService settingsService)
			: base(settings, userManager, context, settingsService) 
		{
			_markupConverter = markupConverter;
			_searchService = searchService;
			PageService = pageService;
		}

		/// <summary>
		/// Display the homepage/mainpage. If no page has been tagged with the 'homepage' tag,
		/// then a dummy PageSummary is put in its place.
		/// </summary>
		[BrowserCache]
		public ActionResult Index()
		{
			// Get the first locked homepage
			PageSummary summary = PageService.FindHomePage();

			if (summary == null)
			{
				summary = new PageSummary();
				summary.Title = SiteStrings.NoMainPage_Title;
				summary.Content = SiteStrings.NoMainPage_Label;
				summary.ContentAsHtml = _markupConverter.ToHtml(SiteStrings.NoMainPage_Label).Html;
				summary.CreatedBy = "";
				summary.CreatedOn = DateTime.UtcNow;
				summary.RawTags = "homepage";
				summary.ModifiedOn = DateTime.UtcNow;
				summary.ModifiedBy = "";
			}

			return View(summary);
		}

		/// <summary>
		/// Searches the lucene index using the search string provided.
		/// </summary>
		public ActionResult Search(string q)
		{
			ViewData["search"] = q;

			List<SearchResult> results = _searchService.Search(q).ToList();
			return View(results);
		}

		/// <summary>
		/// Returns Javascript 'constants' for the site.
		/// </summary>
		/// <param name="version">This is sent by the views to ensure new versions of Roadkill have this JS file cleared from the cache.</param>
		[CacheContentType(Duration = 86400 * 30, ContentType = "application/javascript")] // 30 days
		public ActionResult GlobalJsVars(string version)
		{
			return View();
		}

		/// <summary>
		/// Displays the left side menu view, including new page/settings if logged in.
		/// </summary>
		public ActionResult LeftMenu()
		{
			return Content(PageService.GetMenu(Context));
		}
	}
}