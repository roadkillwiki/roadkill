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
using Roadkill.Tests.Unit.StubsAndMocks;
using MvcContrib.TestHelper;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class HomeControllerTests
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
		private ListCache _listCache;
		private SiteCache _siteCache;
		private PageViewModelCache _pageViewModelCache;
		private MemoryCache _memoryCache;
		private MarkupConverter _markupConverter;

		private HomeController _homeController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;
			_searchService = _container.SearchService;
			_markupConverter = _container.MarkupConverter;

			_listCache = _container.ListCache;
			_siteCache = _container.SiteCache;
			_pageViewModelCache = _container.PageViewModelCache;
			_memoryCache = _container.MemoryCache;

			_homeController = new HomeController(_applicationSettings, _userService, _markupConverter, _pageService, _searchService, _context, _settingsService);
			_homeController.SetFakeControllerContext();
		}

		[Test]
		public void Index_Should_Return_Default_Message_When_No_Homepage_Tag_Exists()
		{
			// Arrange

			// Act
			ActionResult result = _homeController.Index();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			PageViewModel model = result.ModelFromActionResult<PageViewModel>();
			Assert.NotNull(model, "Null model");
			Assert.That(model.Title, Is.EqualTo(SiteStrings.NoMainPage_Title));
			Assert.That(model.Content, Is.EqualTo(SiteStrings.NoMainPage_Label));
		}

		[Test]
		public void Index_Should_Return_Homepage_When_Tag_Exists()
		{
			// Arrange
			Page page1 = new Page() 
			{ 
				Id = 1, 
				Tags = "homepage, tag2", 
				Title = "Welcome to the site" 
			};
			PageContent page1Content = new PageContent() 
			{ 
				Id = Guid.NewGuid(), 
				Page = page1, 
				Text = "Hello world" 
			};
			_repository.Pages.Add(page1);
			_repository.PageContents.Add(page1Content);

			// Act
			ActionResult result = _homeController.Index();
			
			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			PageViewModel model = result.ModelFromActionResult<PageViewModel>();
			Assert.NotNull(model, "Null model");
			Assert.That(model.Title, Is.EqualTo(page1.Title));
			Assert.That(model.Content, Is.EqualTo(page1Content.Text));
		}

		[Test]
		public void Search_Should_Return_Some_Results_With_Unicode_Content()
		{
			// Arrange
			Page page1 = new Page()
			{
				Id = 1,
				Tags = "homepage, tag2",
				Title = "ОШИБКА: неверная последовательность байт для кодировки"
			};
			PageContent page1Content = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page1,
				Text = "БД сервера событий была перенесена из PostgreSQL 8.3 на PostgreSQL 9.1.4. Сервер, развернутый на Windows платформе"
			};
			_repository.Pages.Add(page1);
			_repository.PageContents.Add(page1Content);

			// Act
			ActionResult result = _homeController.Search("ОШИБКА: неверная последовательность байт для кодировки");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			List<SearchResultViewModel> searchResults = result.ModelFromActionResult<IEnumerable<SearchResultViewModel>>().ToList();
			Assert.NotNull(searchResults, "Null model");
			Assert.That(searchResults.Count(), Is.EqualTo(1));
			Assert.That(searchResults[0].Title, Is.EqualTo(page1.Title));
			Assert.That(searchResults[0].ContentSummary, Contains.Substring(page1Content.Text));
		}

		[Test]
		public void GlobalJsVars_Should_Return_View()
		{
			// Arrange

			// Act
			ActionResult result = _homeController.GlobalJsVars("2.0");

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();
		}

		[Test]
		public void NavMenu_Should_Return_View()
		{
			// Arrange

			// Act
			ActionResult result = _homeController.NavMenu();

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult.Content, Is.Not.Empty);
		}

		[Test]
		public void BootstrapNavMenu_Should_Return_View()
		{
			// Arrange

			// Act
			ActionResult result = _homeController.BootstrapNavMenu();

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult.Content, Is.Not.Empty);
		}

		[Test]
		public void LeftMenu_Should_Return_Content()
		{
			// Arrange

			// Act
			ActionResult result = _homeController.LeftMenu();

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult.Content, Is.Not.Empty);
		}
	}
}
