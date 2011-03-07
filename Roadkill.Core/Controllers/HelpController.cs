using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;

namespace Roadkill.Core.Controllers
{
	public class HelpController : ControllerBase
    {
		public ActionResult CreoleReference()
		{
			return View();
		}

		public ActionResult MediaWikiReference()
		{
			return View();
		}

		public ActionResult MarkdownReference()
		{
			return View();
		}
    }
}
