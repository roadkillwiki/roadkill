using System;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Attachments;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security.Windows;

namespace Roadkill.Core.Mvc.Controllers
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

		public InstallController(ApplicationSettings settings, UserManagerBase userManager,
			PageManager pageManager, SearchManager searchManager, IRepository respository,
			SettingsManager settingsManager, IUserContext context, SettingsManager siteSettingsManager)
			: base(settings, userManager, context, siteSettingsManager) 
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
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View("Step1");
		}

		/// <summary>
		/// Displays the second step in the installation wizard.
		/// </summary>
		public ActionResult Step2()
		{
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View(new SettingsSummary());
		}

		/// <summary>
		/// Displays the authentication choice step in the installation wizard.
		/// </summary>
		/// <remarks>The <see cref="SettingsSummary"/> object that is POST'd is passed to the next step.</remarks>
		[HttpPost]
		public ActionResult Step3(SettingsSummary summary)
		{
			if (ApplicationSettings.Installed)
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
			if (ApplicationSettings.Installed)
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
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			summary.AllowedExtensions = "jpg,png,gif,zip,xml,pdf";
			summary.AttachmentsFolder = "~/Attachments";
			summary.MarkupType = "Creole";
			summary.Theme = "Mediawiki";
			summary.UseObjectCache = true;
			summary.UseBrowserCache = false;

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
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			try
			{
				// Any missing values are handled by data annotations. Those that are missed
				// can be seen as fiddling errors which are down to the user.

				if (ModelState.IsValid)
				{
					// The name is passed through each step, so parse it
					DataStoreType dataStoreType = DataStoreType.ByName(summary.DataStoreTypeName);
					summary.DataStoreTypeName = dataStoreType.Name;

					// Update all repository references for the dependencies of this class
					// (changing the For() in StructureMap won't do this as the references have already been created).
					_repository = DependencyContainer.ChangeRepository(dataStoreType, summary.ConnectionString, summary.UseObjectCache);
					UserManager.UpdateRepository(_repository);
					_settingsManager.UpdateRepository(_repository);
					_searchManager.UpdateRepository(_repository);

					// Update the web.config first, so all connections can be referenced.
					ConfigFileManager configManager = new ConfigFileManager();
					configManager.WriteSettings(summary);
					configManager.Save();

					// Create the roadkill schema and save the configuration settings
					_settingsManager.CreateTables(summary);
					_settingsManager.SaveSiteSettings(summary, true);	

					// Add a user if we're not using AD.
					if (!summary.UseWindowsAuth)
					{
						UserManager.AddUser(summary.AdminEmail, "admin", summary.AdminPassword, true, false);
					}					
	
					// Create a blank search index
					_searchManager.CreateIndex();
				}
			}
			catch (Exception e)
			{
				try
				{
					ConfigFileManager configManager = new ConfigFileManager();
					configManager.ResetInstalledState();
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
			if (ApplicationSettings.Installed)
				return Content("");

			string errors = ActiveDirectoryService.TestLdapConnection(connectionString, username, password, groupName);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts to write to the web.config file and save it.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestWebConfig()
		{
			if (ApplicationSettings.Installed)
				return Content("");

			ConfigFileManager configManager = new ConfigFileManager();
			string errors = configManager.TestSaveWebConfig();
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Checks to see if the provided folder exists and if it can be written to.
		/// </summary>
		/// <param name="folder"></param>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestAttachments(string folder)
		{
			string errors = AttachmentFileHandler.TestAttachmentsFolder(folder);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts a database connection using the provided connection string.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestDatabaseConnection(string connectionString, string databaseType)
		{
			string errors = DependencyContainer.TestDbConnection(connectionString, databaseType);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Attempts to copy the correct SQL binaries to the bin folder for the architecture the app pool is running under.
		/// </summary>
		public ActionResult CopySqlite()
		{
			string errors = "";

			try
			{
				string sqliteInteropFileSource = Server.MapPath("~/App_Data/SQLiteBinaries/x86/SQLite.Interop.dll");
				string sqliteInteropFileDest = Server.MapPath("~/bin/SQLite.Interop.dll");

				if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
				{
					sqliteInteropFileSource = Server.MapPath("~/App_Data/SQLiteBinaries/x64/SQLite.Interop.dll");
				}

				System.IO.File.Copy(sqliteInteropFileSource, sqliteInteropFileDest, true);
			}
			catch (Exception e)
			{
				errors = e.ToString();
			}

			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
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
