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
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;
using Roadkill.Tests.Unit.StubsAndMocks.Mvc;

namespace Roadkill.Tests.Unit.Cache
{
	[TestFixture]
	[Category("Unit")]
	public class PageServiceCacheTests
	{
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Setup()
		{
			_pluginFactory = new PluginFactoryMock();
		}

		[Test]
		public void getbyid_should_add_to_cache_when_pagesummary_does_not_exist_in_cache()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			CacheMock pageModelCache = new CacheMock();
			PageService pageService = CreatePageService(pageModelCache, null, settingsRepository, pageRepository);

			PageViewModel expectedModel = CreatePageViewModel();
			expectedModel = pageService.AddPage(expectedModel); // get it back to update the version no.

			// Act
			pageService.GetById(1);

			// Assert
			CacheItem cacheItem = pageModelCache.CacheItems.First();
			string cacheKey = CacheKeys.PageViewModelKey(1, PageViewModelCache.LATEST_VERSION_NUMBER);
			Assert.That(cacheItem.Key, Is.EqualTo(cacheKey));

			PageViewModel actualModel = (PageViewModel)cacheItem.Value;
			Assert.That(actualModel.Id, Is.EqualTo(expectedModel.Id));
			Assert.That(actualModel.VersionNumber, Is.EqualTo(expectedModel.VersionNumber));
			Assert.That(actualModel.Title, Is.EqualTo(expectedModel.Title));
		}

		[Test]
		public void getbyid_should_load_from_cache_when_pagesummary_exists_in_cache()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			CacheMock pageModelCache = new CacheMock();
			PageService pageService = CreatePageService(pageModelCache, null, settingsRepository, pageRepository);

			PageViewModel expectedModel = CreatePageViewModel();
			string cacheKey = CacheKeys.PageViewModelKey(1, PageViewModelCache.LATEST_VERSION_NUMBER);
			pageModelCache.Add(cacheKey, expectedModel, new CacheItemPolicy());

			// Act
			PageViewModel actualModel = pageService.GetById(1);

