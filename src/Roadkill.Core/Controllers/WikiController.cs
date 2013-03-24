using System;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides functionality for the /wiki/{id}/{title} route, which all pages are displayed via.
	/// </summary>
	[OptionalAuthorization]
	public class WikiController : ControllerBase
	{
		private PageManager _pageManager;

		public WikiController(IConfigurationContainer configuration, UserManager userManager, PageManager pageManager, IRoadkillContext context)
			: base(configuration, userManager, context) 
		{
			_pageManager = pageManager;
		}

		/// <summary>
		/// Displays the wiki page using the provided id.
		/// </summary>
		/// <param name="id">The page id</param>
		/// <param name="title">This parameter is passed in, but never used in queries.</param>
		/// <returns>A <see cref="PageSummary"/> to the Index view.</returns>
		/// <remarks>This action adds a "Last-Modified" header using the page's last modified date, if no user is currently logged in.</remarks>
		/// <exception cref="HttpNotFoundResult">Thrown if the page with the id cannot be found.</exception>
		public ActionResult Index(int? id, string title)
		{
			if (id == null || id < 1)
				return RedirectToAction("Index", "Home");

			PageSummary summary = _pageManager.GetById(id.Value);

			if (summary == null)
				return new HttpNotFoundResult(string.Format("The page with id '{0}' could not be found", id));

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
	}
}
