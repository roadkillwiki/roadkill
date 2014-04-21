using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Caching;

namespace Roadkill.Tests.Unit
{
	public class MocksAndStubsContainer
	{
		public ApplicationSettings ApplicationSettings { get; set; }
		public MemoryCache MemoryCache { get; set; }
		public ListCache ListCache { get; set; }
		public SiteCache SiteCache { get; set; }
		public PageViewModelCache PageViewModelCache { get; set; }
		public IUserContext UserContext { get; set; }
		public RepositoryMock Repository { get; set; }
		public UserServiceMock UserService { get; set; }
		public PageService PageService { get; set; }
		public SearchServiceMock SearchService { get; set; }
		public PageHistoryService HistoryService { get; set; }
		public SettingsService SettingsService { get; set; }
		public PluginFactoryMock PluginFactory { get; set; }
		public MarkupConverter MarkupConverter { get; set; }
		public EmailClientMock EmailClient { get; set; }
		public IFileService FileService { get; set; }

		/// <summary>
		/// Creates a new instance of MocksAndStubsContainer.
		/// </summary>
		/// <param name="useCacheMock">The 'Roadkill' MemoryCache is used by default, but as this is static it can have problems with 
		/// the test runner unless you clear the Container.MemoryCache on setup each time, but then doing that doesn't give a realistic 
		/// reflection of how the MemoryCache is used inside an ASP.NET environment.</param>
		public MocksAndStubsContainer(bool useCacheMock = false)
		{
			ApplicationSettings = new ApplicationSettings();
			ApplicationSettings.Installed = true;
			ApplicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");

			// Cache
			MemoryCache = useCacheMock ? new CacheMock() : CacheMock.RoadkillCache;
			ListCache = new ListCache(ApplicationSettings, MemoryCache);
			SiteCache = new SiteCache(ApplicationSettings, MemoryCache);
			PageViewModelCache = new PageViewModelCache(ApplicationSettings, MemoryCache);

			// Repository
			Repository = new RepositoryMock();
			Repository.SiteSettings = new SiteSettings();
			Repository.SiteSettings.MarkupType = "Creole";

			PluginFactory = new PluginFactoryMock();
			MarkupConverter = new MarkupConverter(ApplicationSettings, Repository, PluginFactory);

			// Dependencies for PageService. Be careful to make sure the class using this Container isn't testing the mock.
			SettingsService = new SettingsService(ApplicationSettings, Repository);
			UserService = new UserServiceMock(ApplicationSettings, Repository);
			UserContext = new UserContext(UserService);
			SearchService = new SearchServiceMock(ApplicationSettings, Repository, PluginFactory);
			SearchService.PageContents = Repository.PageContents;
			SearchService.Pages = Repository.Pages;
			HistoryService = new PageHistoryService(ApplicationSettings, Repository, UserContext, PageViewModelCache, PluginFactory);

			PageService = new PageService(ApplicationSettings, Repository, SearchService, HistoryService, UserContext, ListCache, PageViewModelCache, SiteCache, PluginFactory);

			// EmailTemplates
			EmailClient = new EmailClientMock();

			// Other services
			FileService = new FileServiceMock();
		}

		public void ClearCache()
		{
			foreach (string key in MemoryCache.Select(x => x.Key))
				MemoryCache.Remove(key);
		}
	}
}
