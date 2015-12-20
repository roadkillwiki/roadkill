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
		public ConfigReaderWriterStub ConfigReaderWriter { get; set; }
		public IUserContext UserContext { get; set; }

		public MemoryCache MemoryCache { get; set; }
		public ListCache ListCache { get; set; }
		public SiteCache SiteCache { get; set; }
		public PageViewModelCache PageViewModelCache { get; set; }
		
		public PluginFactoryMock PluginFactory { get; set; }
		public MarkupConverter MarkupConverter { get; set; }
		public EmailClientMock EmailClient { get; set; }

		public UserServiceMock UserService { get; set; }
		public PageService PageService { get; set; }
		public SearchServiceMock SearchService { get; set; }
		public PageHistoryService HistoryService { get; set; }
		public SettingsService SettingsService { get; set; }
		public IFileService FileService { get; set; }

		public RepositoryFactoryMock RepositoryFactory { get; set; }
		public SettingsRepositoryMock SettingsRepository { get; set; }
		public UserRepositoryMock UserRepository { get; set; }
		public PageRepositoryMock PageRepository { get; set; }
		public InstallerRepositoryMock InstallerRepository { get; set; }

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
			ConfigReaderWriter = new ConfigReaderWriterStub();

			// Cache
			MemoryCache = useCacheMock ? new CacheMock() : CacheMock.RoadkillCache;
			ListCache = new ListCache(ApplicationSettings, MemoryCache);
			SiteCache = new SiteCache(MemoryCache);
			PageViewModelCache = new PageViewModelCache(ApplicationSettings, MemoryCache);

			// pageRepository
			SettingsRepository = new SettingsRepositoryMock();
			SettingsRepository.SiteSettings = new SiteSettings();
			SettingsRepository.SiteSettings.MarkupType = "Creole";
			UserRepository = new UserRepositoryMock();
			PageRepository = new PageRepositoryMock();
			InstallerRepository = new InstallerRepositoryMock();

			RepositoryFactory = new RepositoryFactoryMock()
			{
				SettingsRepository = SettingsRepository,
				UserRepository = UserRepository,
				PageRepository = PageRepository,
				InstallerRepository = InstallerRepository
			};

			PluginFactory = new PluginFactoryMock();
			MarkupConverter = new MarkupConverter(ApplicationSettings, SettingsRepository, PageRepository, PluginFactory);

			// Dependencies for PageService. Be careful to make sure the class using this Container isn't testing the mock.
			SettingsService = new SettingsService(RepositoryFactory, ApplicationSettings);
			UserService = new UserServiceMock(ApplicationSettings, UserRepository);
			UserContext = new UserContext(UserService);
			SearchService = new SearchServiceMock(ApplicationSettings, SettingsRepository, PageRepository, PluginFactory);
			SearchService.PageContents = PageRepository.PageContents;
			SearchService.Pages = PageRepository.Pages;
			HistoryService = new PageHistoryService(ApplicationSettings, SettingsRepository, PageRepository, UserContext, PageViewModelCache, PluginFactory);

			PageService = new PageService(ApplicationSettings, SettingsRepository, PageRepository, SearchService, HistoryService, UserContext, ListCache, PageViewModelCache, SiteCache, PluginFactory);

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
