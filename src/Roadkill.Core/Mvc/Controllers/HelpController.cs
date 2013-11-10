using System;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Localization;
using System.Linq;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides help for the 3 supported markup syntax.
	/// </summary>
	public class HelpController : ControllerBase
	{
		private CustomTokenParser _customTokenParser;
		private PageService _pageService;

		public HelpController(ApplicationSettings settings, UserServiceBase userManager, IUserContext context, SettingsService settingsService, PageService pageService)
			: base(settings, userManager, context, settingsService) 
		{
			_customTokenParser = new CustomTokenParser(settings);
			_pageService = pageService;
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult About()
		{
			// Get the first page with an "about" tag
			PageViewModel model = _pageService.FindByTag("about").FirstOrDefault();
			if (model == null)
				return RedirectToAction("New", "Pages", new { title = "about", tags = "about" });
			else
				return View("../Wiki/Index", model);
		}

		public ActionResult CreoleReference()
		{
			return View(_customTokenParser.Tokens);
		}

		public ActionResult MediaWikiReference()
		{
			return View(_customTokenParser.Tokens);
		}

		public ActionResult MarkdownReference()
		{
			return View(_customTokenParser.Tokens);
		}
	}
}
