using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;

namespace Roadkill.Core.Controllers
{
	public class WikiController : ControllerBase
    {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">The page id</param>
		/// <param name="title">The parameter is passed in, but never queried</param>
		/// <returns></returns>
		public ActionResult Index(int? id, string title)
		{
			if (id == null || id < 1)
				return RedirectToAction("Index", "Home");

			PageManager manager = new PageManager();
			PageSummary summary = manager.Get(id.Value);

			if (summary == null)
				return new HttpNotFoundResult(string.Format("The page with id '{0}' could not be found", id));

			RoadkillContext.Current.Page = summary;

			Response.AddHeader("Last-Modified", summary.ModifiedOn.ToString("r")); // This is RFC 1123, the header needs RFC 2822 but this works
			return View(summary);
		}
    }
}
