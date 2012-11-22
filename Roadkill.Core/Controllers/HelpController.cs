using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using Roadkill.Core.Domain;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides help for the 3 supported markup syntax.
	/// </summary>
	public class HelpController : ControllerBase
	{
		public HelpController(IConfigurationContainer configuration, UserManager userManager, IRoadkillContext context)
			: base(configuration, userManager, context) 
		{
		}

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
