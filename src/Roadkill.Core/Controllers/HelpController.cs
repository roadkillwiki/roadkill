using System;
using System.Web.Mvc;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides help for the 3 supported markup syntax.
	/// </summary>
	public class HelpController : ControllerBase
	{
		private CustomTokenParser _customTokenParser;

		public HelpController(IConfigurationContainer configuration, UserManager userManager, IRoadkillContext context)
			: base(configuration, userManager, context) 
		{
			_customTokenParser = new CustomTokenParser(configuration);
		}

		[EditorRequired]
		public ActionResult CreoleReference()
		{
			return View(_customTokenParser.Tokens);
		}

		[EditorRequired]
		public ActionResult MediaWikiReference()
		{
			return View(_customTokenParser.Tokens);
		}

		[EditorRequired]
		public ActionResult MarkdownReference()
		{
			return View(_customTokenParser.Tokens);
		}

		[AdminRequired]
		public ActionResult ShowError()
		{
			throw new Exception("foo");
		}
	}
}
