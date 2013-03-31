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

		public HelpController(ApplicationSettings settings, UserManager userManager, IRoadkillContext context, SettingsManager siteSettingsManager)
			: base(settings, userManager, context, siteSettingsManager) 
		{
			_customTokenParser = new CustomTokenParser(settings);
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

#if DEBUG
		[AdminRequired]
		public ActionResult ShowError()
		{
			// There is definitely a more intelligent approach than this, I just need to think of it first.
			throw new Exception("Woops an error occurred");
		}
#endif
	}
}
