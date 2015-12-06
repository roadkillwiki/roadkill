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
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using System.Linq;
using Roadkill.Core.DependencyResolution;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides functionality for the installation wizard.
	/// </summary>
	/// <remarks>If the web.config "installed" setting is "true", then all the actions in
	/// this controller redirect to the homepage</remarks>
	public class InstallController : Controller
	{
		private static string _uiLanguageCode = "en";

		private readonly ApplicationSettings _applicationSettings;
		private readonly ConfigReaderWriter _configReaderWriter;
		private readonly IRepositoryFactory _repositoryFactory;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationSettings">Use solely to detect whether Roadkill is already installed.</param>
		/// <param name="configReaderWriter"></param>
		/// <param name="repositoryFactory"></param>
		public InstallController(ApplicationSettings applicationSettings, ConfigReaderWriter configReaderWriter, IRepositoryFactory repositoryFactory)
		{
			_applicationSettings = applicationSettings;
			_configReaderWriter = configReaderWriter;
			_repositoryFactory = repositoryFactory;
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (_applicationSettings.Installed)
				filterContext.Result = new RedirectResult(this.Url.Action("Index", "Home"));
		}

		/// <summary>
		/// Installs Roadkill with default settings and the provided datastory type and connection string.
		/// </summary>
		public ActionResult Unattended(string databaseName, string connectionString)
		{
			SettingsViewModel settingsModel = new SettingsViewModel();
			settingsModel.DatabaseName = databaseName;
			settingsModel.ConnectionString = connectionString;
			settingsModel.AllowedFileTypes = "jpg,png,gif,zip,xml,pdf";
			settingsModel.AttachmentsFolder = "~/App_Data/Attachments";
			settingsModel.MarkupType = "Creole";
			settingsModel.Theme = "Responsive";
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
		/// Returns Javascript 'constants' for the installer.
		/// </summary>
		public ActionResult InstallerJsVars()
		{
			return View();
		}

		/// <summary>
		/// Displays the language choice page.
		/// </summary>
		public ActionResult Index()
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

			return View("Index", LanguageViewModel.SupportedLocales());
		}

		/// <summary>
		/// Displays the start page for the installer (step1).
		/// </summary>
		public ActionResult Step1(string language)
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
			LanguageViewModel languageModel = LanguageViewModel.SupportedLocales().First(x => x.Code == language);

			return View(languageModel);
		}

		/// <summary>
		/// Displays the second step in the installation wizard (connection strings and site url/name).
		/// </summary>
		public ActionResult Step2(string language)
		{
			// Persist the language change now that we know the web.config can be written to.
			if (!string.IsNullOrEmpty(language))
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
				_configReaderWriter.UpdateLanguage(language);
			}

			var settingsModel = new SettingsViewModel();
			var installationService = new InstallationService(_repositoryFactory, "", "");
			settingsModel.SetSupportedDatabases(installationService.GetSupportedDatabases());

			return View(settingsModel);
		}

		/// <summary>
		/// Displays the authentication choice step in the installation wizard.
		/// </summary>
		/// <remarks>The <see cref="SettingsViewModel"/> object that is POST'd is passed to the next step.</remarks>
		[HttpPost]
		public ActionResult Step3(SettingsViewModel model)
		{
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
			model.AllowedFileTypes = "jpg,png,gif,zip,xml,pdf";
			model.AttachmentsFolder = "~/App_Data/Attachments";
			model.MarkupType = "Creole";
			model.Theme = "Responsive";
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
					_configReaderWriter.ResetInstalledState();
				}
				catch (Exception ex)
				{
					// TODO-translation
					ModelState.AddModelError("An error occurred installing", ex.Message + e);
				}

				ModelState.AddModelError("An error occurred installing", e.Message + e);
			}

			return View(model);
		}

		internal void FinalizeInstall(SettingsViewModel model)
		{
			// Default these two properties for installations
			model.IgnoreSearchIndexErrors = true;
			model.IsPublicSite = true;

			var installationService = new InstallationService(_repositoryFactory, model.DatabaseName, model.ConnectionString);
			installationService.ClearUserTable();
			installationService.CreateTables(model);
			installationService.SaveSiteSettings(model);

			// Add a user if we're not using AD.
			if (!model.UseWindowsAuth)
			{
				installationService.AddAdminUser(model.AdminEmail, model.AdminPassword);
			}

			// Update the web.config first, so all connections can be referenced.
			_configReaderWriter.Save(model);

			// Create a blank search index
			//_searchService.CreateIndex();
		}
	}
}
