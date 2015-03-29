using System;
using System.Security.Policy;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides functionality for the /wiki/{id}/{title} route, which all pages are displayed via.
	/// </summary>
	
	public class WikiController : ControllerBase
	{
		public PageService PageService { get; private set; }

		public WikiController(ApplicationSettings settings, UserServiceBase userManager, PageService pageService,
			IUserContext context, SettingsService settingsService)
			: base(settings, userManager, context, settingsService) 
		{
			PageService = pageService;
		}

		/// <summary>
		/// Displays the wiki page using the provided id.
		/// </summary>
		/// <param name="id">The page id</param>
		/// <param name="title">This parameter is passed in, but never used in queries.</param>
		/// <returns>A <see cref="PageViewModel"/> to the Index view.</returns>
		/// <remarks>This action adds a "Last-Modified" header using the page's last modified date, if no user is currently logged in.</remarks>
		/// <exception cref="HttpNotFoundResult">Thrown if the page with the id cannot be found.</exception>
		[BrowserCache]
        [ViewKeyOptionalAuthorization]
		public ActionResult Index(int? id, string title)
		{
			if (id == null || id < 1)
				return RedirectToAction("Index", "Home");

			PageViewModel model = PageService.GetById(id.Value, true);

		    if (model == null)
				throw new HttpException(404, string.Format("The page with id '{0}' could not be found", id));

			return View(model);
		}

        [ViewKeyOptionalAuthorization]
		public ActionResult PageToolbar(int? id)
		{
			if (id == null || id < 1)
				return Content("");

			PageViewModel model = PageService.GetById(id.Value);

			if (model == null)
				return Content(string.Format("The page with id '{0}' could not be found", id));

            model.ViewKeyLink = Url.Action("Index", "Wiki", new RouteValueDictionary(new {id, viewkey = PageService.GetViewKey(model.Id)}),Request.Url.Scheme, Request.Url.Host);
			return PartialView(model);
		}

		/// <summary>
		/// 404 not found page - configured in the web.config
		/// </summary>
        [OptionalAuthorization]
		public ActionResult NotFound()
		{
			return View("404");
		}

		/// <summary>
		/// 500 internal error - configured in the web.config
		/// </summary>
        [OptionalAuthorization]
		public ActionResult ServerError()
		{
			return View("500");
		}
	}
}
