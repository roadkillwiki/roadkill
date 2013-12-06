using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
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

namespace Roadkill.Tests.Unit.Mvc.Attributes
{
	[TestFixture]
	[Category("Unit")]
	public class BrowserCacheAttributeTests
	{
		private PluginFactoryMock _pluginFactory;
		private RepositoryMock _repositoryMock;
		private readonly DateTime _pageCreatedDate = DateTime.Today;
		private readonly DateTime _pageModifiedDate = DateTime.Today;
		private readonly DateTime _pluginLastSavedDate = DateTime.Today;

		[SetUp]
		public void Setup()
		{
			_pluginFactory = new PluginFactoryMock();
			_repositoryMock = new RepositoryMock();
		}

		[Test]
		public void Should_Not_Set_ViewResult_If_Not_Installed()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = GetSettingsService();

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			attribute.ApplicationSettings.Installed = false;

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void Should_Not_Set_ViewResult_If_UseBrowserCache_Is_Disabled()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = GetSettingsService();

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			attribute.ApplicationSettings.UseBrowserCache = false;

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void Should_Not_Set_ViewResult_If_User_Is_Logged_In()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = GetSettingsService();

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			attribute.Context.CurrentUser = Guid.NewGuid().ToString();

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void Should_Have_200_Http_Status_Code_If_No_Modified_Since_Header()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = GetSettingsService();

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.HttpContext.Response.StatusCode, Is.EqualTo(200));
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void Should_Have_200_Http_Status_Code_If_PluginsSaved_After_Header_Last_Modified_Date()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = GetSettingsService();
			_repositoryMock.SiteSettings.PluginLastSaveDate = DateTime.UtcNow;

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);
			filterContext.HttpContext.Request.Headers.Add("If-Modified-Since", DateTime.Today.ToString("r"));

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.HttpContext.Response.StatusCode, Is.EqualTo(200));
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void Should_Have_304_Http_Status_Code_If_PluginsSaved_Is_Equal_To_Header_Last_Modified_Date()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = GetSettingsService();
			_repositoryMock.SiteSettings.PluginLastSaveDate = DateTime.Today.AddHours(1);

			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);
			filterContext.HttpContext.Request.Headers.Add("If-Modified-Since", DateTime.Today.AddHours(1).ToString("r"));

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.HttpContext.Response.StatusCode, Is.EqualTo(304));
			Assert.That(filterContext.Result, Is.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void Should_Have_304_Http_Status_Code_If_Response_Has_Modified_Since_Header_Matching_Page_Modified_Date()
		{
			// The file date and the browser date always match for a 304 status, the browser will never send back a more recent date,
			// i.e. "Has the file changed since this date I've stored for the last time it was changed?"
			// and *not* "has the file changed since the time right now?". 

			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
			attribute.SettingsService = GetSettingsService();

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

		private SettingsService GetSettingsService()
		{
			_repositoryMock.SiteSettings.PluginLastSaveDate = _pluginLastSavedDate;
			SettingsService service = new SettingsService(new ApplicationSettings(), _repositoryMock);
			return service;
		}

		private WikiController CreateWikiController(BrowserCacheAttribute attribute)
		{
			// Settings
			ApplicationSettings appSettings = new ApplicationSettings() { Installed = true, UseBrowserCache = true };
			UserContextStub userContext = new UserContextStub() { IsLoggedIn = false };

			// PageService
			PageViewModelCache pageViewModelCache = new PageViewModelCache(appSettings, CacheMock.RoadkillCache);
			ListCache listCache = new ListCache(appSettings, CacheMock.RoadkillCache);
			SiteCache siteCache = new SiteCache(appSettings, CacheMock.RoadkillCache);
			SearchServiceMock searchService = new SearchServiceMock(appSettings, _repositoryMock, _pluginFactory);
			PageHistoryService historyService = new PageHistoryService(appSettings, _repositoryMock, userContext, pageViewModelCache, _pluginFactory);
			PageService pageService = new PageService(appSettings, _repositoryMock, searchService, historyService, userContext, listCache, pageViewModelCache, siteCache, _pluginFactory);

			// WikiController
			SettingsService settingsService = new SettingsService(appSettings, _repositoryMock);
			UserServiceStub userManager = new UserServiceStub();
			WikiController wikiController = new WikiController(appSettings, userManager, pageService, userContext, settingsService);

			// Create a page that the request is for
			Page page = new Page() { Title = "title", ModifiedOn = _pageModifiedDate };
			_repositoryMock.AddNewPage(page, "text", "user", _pageCreatedDate);

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