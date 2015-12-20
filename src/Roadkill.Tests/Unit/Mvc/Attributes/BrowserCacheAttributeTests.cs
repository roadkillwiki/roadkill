using System;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Tests.Unit.StubsAndMocks;
using Roadkill.Tests.Unit.StubsAndMocks.Mvc;

namespace Roadkill.Tests.Unit.Mvc.Attributes
{
	[TestFixture]
	[Category("Unit")]
	public class BrowserCacheAttributeTests
	{
		private PluginFactoryMock _pluginFactory;
		private SettingsRepositoryMock _settingsRepository;
		private PageRepositoryMock _pageRepository;

		private readonly DateTime _pageCreatedDate = DateTime.Today;
		private readonly DateTime _pageModifiedDate = DateTime.Today;
		private readonly DateTime _pluginLastSavedDate = DateTime.Today;
		private MocksAndStubsContainer _container;
		private SettingsService _settingsService;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_container.SettingsRepository.SiteSettings.PluginLastSaveDate = _pluginLastSavedDate;

			_pluginFactory = _container.PluginFactory;
			_settingsRepository = _container.SettingsRepository;
			_pageRepository = _container.PageRepository;

			_settingsService = _container.SettingsService;
		}

		[Test]
		public void should_not_set_viewresult_if_not_installed()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = _settingsService;

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			attribute.ApplicationSettings.Installed = false;

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void should_not_set_viewresult_if_usebrowsercache_is_disabled()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = _settingsService;

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			attribute.ApplicationSettings.UseBrowserCache = false;

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void should_not_set_viewresult_if_user_is_logged_in()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = _settingsService;

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			attribute.Context.CurrentUser = Guid.NewGuid().ToString();

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void should_have_200_http_status_code_if_no_modified_since_header()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = _settingsService;

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.HttpContext.Response.StatusCode, Is.EqualTo(200));
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void should_have_200_http_status_code_if_pluginssaved_after_header_last_modified_date()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = _settingsService;
			_settingsRepository.SiteSettings.PluginLastSaveDate = DateTime.UtcNow;

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);
			filterContext.HttpContext.Request.Headers.Add("If-Modified-Since", DateTime.Today.ToUniversalTime().ToString("r"));

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.HttpContext.Response.StatusCode, Is.EqualTo(200));
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void should_have_304_http_status_code_if_pluginssaved_is_equal_to_header_last_modified_date()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = _settingsService;
			_settingsRepository.SiteSettings.PluginLastSaveDate = DateTime.Today.ToUniversalTime().AddHours(1);

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);
			filterContext.HttpContext.Request.Headers.Add("If-Modified-Since", DateTime.Today.AddHours(1).ToUniversalTime().ToString("r"));

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.HttpContext.Response.StatusCode, Is.EqualTo(304));
			Assert.That(filterContext.Result, Is.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void should_have_304_http_status_code_if_response_has_modified_since_header_matching_page_modified_date()
		{
			// The file date and the browser date always match for a 304 status, the browser will never send back a more recent date,
			// i.e. "Has the file changed since this date I've stored for the last time it was changed?"
			// and *not* "has the file changed since the time right now?". 

			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = _settingsService;

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			// (page modified date is set in CreateWikiController)
			filterContext.HttpContext.Request.Headers.Add("If-Modified-Since", DateTime.Today.ToString("r"));

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			HttpStatusCodeResult result = filterContext.Result as HttpStatusCodeResult;
			Assert.That(filterContext.HttpContext.Response.StatusCode, Is.EqualTo(304));
			Assert.That(result.StatusCode, Is.EqualTo(304));
			Assert.That(result.StatusDescription, Is.EqualTo("Not Modified"));
		}

		private WikiController CreateWikiController(BrowserCacheAttribute attribute)
		{
			// Settings
			ApplicationSettings appSettings = new ApplicationSettings() { Installed = true, UseBrowserCache = true };
			UserContextStub userContext = new UserContextStub() { IsLoggedIn = false };

			// PageService
			PageViewModelCache pageViewModelCache = new PageViewModelCache(appSettings, CacheMock.RoadkillCache);
			ListCache listCache = new ListCache(appSettings, CacheMock.RoadkillCache);
			SiteCache siteCache = new SiteCache(CacheMock.RoadkillCache);
			SearchServiceMock searchService = new SearchServiceMock(appSettings, _settingsRepository, _pageRepository, _pluginFactory);
			PageHistoryService historyService = new PageHistoryService(appSettings, _settingsRepository, _pageRepository, userContext, pageViewModelCache, _pluginFactory);
			PageService pageService = new PageService(appSettings, _settingsRepository, _pageRepository, searchService, historyService, userContext, listCache, pageViewModelCache, siteCache, _pluginFactory);

			// WikiController
			SettingsService settingsService = new SettingsService(new RepositoryFactoryMock(), appSettings);
			UserServiceStub userManager = new UserServiceStub();
			WikiController wikiController = new WikiController(appSettings, userManager, pageService, userContext, settingsService);

			// Create a page that the request is for
			Page page = new Page() { Title = "title", ModifiedOn = _pageModifiedDate };
			_pageRepository.AddNewPage(page, "text", "user", _pageCreatedDate);

			// Update the BrowserCacheAttribute
			attribute.ApplicationSettings = appSettings;
			attribute.Context = userContext;
			attribute.PageService = pageService;

			return wikiController;
		}

		private ResultExecutedContext CreateContext(WikiController wikiController)
		{
			// HTTP Context
			ControllerContext controllerContext = new Mock<ControllerContext>().Object;
			MvcMockContainer container = new MvcMockContainer();
			HttpContextBase context = MvcMockHelpers.FakeHttpContext(container);
			controllerContext.HttpContext = context;

			// ResultExecutedContext
			ActionResult result = new ViewResult();
			Exception exception = new Exception();
			bool cancelled = true;

			ResultExecutedContext filterContext = new ResultExecutedContext(controllerContext, result, cancelled, exception);
			filterContext.Controller = wikiController;
			filterContext.RouteData.Values.Add("id", 1);
			filterContext.HttpContext = context;

			return filterContext;
		}
	}
}