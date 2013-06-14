using System;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides help for the 3 supported markup syntax.
	/// </summary>
	public class HelpController : ControllerBase
	{
		private CustomTokenParser _customTokenParser;

		public HelpController(ApplicationSettings settings, UserManagerBase userManager, IUserContext context, SettingsManager siteSettingsManager)
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
			// Test action for errors
			throw new Exception("Woops an error occurred");
		}
#endif
	}
}
