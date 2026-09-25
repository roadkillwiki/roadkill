using System;
using Microsoft.AspNetCore.Mvc;
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
	// About shows a wiki page (the first page tagged "about"): on a private site, the help pages need a login as the pages do
	[OptionalAuthorization]
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
				return View("~/Views/Wiki/Index.cshtml", model);
		}

		public ActionResult CreoleReference()
		{
			return View(_customTokenParser.Tokens);
		}

		public ActionResult MarkdownReference()
		{
			return View(_customTokenParser.Tokens);
		}
	}
}
