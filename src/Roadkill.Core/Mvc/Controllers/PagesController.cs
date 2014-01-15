using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Roadkill.Core.Diff;
using Roadkill.Core.Converters;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using System.Web;
using Roadkill.Core.Text;
using Roadkill.Core.Extensions;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides all page related functionality, including editing and viewing pages.
	/// </summary>
	[HandleError]
	[OptionalAuthorization]
	public class PagesController : ControllerBase
	{
		private SettingsService _settingsService;
		private IPageService _pageService;
		private SearchService _searchService;
		private PageHistoryService _historyService;

		public PagesController(ApplicationSettings settings, UserServiceBase userManager,
			SettingsService settingsService, IPageService pageService, SearchService searchService,
			PageHistoryService historyService, IUserContext context)
			: base(settings, userManager, context, settingsService)
		{
			_settingsService = settingsService;
			_pageService = pageService;
			_searchService = searchService;
			_historyService = historyService;
		}

		/// <summary>
		/// Displays a list of all page titles and ids in Roadkill.
		/// </summary>
		/// <returns>An <see cref="IEnumerable{PageViewModel}"/> as the model.</returns>
		[BrowserCache]
		public ActionResult AllPages()
		{
			return View(_pageService.AllPages());
		}

		/// <summary>
		/// Displays all tags (categories if you prefer that term) in Roadkill.
		/// </summary>
		/// <returns>An <see cref="IEnumerable{TagViewModel}"/> as the model.</returns>
		[BrowserCache]
		public ActionResult AllTags()
		{
			return View(_pageService.AllTags().OrderBy(x => x.Name));
		}

		/// <summary>
		/// Returns all tags in the system as a JSON string.
		/// </summary>
		/// <param name="term">The jQuery UI autocomplete filter passed in, e.g. when "ho" is typed for homepage.</param>
		/// <returns>A string array of tags.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult AllTagsAsJson(string term = "")
		{
			IEnumerable<TagViewModel> tags = _pageService.AllTags();
			if (!string.IsNullOrEmpty(term))
				tags = tags.Where(x => x.Name.StartsWith(term, StringComparison.InvariantCultureIgnoreCase));

			IEnumerable<string> tagsJson = tags.Select(t => t.Name).ToList();
			return Json(tagsJson, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Displays all pages for a particular user.
		/// </summary>
		/// <param name="id">The username</param>
		/// <param name="encoded">Whether the username paramter is Base64 encoded.</param>
		/// <returns>An <see cref="IEnumerable{PageViewModel}"/> as the model.</returns>
		public ActionResult ByUser(string id, bool? encoded)
		{
			// Usernames are base64 encoded by roadkill (to cater for usernames like domain\john).
			// However the URL also supports humanly-readable format, e.g. /ByUser/chris
			if (encoded == true)
			{
				id = id.FromBase64();
			}

			ViewData["Username"] = id;

			return View(_pageService.AllPagesCreatedBy(id));
		}

		/// <summary>
		/// Deletes a wiki page.
		/// </summary>
		/// <param name="id">The id of the page to delete.</param>
		/// <returns>Redirects to AllPages action.</returns>
		/// <remarks>This action requires admin rights.</remarks>
		[AdminRequired]
		public ActionResult Delete(int id)
		{
			_pageService.DeletePage(id);

			return RedirectToAction("AllPages");
		}

		/// <summary>
		/// Displays the edit View for the page provided in the id.
		/// </summary>
		/// <param name="id">The ID of the page to edit.</param>
		/// <returns>An filled <see cref="PageViewModel"/> as the model. If the page id cannot be found, the action
		/// redirects to the New page.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult Edit(int id)
		{
			PageViewModel model = _pageService.GetById(id, true);

			if (model != null)
			{
				if (model.IsLocked && !Context.IsAdmin)
					return new HttpStatusCodeResult(403, string.Format("The page '{0}' can only be edited by administrators.", model.Title));

				model.AllTags = _pageService.AllTags().ToList();

				return View("Edit", model);
			}
			else
			{
				return RedirectToAction("New");
			}
		}

		/// <summary>
		/// Saves all POST'd data for a page edit to the database.
		/// </summary>
		/// <param name="model">A filled <see cref="PageViewModel"/> containing the new data.</param>
		/// <returns>Redirects to /Wiki/{id} using the passed in <see cref="PageViewModel.Id"/>.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(PageViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Edit", model);

			_pageService.UpdatePage(model);

			return RedirectToAction("Index", "Wiki", new { id = model.Id });
		}

		/// <summary>
		/// This action is for JSON calls only. Displays a HTML preview for the provided 
		/// wiki markup/markdown. This action is POST only.
		/// </summary>
		/// <param name="id">The wiki markup.</param>
		/// <returns>The markup as rendered as HTML.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[ValidateInput(false)]
		[EditorRequired]
		[HttpPost]
		public ActionResult GetPreview(string id)
		{
			PageHtml pagehtml = "";

			if (!string.IsNullOrEmpty(id))
			{
				MarkupConverter converter = _pageService.GetMarkupConverter();
				pagehtml = converter.ToHtml(id);
			}

			return JavaScript(pagehtml.Html);
		}

		/// <summary>
		/// Lists the history of edits for a page.
		/// </summary>
		/// <param name="id">The ID of the page.</param>
		/// <returns>An <see cref="IList{PageHistoryViewModel}"/> as the model.</returns>
		[BrowserCache]
		public ActionResult History(int id)
		{
			ViewData["PageId"] = id;
			return View(_historyService.GetHistory(id).ToList());
		}

		/// <summary>
		/// Displays the Edit view in new page mode.
		/// </summary>
		/// <returns>An empty <see cref="PageViewModel"/> as the model.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult New(string title = "", string tags = "")
		{
			PageViewModel model = new PageViewModel()
			{
				Title = title,
				RawTags = tags,
			};

			model.AllTags = _pageService.AllTags().ToList();

			return View("Edit", model);
		}

		/// <summary>
		/// Saves a new page using the provided <see cref="PageViewModel"/> object to the database.
		/// </summary>
		/// <param name="model">The page details to save.</param>
		/// <returns>Redirects to /Wiki/{id} using the newly created page's ID.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult New(PageViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Edit", model);

			model = _pageService.AddPage(model);

			return RedirectToAction("Index", "Wiki", new { id = model.Id });
		}

		/// <summary>
		/// Reverts a page to the version specified, creating a new version in the process.
		/// </summary>
		/// <param name="versionId">The Guid ID of the version to revert to.</param>
		/// <param name="pageId">The id of the page</param>
		/// <returns>Redirects to the History action using the pageId parameter.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult Revert(Guid versionId, int pageId)
		{
			// Check if the page is locked to admin edits only before reverting.
			PageViewModel page = _pageService.GetById(pageId);
			if (page == null || (page.IsLocked && !Context.IsAdmin))
				return RedirectToAction("Index", "Home");

			_historyService.RevertTo(versionId, Context);

			return RedirectToAction("History", new { id = pageId });
		}

		/// <summary>
		/// Returns all pages for the given tag.
		/// </summary>
		/// <param name="id">The tag name</param>
		/// <returns>An <see cref="IEnumerable{PageViewModel}"/> as the model.</returns>
		public ActionResult Tag(string id)
		{
			id = HttpUtility.UrlDecode(id);
			ViewData["Tagname"] = id;

			return View(_pageService.FindByTag(id));
		}

		/// <summary>
		/// Gets a particular version of a page.
		/// </summary>
		/// <param name="id">The Guid ID for the version.</param>
		/// <returns>A <see cref="PageViewModel"/> as the model, which contains the HTML diff
		/// output inside the <see cref="PageViewModel.Content"/> property.</returns>
		public ActionResult Version(Guid id)
		{
			MarkupConverter converter = _pageService.GetMarkupConverter();
			IList<PageViewModel> bothVersions = _historyService.CompareVersions(id).ToList();
			string diffHtml = "";

			if (bothVersions[1] != null)
			{
				string oldVersion = converter.ToHtml(bothVersions[1].Content).Html;
				string newVersion = converter.ToHtml(bothVersions[0].Content).Html;
				HtmlDiff diff = new HtmlDiff(oldVersion, newVersion);
				diffHtml = diff.Build();
			}
			else
			{
				diffHtml = converter.ToHtml(bothVersions[0].Content).Html;
			}

			PageViewModel model = bothVersions[0];
			model.Content = diffHtml;
			return View(model);
		}
	}
}