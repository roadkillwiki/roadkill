using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides functionality for the /wiki/{id}/{title} route, which all pages are displayed via.
	/// </summary>
	public class WikiController : ControllerBase
	{
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

			PageManager manager = new PageManager();
			PageSummary summary = manager.Get(id.Value);

			if (summary == null)
				return new HttpNotFoundResult(string.Format("The page with id '{0}' could not be found", id));

			RoadkillContext.Current.Page = summary;

			// This is using RFC 1123, the header needs RFC 2822 but this gives the same date output
			if (!RoadkillContext.Current.IsLoggedIn)
			{
				Response.AddHeader("Last-Modified", summary.ModifiedOn.ToString("r")); 
			}
			else
			{
				// Don't cache for logged in users
				Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
			}

			return View(summary);
		}
	}
}
