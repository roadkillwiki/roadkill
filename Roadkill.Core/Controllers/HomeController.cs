using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;

namespace Roadkill.Core.Controllers
{
	public class HomeController : ControllerBase
    {
		public ActionResult Index()
		{
			PageManager manager = new PageManager();
			PageSummary summary = manager.FindByTag("homepage").FirstOrDefault();
			if (summary == null)
			{
				summary = new PageSummary();
				summary.Title = "You have no mainpage set";
				summary.Content = "To set a main page, create a page and assign the tag 'homepage' to it.";
			}

			SetPageTitle(summary.Title);

			return View(summary);
		}

		public ActionResult JavascriptSettingsForEditing()
		{		
			SetPageTitle("");
			UrlHelper helper = new UrlHelper(HttpContext.Request.RequestContext);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Format("var ROADKILL_coreScriptPath = '{0}';", helper.Content("~/Assets/Scripts/")));

			if (RoadkillContext.Current.IsLoggedIn)
			{
				builder.AppendLine(string.Format("var ROADKILL_fileManagerUrl = '{0}';", helper.Content("~/Page/AllFiles/")));
				builder.AppendLine(string.Format("var ROADKILL_tagAjaxUrl = '{0}';", helper.Content("~/Page/AllTags/")));
				builder.AppendLine(string.Format("var ROADKILL_markupType = '{0}';", RoadkillSettings.MarkupType));
				builder.AppendLine(string.Format("var ROADKILL_themePath =  '{0}';", Url.Content(RoadkillSettings.ThemePath)));
				builder.AppendLine(string.Format("var ROADKILL_attachmentsPath = '{0}';", Url.Content("~/" + RoadkillSettings.AttachmentsFolder)));
			}

			return Content(builder.ToString(), "text/javascript");
		}

		public ActionResult CreateUser()
		{
			SetPageTitle("Create new user");
			return View();
		}

		public ActionResult Login()
		{
			SetPageTitle("Roadkill Login");
			return View();
		}

		public ActionResult Logout()
		{
			UserManager manager = new UserManager();
			manager.Logout();
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult Login(string username, string password, string fromUrl)
		{
			SetPageTitle("Roadkill Login");

			UserManager manager = new UserManager();
			if (manager.Authenticate(username, password))
			{
				FormsAuthentication.SetAuthCookie(username, true);

				if (!string.IsNullOrWhiteSpace(fromUrl))
					return Redirect(fromUrl);
				else
					return RedirectToAction("Index");
			}
			else
			{
				ModelState.AddModelError("Username/Password", "The username/password are incorrect");
				SetPageTitle("Invalid username/password");
				return View();
			}
		}
    }
}
