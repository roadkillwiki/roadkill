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

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class InstallControllerTests
	{
		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;

		private UserServiceBase _userService;
		private PageService _pageService;
		private SearchServiceMock _searchService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Setup()
		{
			_context = new Mock<IUserContext>().Object;
			_applicationSettings = new ApplicationSettings();
			_applicationSettings.Installed = false;

			// Cache
			ListCache listCache = new ListCache(_applicationSettings, CacheMock.RoadkillCache);
			SiteCache siteCache = new SiteCache(_applicationSettings, CacheMock.RoadkillCache);
			PageViewModelCache pageViewModelCache = new PageViewModelCache(_applicationSettings, CacheMock.RoadkillCache);

			// Dependencies for PageService
			Mock<SearchService> searchMock = new Mock<SearchService>();
			_pluginFactory = new PluginFactoryMock();

			_repository = new RepositoryMock();
			_settingsService = new SettingsService(_applicationSettings, _repository);
			_userService = new Mock<UserServiceBase>(_applicationSettings, null).Object;
			_searchService = new SearchServiceMock(_applicationSettings, _repository, _pluginFactory);
			_searchService.PageContents = _repository.PageContents;
			_searchService.Pages = _repository.Pages;
			_historyService = new PageHistoryService(_applicationSettings, _repository, _context, pageViewModelCache, _pluginFactory);
			_pageService = new PageService(_applicationSettings, _repository, _searchService, _historyService, _context, listCache, pageViewModelCache, siteCache, _pluginFactory);
		}

		[Test]
		public void Index__Should_Return_ViewResult_And_Model_With_LanguageModels_And_Set_UILanguage_To_English()
		{
			// Arrange
			InstallController controller = new InstallController(_applicationSettings, _userService, _pageService,
													_searchService, _repository, _settingsService, _context);

			// Act
			ViewResult result = controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			IEnumerable<LanguageViewModel> models = result.ModelFromActionResult<IEnumerable<LanguageViewModel>>();
			Assert.NotNull(models, "Null model");
			Assert.That(models.Count(), Is.GreaterThanOrEqualTo(1));
			Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo("en"));
		}

		[Test]
		public void Step1_Should_Return_ViewResult_With_Language_Summary_And_Set_UICulture_From_Language()
		{
			// Arrange
			InstallController controller = new InstallController(_applicationSettings, _userService, _pageService,
													_searchService, _repository, _settingsService, _context);

			// Act
			ViewResult result = controller.Step1("hi") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			LanguageViewModel model = result.ModelFromActionResult<LanguageViewModel>();
			Assert.NotNull(model, "Null model");
			Assert.That(model.Code, Is.EqualTo("hi"));

			Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo("hi"));
		}
	}
}