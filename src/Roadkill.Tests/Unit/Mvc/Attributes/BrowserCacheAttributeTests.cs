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
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;

namespace Roadkill.Tests.Unit.Mvc.Attributes
{
	[TestFixture]
	[Category("Unit")]
	public class BrowserCacheAttributeTests
	{
		[Test]
		public void Should_Not_Set_ViewResult_If_Not_Installed()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
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
			WikiController controller = CreateWikiController(attribute);
			ResultExecutedContext filterContext = CreateContext(controller);

			// Act
			attribute.OnResultExecuted(filterContext);

			// Assert
			Assert.That(filterContext.HttpContext.Response.StatusCode, Is.EqualTo(200));
			Assert.That(filterContext.Result, Is.Not.TypeOf<HttpStatusCodeResult>());
		}

		[Test]
		public void Should_Have_304_Http_Status_Code_If_Response_Has_Modified_Since_Header_Matching_Page_Modified_Date()
		{
			// Arrange
			BrowserCacheAttribute attribute = new BrowserCacheAttribute();
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
			RoadkillContextStub userContext = new RoadkillContextStub() { IsLoggedIn = false };

			// PageManager
			RepositoryMock repository = new RepositoryMock();
			PageSummaryCache pageSummaryCache = new PageSummaryCache(appSettings, MemoryCache.Default);
			ListCache listCache = new ListCache(appSettings, MemoryCache.Default);
			SearchManagerMock searchManager = new SearchManagerMock(appSettings, repository);
			HistoryManager historyManager = new HistoryManager(appSettings, repository, userContext, pageSummaryCache);
			PageManager pageManager = new PageManager(appSettings, repository, searchManager, historyManager, userContext, listCache, pageSummaryCache);

			// WikiController
			SettingsManager settingsManager = new SettingsManager(appSettings, repository);
			UserManagerStub userManager = new UserManagerStub();
			WikiController wikiController = new WikiController(appSettings, userManager, pageManager, userContext, settingsManager);

			// Create a page that the request is for
			Page page = new Page() { Title = "title", ModifiedOn = DateTime.Today };
			repository.AddNewPage(page, "text", "user", DateTime.UtcNow);

			// Update the BrowserCacheAttribute
			attribute.ApplicationSettings = appSettings;
			attribute.Context = userContext;
			attribute.PageManager = pageManager;

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