using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Moq;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class InstallControllerTests
	{
		private ApplicationSettings _applicationSettings;
		private ConfigReaderWriterStub _configReaderWriter;
		private InstallController _installController;
		private RepositoryFactoryMock _repositoryFactory;
		private MocksAndStubsContainer _container;
		private SettingsService _settingsService;
		private UserServiceMock _userService;
		private RepositoryMock _repository;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.Installed = false;

			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_configReaderWriter = _container.ConfigReaderWriter;
			_repositoryFactory = _container.RepositoryFactory;
			_repository = _repositoryFactory.Repository;

            _installController = new InstallController(_applicationSettings, _configReaderWriter, _repositoryFactory, _userService);
		}

		[Test]
		public void index__should_return_viewresult_and_model_with_languagemodels_and_set_uilanguage_to_english()
		{
			// Arrange

			// Act
			ActionResult result = _installController.Index();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			IEnumerable<LanguageViewModel> models = viewResult.ModelFromActionResult<IEnumerable<LanguageViewModel>>();
			Assert.NotNull(models, "Null model");
			Assert.That(models.Count(), Is.GreaterThanOrEqualTo(1));
			Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo("en"));
		}

		[Test]
		public void step1_should_return_viewresult_with_languageviewmodel_and_set_uiculture_from_language()
		{
			// Arrange
			string hinduCode = "hi";

			// Act
			ActionResult result = _installController.Step1(hinduCode);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			LanguageViewModel model = viewResult.ModelFromActionResult<LanguageViewModel>();
			Assert.NotNull(model, "Null model");
			Assert.That(model.Code, Is.EqualTo(hinduCode));

			Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo(hinduCode));
		}

		[Test]
		public void step2_should_return_viewresult_with_settingsviewmodel()
		{
			// Arrange
			string hinduCode = "hi";

			// Act
			ActionResult result = _installController.Step2(hinduCode);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			SettingsViewModel model = viewResult.ModelFromActionResult<SettingsViewModel>();
			Assert.NotNull(model, "Null model");
		}

		[Test]
		public void step2_should_set_uiculture_from_language_and_update_config()
		{
			// Arrange
			string hinduCode = "hi";

			// Act
			ActionResult result = _installController.Step2(hinduCode);

			// Assert
			Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo(hinduCode));
			Assert.That(_configReaderWriter.UILanguageCode, Is.EqualTo(hinduCode));
		}

		[Test]
		public void step3_should_return_viewresult_with_settingsviewmodel()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			existingModel.ConnectionString = "connectionstring";
			existingModel.SiteUrl = "siteurl";
			existingModel.SiteName = "sitename";

			// Act
			ActionResult result = _installController.Step3(existingModel);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			SettingsViewModel model = viewResult.ModelFromActionResult<SettingsViewModel>();
			Assert.NotNull(model, "Null model");

			/* The view is responsible for passing these across, 
			   but this gives a rough indication that no data is lost between steps */
			Assert.That(model.ConnectionString, Is.EqualTo(existingModel.ConnectionString));
			Assert.That(model.SiteUrl, Is.EqualTo(existingModel.SiteUrl));
			Assert.That(model.SiteName, Is.EqualTo(existingModel.SiteName));
		}

		[Test]
		public void step3b_should_return_database_viewresult_when_windowsauth_is_false()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			existingModel.UseWindowsAuth = false;

			// Act
			ActionResult result = _installController.Step3b(existingModel);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("Step3Database"));
		}

		[Test]
		public void step3b_should_return_windowsauth_viewresult_when_windowsauth_is_true()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			existingModel.UseWindowsAuth = true;

			// Act
			ActionResult result = _installController.Step3b(existingModel);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("Step3WindowsAuth"));
		}

		[Test]
		public void step3b_should_set_model_default_roles_and_ldap_connectionstring()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();

			// Act
			ActionResult result = _installController.Step3b(existingModel);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			SettingsViewModel model = viewResult.ModelFromActionResult<SettingsViewModel>();
			Assert.NotNull(model, "Null model");

			Assert.That(model.AdminRoleName, Is.EqualTo("Admin"));
			Assert.That(model.EditorRoleName, Is.EqualTo("Editor"));
			Assert.That(model.LdapConnectionString, Is.EqualTo("LDAP://"));
		}

		[Test]
		public void step4_should_set_model_defaults_for_attachments_theme_and_cache()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();

			// Act
			ActionResult result = _installController.Step4(existingModel);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			SettingsViewModel model = viewResult.ModelFromActionResult<SettingsViewModel>();
			Assert.NotNull(model, "Null model");

			Assert.That(model.AllowedFileTypes, Is.EqualTo("jpg,png,gif,zip,xml,pdf"));
			Assert.That(model.AttachmentsFolder, Is.EqualTo("~/App_Data/Attachments"));
			Assert.That(model.MarkupType, Is.EqualTo("Creole"));
			Assert.That(model.Theme, Is.EqualTo("Responsive"));
			Assert.That(model.UseObjectCache, Is.True);
			Assert.That(model.UseBrowserCache, Is.False);
		}

		[Test]
		public void step5_should_finalize_setup()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();

			// Act
			ActionResult result = _installController.Step5(existingModel);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			SettingsViewModel model = viewResult.ModelFromActionResult<SettingsViewModel>();
			Assert.NotNull(model, "Null model");
		}

		[Test]
		public void step5_should_reset_install_state_and_add_modelstate_error_when_exception_is_thrown()
		{
			// Arrange
			SettingsViewModel existingModel = null; // test using a null reference exception

			// Act
			_installController.Step5(existingModel);

			// Assert
			Assert.That(_configReaderWriter.InstallStateReset, Is.True);

			string error = _installController.ModelState["An error occurred installing"].Errors[0].ErrorMessage;
			Assert.That(error, Is.StringStarting("Object reference not set to an instance of an object"), error);
		}

		[Test]
		public void finalize_should_set_publicsite_and_ignoresearcherrors_to_true()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();

			// Act
			_installController.FinalizeInstall(existingModel);

			// Assert
			Assert.That(existingModel.IgnoreSearchIndexErrors, Is.True);
			Assert.That(existingModel.IsPublicSite, Is.True);
		}

		[Test]
		public void finalize_should_save_config_settings()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			existingModel.Theme = "Responsive"; // don't test everything, that's done elsewhere in the ConfigReaderWriterTests

			// Act
			_installController.FinalizeInstall(existingModel);

			// Assert
			Assert.That(_configReaderWriter.Saved, Is.True);
		}

		[Test]
		public void finalize_should_add_adminuser_when_windows_auth_is_false()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			existingModel.UseWindowsAuth = false;

			// Act
			_installController.FinalizeInstall(existingModel);

			// Assert
			UserViewModel adminUser = _userService.ListAdmins().FirstOrDefault();
			Assert.That(adminUser, Is.Not.Null);
		}

		[Test]
		public void unattendedsetup_should_add_admin_user_and_set_default_site_settings()
		{
			// Arrange

			// Act
			ActionResult result = _installController.Unattended("mock datastore", "fake connection string");

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult.Content, Is.EqualTo("Unattended installation complete"));

			UserViewModel adminUser = _userService.ListAdmins().FirstOrDefault(); // check admin
			Assert.That(adminUser, Is.Not.Null);

			ApplicationSettings appSettings = _configReaderWriter.ApplicationSettings; // check settings
			Assert.That(appSettings.DatabaseName, Is.EqualTo("mock datastore"));
			Assert.That(appSettings.ConnectionString, Is.EqualTo("fake connection string"));
			Assert.That(appSettings.UseObjectCache, Is.True);
			Assert.That(appSettings.UseBrowserCache, Is.True);

			Core.Configuration.SiteSettings settings = _settingsService.GetSiteSettings();		
			Assert.That(settings.AllowedFileTypes, Is.EqualTo("jpg,png,gif,zip,xml,pdf"));
			Assert.That(settings.MarkupType, Is.EqualTo("Creole"));
			Assert.That(settings.Theme, Is.EqualTo("Responsive"));
			Assert.That(settings.SiteName, Is.EqualTo("my site"));
			Assert.That(settings.SiteUrl, Is.EqualTo("http://localhost"));
		}

		[Test]
		public void installerjsvars_should_return_view()
		{
			// Arrange

			// Act
			ActionResult result = _installController.InstallerJsVars();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();
		}

		[Test]
		public void finalize_should_install_and_save_site_settings()
		{
			// Arrange
			var existingModel = new SettingsViewModel();
			existingModel.AdminEmail = "email";
			existingModel.AdminPassword = "password";
			existingModel.Theme = "ChewbaccaOnHolidayTheme";

			var installationServiceMock = new Mock<IInstallationService>();
			_installController.GetInstallationService = (factory, service, connectionString, userService) => installationServiceMock.Object;

			// Act
			_installController.FinalizeInstall(existingModel);

			// Assert
			installationServiceMock.Verify(x => x.ClearUserTable());
			installationServiceMock.Verify(x => x.CreateTables());
			installationServiceMock.Verify(x => x.SaveSiteSettings(existingModel));
			installationServiceMock.Verify(x => x.AddAdminUser("email", "password"));
		}
	}
}