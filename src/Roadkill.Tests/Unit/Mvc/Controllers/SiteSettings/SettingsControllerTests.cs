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
using Roadkill.Tests.Unit.StubsAndMocks;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using MvcContrib.TestHelper;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class SettingsControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private SettingsService _settingsService;
		private PageViewModelCache _pageCache;
		private ListCache _listCache;
		private SiteCache _siteCache;
		private MemoryCache _cache;

		private SettingsController _settingsController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.AttachmentsFolder = AppDomain.CurrentDomain.BaseDirectory;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_pageCache = _container.PageViewModelCache;
			_listCache = _container.ListCache;
			_siteCache = _container.SiteCache;
			_cache = _container.MemoryCache;
			var config = new FullTrustConfigReaderWriter(AppDomain.CurrentDomain.BaseDirectory + @"\Roadkill.Tests.dll.config");

			_settingsController = new SettingsController(_applicationSettings, _userService, _settingsService, _context, _siteCache, config);
		}

		[Test]
		public void Index_GET_Should_Return_View_And_ViewModel()
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
		public void Index_POST_Should_Return_View_And_ViewModel()
		{
			// Arrange
			SettingsViewModel model = new SettingsViewModel();

			// Act
			ViewResult result = _settingsController.Index(model) as ViewResult;

			// Assert
			_settingsController.AssertIsOnlyHttpPost(x => x.Index(model));
			
			Assert.That(result, Is.Not.Null, "ViewResult");
			SettingsViewModel resultModel = result.ModelFromActionResult<SettingsViewModel>();
			Assert.That(resultModel, Is.Not.Null, "model");
		}

		[Test]
		public void TestAttachments_Should_Return_Json_Result()
		{
		}

		[Test]
		public void TestDatabaseConnection_Should_Return_Json_Result()
		{
		}
	}
}
