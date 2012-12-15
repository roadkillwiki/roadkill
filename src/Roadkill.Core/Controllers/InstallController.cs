using System;
using System.Web.Mvc;
using Roadkill.Core.Search;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides functionality for the installation wizard.
	/// </summary>
	/// <remarks>If the web.config "installed" setting is "true", then all the actions in
	/// this controller redirect to the homepage</remarks>
	public class InstallController : ControllerBase
	{
		private IRepository _repository;
		private PageManager _pageManager;
		private SearchManager _searchManager;
		private SettingsManager _settingsManager;

		public InstallController(IConfigurationContainer configuration, UserManager userManager,
			PageManager pageManager, SearchManager searchManager, IRepository respository, 
			SettingsManager settingsManager, IRoadkillContext context)
			: base(configuration, userManager, context) 
		{
			_pageManager = pageManager;
			_searchManager = searchManager;
			_repository = respository;
			_settingsManager = settingsManager;
		}

		/// <summary>
		/// Displays the start page for the installer (step1).
		/// </summary>
		public ActionResult Index()
		{
			if (Configuration.ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			CopySqliteBinaries();

			return View("Step1");
		}

		/// <summary>
		/// Displays the second step in the installation wizard.
		/// </summary>
		public ActionResult Step2()
		{
			if (Configuration.ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View(new SettingsSummary(Configuration));
		}

		/// <summary>
		/// Displays the authentication choice step in the installation wizard.
		/// </summary>
		/// <remarks>The <see cref="SettingsSummary"/> object that is POST'd is passed to the next step.</remarks>
		[HttpPost]
		public ActionResult Step3(SettingsSummary summary)
		{
			if (Configuration.ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View(summary);
		}

		/// <summary>
		/// Displays either the Windows Authentication settings view, or the DB settings view depending on
		/// the choice in Step3.
		/// </summary>
		/// <remarks>The <see cref="SettingsSummary"/> object that is POST'd is passed to the next step.</remarks>
		[HttpPost]
		public ActionResult Step3b(SettingsSummary summary)
		{
			if (Configuration.ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			summary.LdapConnectionString = "LDAP://";
			summary.EditorRoleName = "Editor";
			summary.AdminRoleName = "Admin";

			if (summary.UseWindowsAuth)
				return View("Step3WindowsAuth", summary);
			else
				return View("Step3Database",summary);
		}

		/// <summary>
		/// Displays the final installation step, which provides choices for caching, themes etc.
		/// </summary>
		/// <remarks>The <see cref="SettingsSummary"/> object that is POST'd is passed to the next step.</remarks>
		[HttpPost]
		public ActionResult Step4(SettingsSummary summary)
		{
			if (Configuration.ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			summary.AllowedExtensions = "jpg,png,gif,zip,xml,pdf";
			summary.AttachmentsFolder = "~/Attachments";
			summary.MarkupType = "Creole";
			summary.Theme = "Mediawiki";
			summary.CacheEnabled = true;
			summary.CacheText = true;

			return View(summary);
		}

		/// <summary>
		/// Validates the POST'd <see cref="SettingsSummary"/> object. If the settings are valid,
		/// an attempt is made to install using this.
		/// </summary>
		/// <returns>The Step5 view is displayed.</returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Step5(SettingsSummary summary)
		{
			if (Configuration.ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			InstallHelper installHelper = new InstallHelper(UserManager, _repository);

			try
			{
				// Any missing values are handled by data annotations. Those that are missed
				// can be seen as fiddling errors which are down to the user.

				if (ModelState.IsValid)
				{
					// Update the web.config first, so all connections can be referenced.
					_settingsManager.SaveWebConfigSettings(summary);

					// Create the roadkill schema and save the configuration settings
					_settingsManager.CreateTables(summary);
					_settingsManager.SaveSiteConfiguration(summary, true);	

					// Add a user if we're not using AD.
					if (!summary.UseWindowsAuth)
					{
						installHelper.AddAdminUser(summary);
					}					
	
					// Create a blank search index
					_searchManager.CreateIndex();
				}
			}
			catch (Exception e)
			{
				try
				{
					installHelper.ResetInstalledState();
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("An error ocurred installing", ex.Message + e);
				}

				ModelState.AddModelError("An error ocurred installing", e.Message + e);
			}

			return View(summary);
		}

		//
		// JSON actions
		//

		/// <summary>
		/// This action is for JSON calls only. Attempts to contact an Active Directory server using the
		/// connection string and user details provided.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestLdap(string connectionString, string username, string password, string groupName)
		{
			if (Configuration.ApplicationSettings.Installed)
				return Content("");

			InstallHelper installHelper = new InstallHelper(UserManager, _repository);
			string errors = installHelper.TestLdapConnection(connectionString, username, password, groupName);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts to write to the web.config file and save it.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestWebConfig()
		{
			if (Configuration.ApplicationSettings.Installed)
				return Content("");

			InstallHelper installHelper = new InstallHelper(UserManager, _repository);
			string errors = installHelper.TestSaveWebConfig();
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Checks to see if the provided folder exists and if it can be written to.
		/// </summary>
		/// <param name="folder"></param>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestAttachments(string folder)
		{
			string errors = InstallHelper.TestAttachments(folder);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts a database connection using the provided connection string.
		/// </summary>
		/// <param name="folder"></param>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestDatabaseConnection(string connectionString,string databaseType)
		{
			InstallHelper installHelper = new InstallHelper(UserManager, _repository);
			string errors = installHelper.TestConnection(connectionString, databaseType);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Attempts to copy the correct SQL binaries to the bin folder for the architecture the app pool is running under.
		/// </summary>
		private void CopySqliteBinaries()
		{
			//
			// Copy the SQLite files over
			//
			string sqliteFileSource = Server.MapPath("~/App_Data/SQLiteBinaries/x86/System.Data.SQLite.dll");
			string sqliteFileDest = Server.MapPath("~/bin/System.Data.SQLite.dll");
			string sqliteLinqFileSource = Server.MapPath("~/App_Data/SQLiteBinaries/x86/System.Data.SQLite.Linq.dll");
			string sqliteFileLinqDest = Server.MapPath("~/bin/System.Data.SQLite.Linq.dll");

			if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
			{
				sqliteFileSource = Server.MapPath("~/App_Data/SQLiteBinaries/x64/System.Data.SQLite.dll");
				sqliteLinqFileSource = Server.MapPath("~/App_Data/SQLiteBinaries/x64/System.Data.SQLite.Linq.dll");
			}

			System.IO.File.Copy(sqliteFileSource, sqliteFileDest, true);
			System.IO.File.Copy(sqliteLinqFileSource, sqliteFileLinqDest, true);
		}
	}

	/// <summary>
	/// Basic error information for the JSON actions
	/// </summary>
	public class TestResult
	{
		/// <summary>
		/// Any error message associated with the call.
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Indicates if there are any errors.
		/// </summary>
		public bool Success 
		{
			get { return string.IsNullOrEmpty(ErrorMessage); }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestResult"/> class.
		/// </summary>
		/// <param name="errorMessage">The error message.</param>
		public TestResult(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}
	}
}
