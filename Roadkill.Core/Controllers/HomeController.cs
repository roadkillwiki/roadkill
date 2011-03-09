using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.Web.Management;
using System.Data.SqlClient;

namespace Roadkill.Core.Controllers
{
	public class HomeController : ControllerBase
    {
		public ActionResult Index()
		{
			bool b = Membership.EnablePasswordReset;
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
			//if (RoadkillSettings.Installed)
			//	return RedirectToAction("Index", "Home");

			return View();
		}

		[HttpPost]
		public ActionResult Install(string connectionString, string adminPassword)
		{
			string databaseName = "";

			try
			{
				using (SqlConnection connection = new SqlConnection(RoadkillSettings.ConnectionString))
				{
					connection.Open();
					databaseName = connection.Database;
				}
			}
			catch (SqlException)
			{
				// TODO: InstallerException
				throw new InstallerException("No database name was specified in the connection string");
			}

			if (string.IsNullOrEmpty(databaseName))
				throw new InstallerException("No database name was specified in the connection string");

			// Create the provider database and schema
			SqlServices.Install(databaseName, SqlFeatures.Membership | SqlFeatures.RoleManager, RoadkillSettings.ConnectionString);

			// Create the roadkill schema
			Page.Configure(RoadkillSettings.ConnectionString, true,RoadkillSettings.CachedEnabled);

			// Add the admin user, admin role and editor roles.
			UserManager manager = new UserManager();
			MembershipCreateStatus status = manager.AddAdminUser(adminPassword, "admin@localhost");
			if (status == MembershipCreateStatus.DuplicateUserName)
			{
				// Do nothing, for now. The passwords may be out of sync which 
				// requires the view being changed to accomodate this.
			}

			// Update the web.config to indicate install is complete
			//RoadkillSettings.SaveWebConfig(connectionString);

			return View("InstallComplete");
		}
    }
}
