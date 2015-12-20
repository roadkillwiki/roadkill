using System;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.Controllers.SiteSettings
{
	[TestFixture]
	[Category("Unit")]
	public class SettingsControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private SettingsRepositoryMock _settingsRepository;
		private UserServiceMock _userService;
		private SettingsService _settingsService;
		private PageViewModelCache _pageCache;
		private ListCache _listCache;
		private SiteCache _siteCache;
		private MemoryCache _cache;
		private ConfigReaderWriterStub _configReaderWriter;

		private SettingsController _settingsController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
			_container.ClearCache();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.AttachmentsFolder = AppDomain.CurrentDomain.BaseDirectory;
			_context = _container.UserContext;
			_settingsRepository = _container.SettingsRepository;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_pageCache = _container.PageViewModelCache;
			_listCache = _container.ListCache;
			_siteCache = _container.SiteCache;
			_cache = _container.MemoryCache;
			_configReaderWriter = new ConfigReaderWriterStub();

			_settingsController = new SettingsController(_applicationSettings, _userService, _settingsService, _context, _siteCache, _configReaderWriter);
		}

		[Test]
		public void index_get_should_return_view_and_viewmodel()
		{
			// Arrange

			// Act
			ViewResult result = _settingsController.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
			SettingsViewModel model = result.ModelFromActionResult<SettingsViewModel>();
			Assert.That(model, Is.Not.Null, "model");
		}

		[Test]
		public void index_post_should_return_viewresult_and_save_settings()
		{
			// Arrange
			SettingsViewModel model = new SettingsViewModel();
			model.MenuMarkup = "some new markup";

			// Act
			ViewResult result = _settingsController.Index(model) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
			SettingsViewModel resultModel = result.ModelFromActionResult<SettingsViewModel>();
			Assert.That(resultModel, Is.Not.Null, "model");

			Assert.That(_settingsRepository.GetSiteSettings().MenuMarkup, Is.EqualTo("some new markup"));
		}

		[Test]
		public void index_post_should_accept_httppost_only()
		{
			// Arrange
			SettingsViewModel model = new SettingsViewModel();

			// Act
			ViewResult result = _settingsController.Index(model) as ViewResult;

			// Assert
			_settingsController.AssertHttpPostOnly(x => x.Index(model));
		}

		[Test]
		public void index_post_should_clear_site_cache()
		{
			// Arrange
			_siteCache.AddMenu("some menu");
			_siteCache.AddAdminMenu("admin menu");
			_siteCache.AddLoggedInMenu("logged in menu");

			SettingsViewModel model = new SettingsViewModel();

			// Act
			ViewResult result = _settingsController.Index(model) as ViewResult;

			// Assert
			Assert.That(_cache.Count(), Is.EqualTo(0));
		}
	}
}
