using System;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Attachments;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security.Windows;
using System.IO;
using Roadkill.Core.DI;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using System.Linq;

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
		private PageService _pageService;
		private SearchService _searchService;
		private SettingsService _settingsService;
		private static string _uiLanguageCode = "en";

		public InstallController(ApplicationSettings settings, UserServiceBase userManager,
			PageService pageService, SearchService searchService, IRepository respository,
			SettingsService settingsService, IUserContext context)
			: base(settings, userManager, context, settingsService) 
		{
			_pageService = pageService;
			_searchService = searchService;
			_repository = respository;
			_settingsService = settingsService;
		}

		/// <summary>
		/// Installs Roadkill with default settings and the provided datastory type and connection string.
		/// </summary>
		public ActionResult Unattended(string datastoreType, string connectionString)
		{
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			SettingsViewModel settingsModel = new SettingsViewModel();
			settingsModel.DataStoreTypeName = datastoreType;
			settingsModel.ConnectionString = connectionString;
			settingsModel.AllowedFileTypes = "jpg,png,gif,zip,xml,pdf";
			settingsModel.AttachmentsFolder = "~/App_Data/Attachments";
			settingsModel.MarkupType = "Creole";
			settingsModel.Theme = "Mediawiki";
			settingsModel.UseObjectCache = true;
			settingsModel.UseBrowserCache = true;
			settingsModel.AdminEmail = "admin@localhost";
			settingsModel.AdminPassword = "Password1";
			settingsModel.AdminRoleName = "admins";
			settingsModel.EditorRoleName = "editors";
			settingsModel.SiteName = "my site";
			settingsModel.SiteUrl = "http://localhost";

			FinalizeInstall(settingsModel);

			return Content("Unattended installation complete");
		}

		/// <summary>
		/// Displays the language choice page.
		/// </summary>
		public ActionResult Index()
		{
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

			return View("Index", LanguageViewModel.SupportedLocales());
		}

		/// <summary>
		/// Displays the start page for the installer (step1).
		/// </summary>
		public ActionResult Step1(string language)
		{
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
			LanguageViewModel languageModel = LanguageViewModel.SupportedLocales().First(x => x.Code == language);

			return View(languageModel);
		}

		/// <summary>
		/// Displays the second step in the installation wizard.
		/// </summary>
		public ActionResult Step2(string language)
		{
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			// Persist the language change now that we know the web.config can be written to.
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
			ConfigReaderWriter configReader = ConfigReaderWriterFactory.GetConfigReader();
			configReader.UpdateLanguage(language);

			return View(new SettingsViewModel());
		}

		/// <summary>
		/// Displays the authentication choice step in the installation wizard.
		/// </summary>
		/// <remarks>The <see cref="SettingsViewModel"/> object that is POST'd is passed to the next step.</remarks>
		[HttpPost]
		public ActionResult Step3(SettingsViewModel model)
		{
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View(model);
		}

		/// <summary>
		/// Displays either the Windows Authentication settings view, or the DB settings view depending on
		/// the choice in Step3.
		/// </summary>
		/// <remarks>The <see cref="SettingsViewModel"/> object that is POST'd is passed to the next step.</remarks>
		[HttpPost]
		public ActionResult Step3b(SettingsViewModel model)
		{
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			model.LdapConnectionString = "LDAP://";
			model.EditorRoleName = "Editor";
			model.AdminRoleName = "Admin";

			if (model.UseWindowsAuth)
				return View("Step3WindowsAuth", model);
			else
				return View("Step3Database",model);
		}

		/// <summary>
		/// Displays the final installation step, which provides choices for caching, themes etc.
		/// </summary>
		/// <remarks>The <see cref="SettingsViewModel"/> object that is POST'd is passed to the next step.</remarks>
		[HttpPost]
		public ActionResult Step4(SettingsViewModel model)
		{
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			model.AllowedFileTypes = "jpg,png,gif,zip,xml,pdf";
			model.AttachmentsFolder = "~/App_Data/Attachments";
			model.MarkupType = "Creole";
			model.Theme = "Mediawiki";
			model.UseObjectCache = true;
			model.UseBrowserCache = false;

			return View(model);
		}

		/// <summary>
		/// Validates the POST'd <see cref="SettingsViewModel"/> object. If the settings are valid,
		/// an attempt is made to install using this.
		/// </summary>
		/// <returns>The Step5 view is displayed.</returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Step5(SettingsViewModel model)
		{
			if (ApplicationSettings.Installed)
				return RedirectToAction("Index", "Home");

			try
			{
				// Any missing values are handled by data annotations. Those that are missed
				// can be seen as fiddling errors which are down to the user.

				if (ModelState.IsValid)
				{
					FinalizeInstall(model);
				}
			}
			catch (Exception e)
			{
				try
				{
					ConfigReaderWriter configReader = ConfigReaderWriterFactory.GetConfigReader();
					configReader.ResetInstalledState();
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("An error ocurred installing", ex.Message + e);
				}

				ModelState.AddModelError("An error ocurred installing", e.Message + e);
			}

			return View(model);
		}

		private void FinalizeInstall(SettingsViewModel model)
		{
			// The name is passed through each step, so parse it
			DataStoreType dataStoreType = DataStoreType.ByName(model.DataStoreTypeName);
			model.DataStoreTypeName = dataStoreType.Name;

			// Update all repository references for the dependencies of this class
			// (changing the For() in StructureMap won't do this as the references have already been created).
			_repository = RepositoryManager.ChangeRepository(dataStoreType, model.ConnectionString, model.UseObjectCache);
			UserManager.UpdateRepository(_repository);
			_settingsService.UpdateRepository(_repository);
			_searchService.UpdateRepository(_repository);

			// Default these two properties for installations
			model.IgnoreSearchIndexErrors = true;
			model.IsPublicSite = true;

			// Update the web.config first, so all connections can be referenced.
			ConfigReaderWriter configReader = ConfigReaderWriterFactory.GetConfigReader();
			configReader.Save(model);

			// Create the roadkill schema and save the configuration settings
			_settingsService.CreateTables(model);
			_settingsService.SaveSiteSettings(model);

			// Add a user if we're not using AD.
			if (!model.UseWindowsAuth)
			{
				UserManager.AddUser(model.AdminEmail, "admin", model.AdminPassword, true, false);
			}

			// Create a blank search index
			_searchService.CreateIndex();
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

			string errors = ActiveDirectoryProvider.TestLdapConnection(connectionString, username, password, groupName);
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

			ConfigReaderWriter configReader = ConfigReaderWriterFactory.GetConfigReader();
			string errors = configReader.TestSaveWebConfig();
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Checks to see if the provided folder exists and if it can be written to.
		/// </summary>
		/// <param name="folder"></param>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestAttachments(string folder)
		{
			string errors = AttachmentPathUtil.AttachmentFolderExistsAndWriteable(folder, HttpContext);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts a database connection using the provided connection string.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestDatabaseConnection(string connectionString, string databaseType)
		{
			string errors = RepositoryManager.TestDbConnection(connectionString, databaseType);
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
				string sqliteInteropFileSource = Path.Combine(ApplicationSettings.SQLiteBinariesPath, "x86", "SQLite.Interop.dll");

				string sqliteInteropFileDest = Server.MapPath("~/bin/SQLite.Interop.dll");

				if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
				{
					sqliteInteropFileSource = Path.Combine(ApplicationSettings.SQLiteBinariesPath, "x64", "SQLite.Interop.dll");
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
