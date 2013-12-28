using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Localization;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using System.Runtime.Caching;
using System.Threading;
using Roadkill.Tests.Unit.StubsAndMocks;
using MvcContrib.TestHelper;
using Roadkill.Core.DI;
using StructureMap;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class InstallControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;
		private SearchServiceMock _searchService;
		private ConfigReaderWriterStub _configReaderWriter;

		private InstallController _installController;
		private List<DataStoreType> _defaultDataStoreTypes;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// This is a bit nasty - take the original *STATIC* list of types and reset them once done
			// (The refactor alternative to this would be the DataStoreType to take a factory of some sort)
			_defaultDataStoreTypes = DataStoreType.AllTypes.ToList();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			DataStoreType.AllTypes = _defaultDataStoreTypes;
		}

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.Installed = false;

			_context = _container.UserContext;
			_repository = _container.Repository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;
			_searchService = _container.SearchService;
			_configReaderWriter = new ConfigReaderWriterStub();

			_installController = new InstallController(_applicationSettings, _userService, _pageService, _searchService, _repository, _settingsService, _context, _configReaderWriter);
		}

		[Test]
		public void Index_Should_Redirect_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _installController.Index();

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Step1_Should_Redirect_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _installController.Step1("en");

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Step2_Should_Redirect_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _installController.Step2("en");

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Step3_Should_Redirect_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _installController.Step3(new SettingsViewModel());

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Step3b_Should_Redirect_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _installController.Step3b(new SettingsViewModel());

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Step4_Should_Redirect_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _installController.Step4(new SettingsViewModel());

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Step5_Should_Redirect_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _installController.Step5(new SettingsViewModel());

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Index__Should_Return_ViewResult_And_Model_With_LanguageModels_And_Set_UILanguage_To_English()
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
		public void Step1_Should_Return_ViewResult_With_LanguageViewModel_And_Set_UICulture_From_Language()
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
		public void Step2_Should_Return_ViewResult_With_SettingsViewModel()
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
		public void Step2_Should_Set_UICulture_From_Language_And_Update_Config()
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
		public void Step3_Should_Return_ViewResult_With_SettingsViewModel()
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
		public void Step3b_Should_Return_Database_ViewResult_When_WindowsAuth_Is_False()
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
		public void Step3b_Should_Return_WindowsAuth_ViewResult_When_WindowsAuth_Is_True()
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
		public void Step3b_Should_Set_Model_Default_Roles_And_LDAP_ConnectionString()
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
		public void Step4_Should_Set_Model_Defaults_For_Attachments_Theme_And_Cache()
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
		public void Step5_Should_Finalize_Setup()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			SetMockDataStoreType(existingModel);

			// Act
			ActionResult result = _installController.Step5(existingModel);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			SettingsViewModel model = viewResult.ModelFromActionResult<SettingsViewModel>();
			Assert.NotNull(model, "Null model");
		}

		[Test]
		[Description("This test will also test the Finalize method's datastoretype parsing and changing the repository")]
		public void Step5_Should_Reset_Install_State_And_Add_ModelState_Error_When_Exception_Is_Thrown()
		{
			// Arrange
			string exceptionMessage = RepositoryThrowsExceptionsMock.ExceptionMessage;

			SettingsViewModel existingModel = new SettingsViewModel();
			SetMockDataStoreType(existingModel);
			DataStoreType.AllTypes.First().CustomRepositoryType = typeof(RepositoryThrowsExceptionsMock).AssemblyQualifiedName;

			// Act
			ActionResult result = _installController.Step5(existingModel);

			// Assert
			Assert.That(_configReaderWriter.InstallStateReset, Is.True);

			string error = _installController.ModelState["An error ocurred installing"].Errors[0].ErrorMessage;
			Assert.That(error, Is.StringStarting(exceptionMessage), error);
		}

		[Test]
		public void Finalize_Should_Set_PublicSite_And_IgnoreSearchErrors_To_True()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			SetMockDataStoreType(existingModel);

			// Act
			_installController.FinalizeInstall(existingModel);

			// Assert
			Assert.That(existingModel.IgnoreSearchIndexErrors, Is.True);
			Assert.That(existingModel.IsPublicSite, Is.True);
		}

		[Test]
		public void Finalize_Should_Save_Config_Settings()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			existingModel.Theme = "Responsive"; // don't test everything, that's done elsewhere in the ConfigReaderWriterTests
			SetMockDataStoreType(existingModel);

			// Act
			_installController.FinalizeInstall(existingModel);

			// Assert
			Assert.That(_configReaderWriter.Saved, Is.True);
		}

		[Test]
		public void Finalize_Should_Install_And_Save_Site_Settings()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			existingModel.Theme = "ChewbaccaOnHolidayTheme";
			SetMockDataStoreType(existingModel);

			// Act
			_installController.FinalizeInstall(existingModel);

			// Assert
			RepositoryMock repository = (RepositoryMock) ObjectFactory.GetInstance<IRepository>();
			Assert.That(repository.Installed, Is.True);
			SiteSettings settings = _settingsService.GetSiteSettings();
			Assert.That(settings.Theme, Is.EqualTo("ChewbaccaOnHolidayTheme"));
		}

		[Test]
		public void Finalize_Should_Add_AdminUser_When_Windows_Auth_Is_False()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			existingModel.UseWindowsAuth = false;
			SetMockDataStoreType(existingModel);

			// Act
			_installController.FinalizeInstall(existingModel);

			// Assert
			UserViewModel adminUser = _userService.ListAdmins().FirstOrDefault();
			Assert.That(adminUser, Is.Not.Null);
		}

		[Test]
		public void UnattendedSetup_Should_Redirect_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _installController.Unattended("mock datastore", "fake connection string");

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void UnattendedSetup_Should_Add_Admin_User_And_Set_Default_Site_Settings()
		{
			// Arrange
			SettingsViewModel existingModel = new SettingsViewModel();
			SetMockDataStoreType(existingModel);

			// Act
			ActionResult result = _installController.Unattended("mock datastore", "fake connection string");

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult.Content, Is.EqualTo("Unattended installation complete"));

			UserViewModel adminUser = _userService.ListAdmins().FirstOrDefault(); // check admin
			Assert.That(adminUser, Is.Not.Null);

			ApplicationSettings appSettings = _configReaderWriter.ApplicationSettings; // check settings
			Assert.That(appSettings.DataStoreType.Name, Is.EqualTo("mock datastore"));
			Assert.That(appSettings.ConnectionString, Is.EqualTo("fake connection string"));
			Assert.That(appSettings.UseObjectCache, Is.True);
			Assert.That(appSettings.UseBrowserCache, Is.True);

			SiteSettings settings = _settingsService.GetSiteSettings();		
			Assert.That(settings.AllowedFileTypes, Is.EqualTo("jpg,png,gif,zip,xml,pdf"));
			Assert.That(settings.MarkupType, Is.EqualTo("Creole"));
			Assert.That(settings.Theme, Is.EqualTo("Responsive"));
			Assert.That(settings.SiteName, Is.EqualTo("my site"));
			Assert.That(settings.SiteUrl, Is.EqualTo("http://localhost"));
		}

		[Test]
		public void InstallerJsVars_Should_Return_View()
		{
			// Arrange

			// Act
			ActionResult result = _installController.InstallerJsVars();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();
		}
		
		[Test]
		public void InstallerJsVars_Should_Redirect_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _installController.InstallerJsVars();

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult.Content, Is.Empty);
		}

		/// <summary>
		/// Resets DataStoreType.AllTypes to have just one type, "mock datastore"
		/// </summary>
		/// <param name="existingModel"></param>
		private void SetMockDataStoreType(SettingsViewModel existingModel)
		{
			string typeName = typeof(RepositoryMock).AssemblyQualifiedName;
			DataStoreType mockDataStoreType = new DataStoreType("mock datastore", "mock", typeName);
			List<DataStoreType> datastoreTypes = new List<DataStoreType>() { mockDataStoreType };
			DataStoreType.AllTypes = datastoreTypes;

			existingModel.DataStoreTypeName = "mock datastore";
		}
	}
}