using System;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Roadkill.Core.Configuration;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides functionality for the /wiki/{id}/{title} route, which all pages are displayed via.
	/// </summary>
	[OptionalAuthorization]
	public class WikiController : ControllerBase
	{
		public PageManager PageManager { get; private set; }

		public WikiController(ApplicationSettings settings, UserManagerBase userManager, PageManager pageManager,
			IUserContext context, SettingsManager siteSettingsManager)
			: base(settings, userManager, context, siteSettingsManager) 
		{
			PageManager = pageManager;
		}

		/// <summary>
		/// Displays the wiki page using the provided id.
		/// </summary>
		/// <param name="id">The page id</param>
		/// <param name="title">This parameter is passed in, but never used in queries.</param>
		/// <returns>A <see cref="PageSummary"/> to the Index view.</returns>
		/// <remarks>This action adds a "Last-Modified" header using the page's last modified date, if no user is currently logged in.</remarks>
		/// <exception cref="HttpNotFoundResult">Thrown if the page with the id cannot be found.</exception>
		[BrowserCache]
		public ActionResult Index(int? id, string title)
		{
			if (id == null || id < 1)
				return RedirectToAction("Index", "Home");

			PageSummary summary = PageManager.GetById(id.Value);

			if (summary == null)
				return new HttpNotFoundResult(string.Format("The page with id '{0}' could not be found", id));

			return View(summary);
		}

		public ActionResult PageToolbar(int? id)
		{
			if (id == null || id < 1)
				return Content("");

			PageSummary summary = PageManager.GetById(id.Value);

			if (summary == null)
				return Content(string.Format("The page with id '{0}' could not be found", id));

			return PartialView(summary);
		}
	}
}
