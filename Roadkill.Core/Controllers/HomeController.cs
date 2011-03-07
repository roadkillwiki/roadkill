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

			return View(summary);
		}

		public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Login(string username, string password, string fromUrl)
		{
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
				return View();
			}
		}

		public ActionResult Logout()
		{
			UserManager manager = new UserManager();
			manager.Logout();
			return RedirectToAction("Index");
		}

		public ActionResult Install()
		{
			if (RoadkillSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View();
		}

		[HttpPost]
		public ActionResult Install(string connectionString, string adminPassword)
		{
			UserManager manager = new UserManager();

			MembershipCreateStatus status = manager.AddAdminUser(adminPassword, "admin@localhost");
			if (status == MembershipCreateStatus.DuplicateUserName)
			{
				// Do nothing, for now. The passwords may be out of sync which 
				// requires the view being changed to accomodate this.
			}

			Page.Configure(RoadkillSettings.ConnectionString, true);
			RoadkillSettings.Install(connectionString, adminPassword);

			return View("InstallComplete");
		}
    }
}
