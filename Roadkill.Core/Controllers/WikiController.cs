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
		public ActionResult Index(int id, string title)
		{
			PageManager manager = new PageManager();
			PageSummary summary = manager.Get(id);

			if (summary == null)
				return new HttpNotFoundResult(string.Format("The page with title or id '{0}' could not be found", id));

			RoadkillContext.Current.Page = summary;

			return View(summary);
		}
    }
}
