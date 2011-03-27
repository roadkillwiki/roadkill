using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.Web.Management;
using System.Data.SqlClient;
using Roadkill.Core.Converters;
using Roadkill.Core.Search;
using System.IO;

namespace Roadkill.Core.Controllers
{
	public class InstallController : ControllerBase
    {
		public ActionResult Index()
		{
			if (RoadkillSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View("Step1");
		}

		public ActionResult Step2()
		{
			if (RoadkillSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View(new SettingsSummary());
		}	

		[HttpPost]
		public ActionResult Step3(SettingsSummary summary)
		{
			if (RoadkillSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View(summary);
		}

		[HttpPost]
		public ActionResult Step3b(SettingsSummary summary)
		{
			if (RoadkillSettings.Installed)
				return RedirectToAction("Index", "Home");

			summary.LdapConnectionString = "LDAP://";
			summary.EditorRoleName = "Editor";
			summary.AdminRoleName = "Admin";

			if (summary.UseWindowsAuth)
				return View("Step3WindowsAuth", summary);
			else
				return View("Step3Database",summary);
		}

		[HttpPost]
		public ActionResult Step4(SettingsSummary summary)
		{
			if (RoadkillSettings.Installed)
				return RedirectToAction("Index", "Home");

			summary.AllowedExtensions = "jpg,png,gif,zip,xml,pdf";
			summary.AttachmentsFolder = "~/Attachments";
			summary.MarkupType = "Creole";
			summary.Theme = "Mediawiki";
			summary.CacheEnabled = true;
			summary.CacheText = true;

			return View(summary);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Step5(SettingsSummary summary)
		{
			if (RoadkillSettings.Installed)
				return RedirectToAction("Index", "Home");

			try
			{
				// Any missing values are handled by data annotations. Those that are missed
				// can be seen as fiddling errors which are down to the user.

				if (ModelState.IsValid)
				{
					// Update the web.config first, so all connections can be referenced.
					InstallManager.SaveWebConfigSettings(summary);

					// ASP.NET SQL user providers
					if (!summary.UseWindowsAuth)
					{
						InstallManager.InstallAspNetUsersDatabase(summary);
					}

					// Create the roadkill schema and save the configuration settings
					InstallManager.SaveDbSettings(summary,true);	
	
					// Create a blank search index
					SearchManager.CreateIndex();
				}
			}
			catch (Exception e)
			{
				try
				{
					InstallManager.ResetInstalledState();
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("An error ocurred installing", ex.Message + e.StackTrace);
				}

				ModelState.AddModelError("An error ocurred installing", e.Message + e.StackTrace);
			}

			return View(summary);
		}

		//
		// JSON actions
		//

		public ActionResult TestLdap(string connectionString, string username, string password, string groupName)
		{
			if (RoadkillSettings.Installed)
				return Content("");

			string errors = InstallManager.TestLdapConnection(connectionString, username, password, groupName);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		public ActionResult TestWebConfig()
		{
			if (RoadkillSettings.Installed)
				return Content("");

			string errors = InstallManager.TestSaveWebConfig();
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		public ActionResult TestAttachments(string folder)
		{
			if (RoadkillSettings.Installed)
				return Content("");

			string errors = InstallManager.TestAttachments(folder);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		public ActionResult TestDatabaseConnection(string connectionString)
		{
			if (RoadkillSettings.Installed)
				return Content("");

			string errors = InstallManager.TestConnection(connectionString);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}
    }

	/// <summary>
	/// Basic error information for the JSON-based install tests.
	/// </summary>
	public class TestResult
	{
		public string ErrorMessage { get; set; }

		public bool Success 
		{
			get { return string.IsNullOrEmpty(ErrorMessage); }
		}

		public TestResult(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}
	}
}