			// Assert
			Assert.That(actualModel.Id, Is.EqualTo(expectedModel.Id));
			Assert.That(actualModel.VersionNumber, Is.EqualTo(expectedModel.VersionNumber));
			Assert.That(actualModel.Title, Is.EqualTo(expectedModel.Title));
		}

		[Test]
		public void addpage_should_clear_list_and_pagesummary_caches()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			CacheMock pageModelCache = new CacheMock();
			CacheMock listCache = new CacheMock();

			PageService pageService = CreatePageService(pageModelCache, listCache, settingsRepository, pageRepository);
			PageViewModel expectedModel = CreatePageViewModel();
			AddPageCacheItem(pageModelCache, "key", expectedModel);
			AddListCacheItem(listCache, "key", new List<string>() { "tag1", "tag2" });

			// Act
			pageService.AddPage(new PageViewModel() { Title = "totoro" });

			// Assert
			Assert.That(pageModelCache.CacheItems.Count, Is.EqualTo(0));
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(0));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void AllPages_Should_Load_From_Cache(bool loadPageContent)
		{
			string cacheKey = (loadPageContent) ? (CacheKeys.AllPagesWithContent()) : (CacheKeys.AllPages());

			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			CacheMock listCache = new CacheMock();

			PageService pageService = CreatePageService(null, listCache, settingsRepository, pageRepository);
			PageViewModel expectedModel = CreatePageViewModel();
			AddListCacheItem(listCache, cacheKey, new List<PageViewModel>() { expectedModel });

			// Act
			IEnumerable<PageViewModel> actualList = pageService.AllPages(loadPageContent);

			// Assert
			Assert.That(actualList, Contains.Item(expectedModel));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void AllPages_Should_Add_To_Cache_When_Cache_Is_Empty(bool loadPageContent)
		{
			// Arrange
			string cacheKey = (loadPageContent) ? (CacheKeys.AllPagesWithContent()) : (CacheKeys.AllPages());

			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			pageRepository.AddNewPage(new Page() { Title = "1" }, "text", "admin", DateTime.UtcNow);
			pageRepository.AddNewPage(new Page() { Title = "2" }, "text", "admin", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageService pageService = CreatePageService(null, listCache, settingsRepository, pageRepository);

			// Act
			pageService.AllPages(loadPageContent);

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(cacheKey));
		}

		[Test]
		public void allpagescreatedby_should_load_from_cache()
		{
			string adminCacheKey = CacheKeys.AllPagesCreatedByKey("admin");
			string editorCacheKey = CacheKeys.AllPagesCreatedByKey("editor");

			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			CacheMock listCache = new CacheMock();

			PageService pageService = CreatePageService(null, listCache, settingsRepository, pageRepository);
			PageViewModel adminModel = CreatePageViewModel();
			PageViewModel editorModel = CreatePageViewModel("editor");
			listCache.Add(CacheKeys.AllPagesCreatedByKey("admin"), new List<PageViewModel>() { adminModel }, new CacheItemPolicy());
			listCache.Add(CacheKeys.AllPagesCreatedByKey("editor"), new List<PageViewModel>() { editorModel }, new CacheItemPolicy());

			// Act
			IEnumerable<PageViewModel> actualList = pageService.AllPagesCreatedBy("admin");

			// Assert
			Assert.That(actualList, Contains.Item(adminModel));
			Assert.That(actualList, Is.Not.Contains(editorModel));

		}

		[Test]
		public void allpagescreatedby_should_add_to_cache_when_cache_is_empty()
		{
			// Arrange
			string adminCacheKey = CacheKeys.AllPagesCreatedByKey("admin");

			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			pageRepository.AddNewPage(new Page() { Title = "1" }, "text", "admin", DateTime.UtcNow);
			pageRepository.AddNewPage(new Page() { Title = "2" }, "text", "admin", DateTime.UtcNow);
			pageRepository.AddNewPage(new Page() { Title = "3" }, "text", "editor", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageService pageService = CreatePageService(null, listCache, settingsRepository, pageRepository);

			// Act
			pageService.AllPagesCreatedBy("admin");

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(adminCacheKey));
		}

		[Test]
		public void alltags_should_load_from_cache()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			CacheMock listCache = new CacheMock();

			PageService pageService = CreatePageService(null, listCache, settingsRepository, pageRepository);
			List<string> expectedTags = new List<string>() { "tag1", "tag2", "tag3" };
			AddListCacheItem(listCache, CacheKeys.AllTags(), expectedTags);

			// Act
			IEnumerable<string> actualTags = pageService.AllTags().Select(x => x.Name);

			// Assert
			Assert.That(actualTags, Is.SubsetOf(expectedTags));
		}

		[Test]
		public void alltags_should_add_to_cache_when_cache_is_empty()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			pageRepository.AddNewPage(new Page() { Tags = "tag1;tag2" }, "text", "admin", DateTime.UtcNow);
			pageRepository.AddNewPage(new Page() { Tags = "tag3;tag4" }, "text", "admin", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageService pageService = CreatePageService(null, listCache, settingsRepository, pageRepository);

			// Act
			pageService.AllTags();

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(CacheKeys.AllTags()));
		}

		[Test]
		public void deletepage_should_clear_list_and_pagesummary_caches()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			pageRepository.AddNewPage(new Page(), "text", "admin", DateTime.UtcNow);
			CacheMock pageCache = new CacheMock();
			CacheMock listCache = new CacheMock();

			PageService pageService = CreatePageService(pageCache, listCache, settingsRepository, pageRepository);
			PageViewModel expectedModel = CreatePageViewModel();
			AddPageCacheItem(pageCache, "key", expectedModel);
			AddListCacheItem(listCache, "key", new List<string>() { "tag1", "tag2" });

			// Act
			pageService.DeletePage(1);

			// Assert
			Assert.That(pageCache.CacheItems.Count, Is.EqualTo(0));
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(0));
		}

		[Test]
		public void findhomepage_should_load_from_cache()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			CacheMock modelCache = new CacheMock();

			PageService pageService = CreatePageService(modelCache, null, settingsRepository, pageRepository);
			PageViewModel expectedModel = CreatePageViewModel();
			expectedModel.RawTags = "homepage";
			modelCache.Add(CacheKeys.HomepageKey(), expectedModel, new CacheItemPolicy());

			// Act
			PageViewModel actualModel = pageService.FindHomePage();

			// Assert
			Assert.That(actualModel.Id, Is.EqualTo(expectedModel.Id));
			Assert.That(actualModel.VersionNumber, Is.EqualTo(expectedModel.VersionNumber));
			Assert.That(actualModel.Title, Is.EqualTo(expectedModel.Title));
		}

		[Test]
		public void findhomepage_should_add_to_cache_when_cache_is_empty()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			pageRepository.AddNewPage(new Page() { Title = "1", Tags = "homepage" }, "text", "admin", DateTime.UtcNow);

			CacheMock pageCache = new CacheMock();
			PageService pageService = CreatePageService(pageCache, null, settingsRepository, pageRepository);

			// Act
			pageService.FindHomePage();

			// Assert
			Assert.That(pageCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(pageCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(CacheKeys.HomepageKey()));
		}

		[Test]
		public void findbytag_should_load_from_cache()
		{
			string tag1CacheKey = CacheKeys.PagesByTagKey("tag1");
			string tag2CacheKey = CacheKeys.PagesByTagKey("tag2");

			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			CacheMock listCache = new CacheMock();

			PageService pageService = CreatePageService(null, listCache, settingsRepository, pageRepository);
			PageViewModel tag1Model = CreatePageViewModel();
			tag1Model.RawTags = "tag1";
			PageViewModel tag2Model = CreatePageViewModel();
			tag2Model.RawTags = "tag2";

			listCache.Add(tag1CacheKey, new List<PageViewModel>() { tag1Model }, new CacheItemPolicy());
			listCache.Add(tag2CacheKey, new List<PageViewModel>() { tag2Model }, new CacheItemPolicy());

			// Act
			IEnumerable<PageViewModel> actualList = pageService.FindByTag("tag1");

			// Assert
			Assert.That(actualList, Contains.Item(tag1Model));
			Assert.That(actualList, Is.Not.Contains(tag2Model));

		}

		[Test]
		public void findbytag_should_add_to_cache_when_cache_is_empty()
		{
			// Arrange
			string cacheKey = CacheKeys.PagesByTagKey("tag1");

			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			pageRepository.AddNewPage(new Page() { Title = "1", Tags = "tag1" }, "text", "admin", DateTime.UtcNow);
			pageRepository.AddNewPage(new Page() { Title = "2", Tags = "tag2" }, "text", "admin", DateTime.UtcNow);
			pageRepository.AddNewPage(new Page() { Title = "2", Tags = "tag3" }, "text", "admin", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageService pageService = CreatePageService(null, listCache, settingsRepository, pageRepository);

			// Act
			pageService.FindByTag("tag1");

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.FirstOrDefault().Key, Is.EqualTo(cacheKey));
		}

		[Test]
		public void updatepage_should_clear_list_cache_and_pagesummary_cache()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			pageRepository.AddNewPage(new Page() { Tags = "homepage" }, "text", "admin", DateTime.UtcNow);
			pageRepository.AddNewPage(new Page() { Tags = "tag2" }, "text", "admin", DateTime.UtcNow);

			CacheMock pageCache = new CacheMock();
			CacheMock listCache = new CacheMock();
			PageService pageService = CreatePageService(pageCache, listCache, settingsRepository, pageRepository);

			PageViewModel homepageModel = CreatePageViewModel();
			homepageModel.Id = 1;
			PageViewModel page2Model = CreatePageViewModel();
			page2Model.Id = 2;

			AddPageCacheItem(pageCache, CacheKeys.HomepageKey(), homepageModel);
			pageCache.Add(CacheKeys.PageViewModelKey(2, 0), page2Model, new CacheItemPolicy());
			AddListCacheItem(listCache, CacheKeys.AllTags(), new List<string>() { "tag1", "tag2" });

			// Act
			pageService.UpdatePage(page2Model);

			// Assert
			Assert.That(pageCache.CacheItems.Count, Is.EqualTo(1));
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(0));
		}

		[Test]
		public void updatepage_should_remove_homepage_from_cache_when_homepage_is_updated()
		{
			// Arrange
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			pageRepository.AddNewPage(new Page() { Tags = "homepage" }, "text", "admin", DateTime.UtcNow);

			CacheMock pageCache = new CacheMock();
			CacheMock listCache = new CacheMock();
			PageService pageService = CreatePageService(pageCache, listCache, settingsRepository, pageRepository);

			PageViewModel homepageModel = CreatePageViewModel();
			homepageModel.RawTags = "homepage";
			pageCache.Add(CacheKeys.HomepageKey(), homepageModel, new CacheItemPolicy());

			// Act
			pageService.UpdatePage(homepageModel);

			// Assert
			Assert.That(pageCache.CacheItems.Count, Is.EqualTo(0));
		}

		[Test]
		public void renametag_should_clear_listcache()
		{
			// Arrange
			string tag1CacheKey = CacheKeys.PagesByTagKey("tag1");
			SettingsRepositoryMock settingsRepository = new SettingsRepositoryMock();
			PageRepositoryMock pageRepository = new PageRepositoryMock();
			pageRepository.AddNewPage(new Page() { Tags = "homepage, tag1" }, "text1", "admin", DateTime.UtcNow);

			CacheMock listCache = new CacheMock();
			PageViewModel homepageModel = CreatePageViewModel();
			PageViewModel page1Model = CreatePageViewModel();
			AddListCacheItem(listCache, tag1CacheKey, new List<PageViewModel>() { homepageModel, page1Model });

			PageService pageService = CreatePageService(null, listCache, settingsRepository, pageRepository);

			// Act
			pageService.RenameTag("tag1", "some.other.tag"); // calls UpdatePage, which clears the cache

			// Assert
			Assert.That(listCache.CacheItems.Count, Is.EqualTo(0));
		}

		private PageViewModel CreatePageViewModel(string createdBy = "admin")
		{
			PageViewModel model = new PageViewModel();
			model.Title = "my title";
			model.Id = 1;
			model.CreatedBy = createdBy;
			model.VersionNumber = PageViewModelCache.LATEST_VERSION_NUMBER;

			return model;
		}

		private PageService CreatePageService(ObjectCache pageObjectCache, ObjectCache listObjectCache, SettingsRepositoryMock settingsRepository, PageRepositoryMock pageRepository)
		{
			// Stick to memorycache when each one isn't used
			if (pageObjectCache == null)
				pageObjectCache = CacheMock.RoadkillCache;

			if (listObjectCache == null)
				listObjectCache = CacheMock.RoadkillCache;

			// Settings
			ApplicationSettings appSettings = new ApplicationSettings() { Installed = true, UseObjectCache = true };
			UserContextStub userContext = new UserContextStub() { IsLoggedIn = false };

			// PageService
			PageViewModelCache pageViewModelCache = new PageViewModelCache(appSettings, pageObjectCache);
			ListCache listCache = new ListCache(appSettings, listObjectCache);
			SiteCache siteCache = new SiteCache(CacheMock.RoadkillCache);
			SearchServiceMock searchService = new SearchServiceMock(appSettings, settingsRepository, pageRepository, _pluginFactory);
			PageHistoryService historyService = new PageHistoryService(appSettings, settingsRepository, pageRepository, userContext, pageViewModelCache, _pluginFactory);
			PageService pageService = new PageService(appSettings, settingsRepository, pageRepository, searchService, historyService, userContext, listCache, pageViewModelCache, siteCache, _pluginFactory);

			return pageService;
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

		private void AddPageCacheItem(CacheMock cache, string key, object value)
		{
			cache.Add(CacheKeys.PAGEVIEWMODEL_CACHE_PREFIX + key, value, new CacheItemPolicy());
		}

		private void AddListCacheItem(CacheMock cache, string key, object value)
		{
			cache.Add(CacheKeys.ListCacheKey(key), value, new CacheItemPolicy());
		}
	}
}