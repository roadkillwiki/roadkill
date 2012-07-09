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
	/// Provides help for the 3 supported markup syntax.
	/// </summary>
	public class HelpController : ControllerBase
	{
		[EditorRequired]
		public ActionResult CreoleReference()
		{
			return View(CustomTokenParser.Tokens);
		}

		[EditorRequired]
		public ActionResult MediaWikiReference()
		{
			return View(CustomTokenParser.Tokens);
		}

		[EditorRequired]
		public ActionResult MarkdownReference()
		{
			return View(CustomTokenParser.Tokens);
		}
	}
}
