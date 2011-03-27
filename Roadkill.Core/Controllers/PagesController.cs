using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.IO;
using Roadkill.Core.Diff;
using Roadkill.Core.Converters;

namespace Roadkill.Core.Controllers
{
	public class PagesController : ControllerBase
    {
		public ActionResult AllPages()
		{
			PageManager manager = new PageManager();
			return View(manager.AllPages());
		}

		public ActionResult ByUser(string id,bool? encoded)
		{
			// Usernames are base64 encoded by roadkill (to cater for usernames like domain\john).
			// However the URL also supports humanly-readable format, e.g. /ByUser/chris
			if (encoded == true)
			{
				id = id.FromBase64();
			}

			ViewData["Username"] = id;

			PageManager manager = new PageManager();
			return View(manager.AllPagesCreatedBy(id));
		}

		public ActionResult Tag(string id)
		{
			ViewData["Tagname"] = id;

			PageManager manager = new PageManager();
			return View(manager.FindByTag(id));
		}

		public ActionResult AllTags()
		{
			PageManager manager = new PageManager();
			return View(manager.AllTags());
		}

		[EditorRequired]
		public ActionResult AllTagsAsJson()
		{
			PageManager manager = new PageManager();
			IEnumerable<TagSummary> tags = manager.AllTags();
			List<string> tagsArray = new List<string>();
			foreach (TagSummary summary in tags)
			{
				tagsArray.Add(summary.Name);
			}

			return Json(tagsArray, JsonRequestBehavior.AllowGet);
		}

		public ActionResult History(int id)
		{
			HistoryManager manager = new HistoryManager();
			return View(manager.GetHistory(id).ToList());
		}

		[ValidateInput(false)]
		[EditorRequired]
		public ActionResult GetPreview(string id)
		{
			string html = "";

			if (!string.IsNullOrEmpty(id))
			{
				html = id.WikiMarkupToHtml();
			}
			
			return JavaScript(html);
		}

		public ActionResult Version(Guid id)
		{
			HistoryManager manager = new HistoryManager();
			IList<PageSummary> bothVersions = manager.CompareVersions(id).ToList();
			string diffHtml = "";

			if (bothVersions[1] != null)
			{
				string oldVersion = bothVersions[1].Content.WikiMarkupToHtml();
				string newVersion = bothVersions[0].Content.WikiMarkupToHtml();
				HtmlDiff diff = new HtmlDiff(oldVersion, newVersion);
				diffHtml = diff.Build();
			}
			else
			{
				diffHtml = bothVersions[0].Content.WikiMarkupToHtml();
			}

			PageSummary summary = bothVersions[0];
			summary.Content = diffHtml;
			return View(summary);
		}

		[EditorRequired]
		public ActionResult Revert(Guid versionId,int pageId)
		{
			HistoryManager manager = new HistoryManager();
			manager.RevertTo(versionId);

			return RedirectToAction("History", new { id = pageId });
		}

		[EditorRequired]
		public ActionResult New()
		{
			return View("Edit", new PageSummary());
		}

		[EditorRequired]
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult New(PageSummary summary)
		{
			if (!ModelState.IsValid)
				return View("Edit", summary);

			PageManager manager = new PageManager();
			summary = manager.AddPage(summary);

			return RedirectToAction("Index", "Wiki", new { id = summary.Id });
		}

		[EditorRequired]
		public ActionResult Edit(int id)
		{
			PageManager manager = new PageManager();
			PageSummary summary = manager.Get(id);

			if (summary != null)
			{
				return View("Edit", summary);
			}
			else
			{
				return RedirectToAction("New");
			}
		}

		[EditorRequired]
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(PageSummary summary)
		{
			if (!ModelState.IsValid)
				return View("Edit", summary);

			PageManager manager = new PageManager();
			manager.UpdatePage(summary);

			return RedirectToAction("Index","Wiki", new { id = summary.Id });
		}

		[AdminRequired]
		public ActionResult Delete(int id)
		{
			PageManager manager = new PageManager();
			manager.DeletePage(id);

			return RedirectToAction("AllPages");
		}
    }
}
