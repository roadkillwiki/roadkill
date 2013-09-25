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
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Cache
{
	[TestFixture]
	[Category("Unit")]
	public class PageManagerCacheTests
	{
		[Test]
		public void GetById_Should_Add_To_Cache_When_PageSummary_Does_Not_Exist_In_Cache()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			CacheMock summaryCache = new CacheMock();
			PageManager pageManager = CreatePageManager(summaryCache, null, repository);

			PageSummary expectedSummary = CreatePageSummary();
			expectedSummary = pageManager.AddPage(expectedSummary); // get it back to update the version no.

			// Act
			pageManager.GetById(1);

			// Assert
			CacheItem cacheItem = summaryCache.CacheItems.First();
			string cacheKey = CacheKeys.PageSummaryKey(1, PageSummaryCache.LATEST_VERSION_NUMBER);
			Assert.That(cacheItem.Key, Is.EqualTo(cacheKey));

			PageSummary actualSummary = (PageSummary) cacheItem.Value;
			Assert.That(actualSummary.Id, Is.EqualTo(expectedSummary.Id));
			Assert.That(actualSummary.VersionNumber, Is.EqualTo(expectedSummary.VersionNumber));
			Assert.That(actualSummary.Title, Is.EqualTo(expectedSummary.Title));
		}

		[Test]
		public void GetById_Should_Load_From_Cache_When_PageSummary_Exists_In_Cache()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			CacheMock summaryCache = new CacheMock();
			PageManager pageManager = CreatePageManager(summaryCache, null, repository);

			PageSummary expectedSummary = CreatePageSummary();
			string cacheKey = CacheKeys.PageSummaryKey(1, PageSummaryCache.LATEST_VERSION_NUMBER);
			summaryCache.Add(cacheKey, expectedSummary, new CacheItemPolicy());

			// Act
			PageSummary actualSummary = pageManager.GetById(1);

			// Assert
			Assert.That(actualSummary.Id, Is.EqualTo(expectedSummary.Id));
			Assert.That(actualSummary.VersionNumber, Is.EqualTo(expectedSummary.VersionNumber));
			Assert.That(actualSummary.Title, Is.EqualTo(expectedSummary.Title));
		}

		[Test]
		public void AddPage_Should_Clear_List_And_PageSummary_Caches()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			CacheMock summaryCache = new CacheMock();
			CacheMock listCache = new CacheMock();

			PageManager pageManager = CreatePageManager(summaryCache, listCache, repository);
			PageSummary expectedSummary = CreatePageSummary();
			summaryCache.Add("key", expectedSummary, new CacheItemPolicy());
			listCache.Add("key", new List<string>() { "tag1", "tag2" }, new CacheItemPolicy());

			// Act
			pageManager.AddPage(new PageSummary() { Title = "totoro" });

			// Assert
			Assert.That(summaryCache.CacheItems.Count, Is.EqualTo(0));
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(0));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void AllPages_Should_Load_From_Cache(bool loadPageContent)
		{
			string cacheKey = (loadPageContent) ? (CacheKeys.ALLPAGES_CONTENT) : (CacheKeys.ALLPAGES);

			// Arrange
			RepositoryMock repository = new RepositoryMock();
			CacheMock listCache = new CacheMock();

			PageManager pageManager = CreatePageManager(null, listCache, repository);
			PageSummary expectedSummary = CreatePageSummary();
			listCache.Add(cacheKey, new List<PageSummary>() {expectedSummary}, new CacheItemPolicy());

			// Act
			IEnumerable<PageSummary> actualList = pageManager.AllPages(loadPageContent);

			// Assert
			Assert.That(actualList, Contains.Item(expectedSummary));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void AllPages_Should_Add_To_Cache_When_Cache_Is_Empty(bool loadPageContent)
		{
			// Arrange
			string cacheKey = (loadPageContent) ? (CacheKeys.ALLPAGES_CONTENT) : (CacheKeys.ALLPAGES);

			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page() { Title = "1" }, "text", "admin", DateTime.UtcNow);
			repository.AddNewPage(new Page() { Title = "2" }, "text", "admin", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageManager pageManager = CreatePageManager(null, listCache, repository);

			// Act
			pageManager.AllPages(loadPageContent);

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(cacheKey));
		}

		[Test]
		public void AllPagesCreatedBy_Should_Load_From_Cache()
		{
			string adminCacheKey = CacheKeys.AllPagesCreatedByKey("admin");
			string editorCacheKey = CacheKeys.AllPagesCreatedByKey("editor");

			// Arrange
			RepositoryMock repository = new RepositoryMock();
			CacheMock listCache = new CacheMock();

			PageManager pageManager = CreatePageManager(null, listCache, repository);
			PageSummary adminSummary = CreatePageSummary();
			PageSummary editorSummary = CreatePageSummary("editor");
			listCache.Add(adminCacheKey, new List<PageSummary>() { adminSummary }, new CacheItemPolicy());
			listCache.Add(editorCacheKey, new List<PageSummary>() { editorSummary }, new CacheItemPolicy());

			// Act
			IEnumerable<PageSummary> actualList = pageManager.AllPagesCreatedBy("admin");

			// Assert
			Assert.That(actualList, Contains.Item(adminSummary));
			Assert.That(actualList, Is.Not.Contains(editorSummary));

		}

		[Test]
		public void AllPagesCreatedBy_Should_Add_To_Cache_When_Cache_Is_Empty()
		{
			// Arrange
			string adminCacheKey = CacheKeys.AllPagesCreatedByKey("admin");

			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page() { Title = "1" }, "text", "admin", DateTime.UtcNow);
			repository.AddNewPage(new Page() { Title = "2" }, "text", "admin", DateTime.UtcNow);
			repository.AddNewPage(new Page() { Title = "3" }, "text", "editor", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageManager pageManager = CreatePageManager(null, listCache, repository);

			// Act
			pageManager.AllPagesCreatedBy("admin");

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(adminCacheKey));
		}

		[Test]
		public void AllTags_Should_Load_From_Cache()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			CacheMock listCache = new CacheMock();

			PageManager pageManager = CreatePageManager(null, listCache, repository);
			List<string> expectedTags = new List<string>() { "tag1", "tag2", "tag3" };
			listCache.Add(CacheKeys.ALLTAGS, expectedTags, new CacheItemPolicy());

			// Act
			IEnumerable<string> actualTags = pageManager.AllTags().Select(x => x.Name);

			// Assert
			Assert.That(actualTags, Is.SubsetOf(expectedTags));
		}

		[Test]
		public void AllTags_Should_Add_To_Cache_When_Cache_Is_Empty()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page() { Tags = "tag1;tag2" }, "text", "admin", DateTime.UtcNow);
			repository.AddNewPage(new Page() { Tags = "tag3;tag4" }, "text", "admin", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageManager pageManager = CreatePageManager(null, listCache, repository);

			// Act
			pageManager.AllTags();

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(CacheKeys.ALLTAGS));
		}

		[Test]
		public void DeletePage_Should_Clear_List_And_PageSummary_Caches()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page(), "text", "admin", DateTime.UtcNow);
			CacheMock summaryCache = new CacheMock();
			CacheMock listCache = new CacheMock();

			PageManager pageManager = CreatePageManager(summaryCache, listCache, repository);
			PageSummary expectedSummary = CreatePageSummary();
			summaryCache.Add("key", expectedSummary, new CacheItemPolicy());
			listCache.Add("key", new List<string>() { "tag1", "tag2" }, new CacheItemPolicy());

			// Act
			pageManager.DeletePage(1);

			// Assert
			Assert.That(summaryCache.CacheItems.Count, Is.EqualTo(0));
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(0));
		}

		[Test]
		public void FindHomePage_Should_Load_From_Cache()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			CacheMock summaryCache = new CacheMock();

			PageManager pageManager = CreatePageManager(summaryCache, null, repository);
			PageSummary expectedSummary = CreatePageSummary();
			expectedSummary.RawTags = "homepage";
			summaryCache.Add(CacheKeys.HOMEPAGE, expectedSummary, new CacheItemPolicy());

			// Act
			PageSummary actualSummary = pageManager.FindHomePage();

			// Assert
			Assert.That(actualSummary.Id, Is.EqualTo(expectedSummary.Id));
			Assert.That(actualSummary.VersionNumber, Is.EqualTo(expectedSummary.VersionNumber));
			Assert.That(actualSummary.Title, Is.EqualTo(expectedSummary.Title));
		}

		[Test]
		public void FindHomePage_Should_Add_To_Cache_When_Cache_Is_Empty()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page() { Title = "1", Tags= "homepage" }, "text", "admin", DateTime.UtcNow);

			CacheMock summaryCache = new CacheMock();
			PageManager pageManager = CreatePageManager(summaryCache, null, repository);

			// Act
			pageManager.FindHomePage();

			// Assert
			Assert.That(summaryCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(summaryCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(CacheKeys.HOMEPAGE));
		}

		[Test]
		public void FindByTag_Should_Load_From_Cache()
		{
			string tag1CacheKey = CacheKeys.PagesByTagKey("tag1");
			string tag2CacheKey = CacheKeys.PagesByTagKey("tag2");

			// Arrange
			RepositoryMock repository = new RepositoryMock();
			CacheMock listCache = new CacheMock();

			PageManager pageManager = CreatePageManager(null, listCache, repository);
			PageSummary tag1Summary = CreatePageSummary();
			tag1Summary.RawTags = "tag1";
			PageSummary tag2Summary = CreatePageSummary();
			tag2Summary.RawTags = "tag2";

			listCache.Add(tag1CacheKey, new List<PageSummary>() { tag1Summary }, new CacheItemPolicy());
			listCache.Add(tag2CacheKey, new List<PageSummary>() { tag2Summary }, new CacheItemPolicy());

			// Act
			IEnumerable<PageSummary> actualList = pageManager.FindByTag("tag1");

			// Assert
			Assert.That(actualList, Contains.Item(tag1Summary));
			Assert.That(actualList, Is.Not.Contains(tag2Summary));

		}

		[Test]
		public void FindByTag_Should_Add_To_Cache_When_Cache_Is_Empty()
		{
			// Arrange
			string cacheKey = CacheKeys.PagesByTagKey("tag1");

			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page() { Title = "1", Tags = "tag1" }, "text", "admin", DateTime.UtcNow);
			repository.AddNewPage(new Page() { Title = "2", Tags = "tag2" }, "text", "admin", DateTime.UtcNow);
			repository.AddNewPage(new Page() { Title = "2", Tags = "tag3" }, "text", "admin", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageManager pageManager = CreatePageManager(null, listCache, repository);

			// Act
			pageManager.FindByTag("tag1");

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(cacheKey));
		}

		[Test]
		public void UpdatePage_Should_Clear_List_Cache_And_PageSummary_Cache()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page() { Tags = "homepage" }, "text", "admin", DateTime.UtcNow);
			repository.AddNewPage(new Page() { Tags = "tag2" }, "text", "admin", DateTime.UtcNow);

			CacheMock summaryCache = new CacheMock();
			CacheMock listCache = new CacheMock();
			PageManager pageManager = CreatePageManager(summaryCache, listCache, repository);

			PageSummary homepageSummary = CreatePageSummary();
			homepageSummary.Id = 1;		
			PageSummary page2Summary = CreatePageSummary();
			page2Summary.Id = 2;

			summaryCache.Add(CacheKeys.HOMEPAGE, homepageSummary, new CacheItemPolicy());
			summaryCache.Add(CacheKeys.PageSummaryKey(2,0), page2Summary, new CacheItemPolicy());
			listCache.Add(CacheKeys.ALLTAGS, new List<string>() { "tag1", "tag2" }, new CacheItemPolicy());

			// Act
			pageManager.UpdatePage(page2Summary);

			// Assert
			Assert.That(summaryCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(0));
		}

		[Test]
		public void UpdatePage_Should_Remove_Homepage_From_Cache_When_Homepage_Is_Updated()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page() { Tags = "homepage" }, "text", "admin", DateTime.UtcNow);

			CacheMock summaryCache = new CacheMock();
			CacheMock listCache = new CacheMock();
			PageManager pageManager = CreatePageManager(summaryCache, listCache, repository);

			PageSummary homepageSummary = CreatePageSummary();
			homepageSummary.RawTags = "homepage";
			summaryCache.Add(CacheKeys.HOMEPAGE, homepageSummary, new CacheItemPolicy());

			// Act
			pageManager.UpdatePage(homepageSummary);

			// Assert
			Assert.That(summaryCache.CacheItems.Count, Is.EqualTo(0));
		}

		[Test]
		public void RenameTag_Should_Clear_ListCache()
		{
			// Arrange
			string tag1CacheKey = CacheKeys.PagesByTagKey("tag1");
			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page() { Tags = "homepage, tag1" }, "text1", "admin", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageSummary homepageSummary = CreatePageSummary();
			PageSummary page1Summary = CreatePageSummary();
			listCache.Add(tag1CacheKey, new List<PageSummary>() { homepageSummary, page1Summary }, new CacheItemPolicy());

			PageManager pageManager = CreatePageManager(null, listCache, repository);

			// Act
			pageManager.RenameTag("tag1", "some.other.tag"); // calls UpdatePage, which clears the cache

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(0));
		}

		private PageSummary CreatePageSummary(string createdBy = "admin")
		{
			PageSummary summary = new PageSummary();
			summary.Title = "my title";
			summary.Id = 1;
			summary.CreatedBy = createdBy;
			summary.VersionNumber = PageSummaryCache.LATEST_VERSION_NUMBER;

			return summary;
		}

		private PageManager CreatePageManager(ObjectCache summaryObjectCache, ObjectCache listObjectCache, RepositoryMock repository)
		{
			// Stick to memorycache when each one isn't used
			if (summaryObjectCache == null)
				summaryObjectCache = MemoryCache.Default;

			if (listObjectCache == null)
				listObjectCache = MemoryCache.Default;

			// Settings
			ApplicationSettings appSettings = new ApplicationSettings() { Installed = true, UseObjectCache = true };
			RoadkillContextStub userContext = new RoadkillContextStub() { IsLoggedIn = false };

			// PageManager
			PageSummaryCache pageSummaryCache = new PageSummaryCache(appSettings, summaryObjectCache);
			ListCache listCache = new ListCache(appSettings, listObjectCache);
			SiteCache siteCache = new SiteCache(appSettings, MemoryCache.Default);
			SearchManagerMock searchManager = new SearchManagerMock(appSettings, repository);
			HistoryManager historyManager = new HistoryManager(appSettings, repository, userContext, pageSummaryCache);
			PageManager pageManager = new PageManager(appSettings, repository, searchManager, historyManager, userContext, listCache, pageSummaryCache, siteCache);

			return pageManager;
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