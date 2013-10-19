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

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class InstallControllerTests
	{
		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;

		private UserServiceBase _userManager;
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
			ListCache listCache = new ListCache(_applicationSettings, MemoryCache.Default);
			SiteCache siteCache = new SiteCache(_applicationSettings, MemoryCache.Default);
			PageSummaryCache pageSummaryCache = new PageSummaryCache(_applicationSettings, MemoryCache.Default);

			// Dependencies for PageService
			Mock<SearchService> searchMock = new Mock<SearchService>();
			_pluginFactory = new PluginFactoryMock();

			_repository = new RepositoryMock();
			_settingsService = new SettingsService(_applicationSettings, _repository);
			_userManager = new Mock<UserServiceBase>(_applicationSettings, null).Object;
			_searchService = new SearchServiceMock(_applicationSettings, _repository, _pluginFactory);
			_searchService.PageContents = _repository.PageContents;
			_searchService.Pages = _repository.Pages;
			_historyService = new PageHistoryService(_applicationSettings, _repository, _context, pageSummaryCache, _pluginFactory);
			_pageService = new PageService(_applicationSettings, _repository, _searchService, _historyService, _context, listCache, pageSummaryCache, siteCache, _pluginFactory);
		}

		[Test]
		public void Index__Should_Return_ViewResult_And_Model_With_LanguageSummaries_And_Set_UILanguage_To_English()
		{
			// Arrange
			InstallController controller = new InstallController(_applicationSettings, _userManager, _pageService,
													_searchService, _repository, _settingsService, _context);

			// Act
			ViewResult result = controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			IEnumerable<LanguageSummary> summaries = result.ModelFromActionResult<IEnumerable<LanguageSummary>>();
			Assert.NotNull(summaries, "Null model");
			Assert.That(summaries.Count(), Is.GreaterThanOrEqualTo(1));
			Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo("en"));
		}

		[Test]
		public void Step1_Should_Return_ViewResult_With_Language_Summary_And_Set_UICulture_From_Language()
		{
			// Arrange
			InstallController controller = new InstallController(_applicationSettings, _userManager, _pageService,
													_searchService, _repository, _settingsService, _context);

			// Act
			ViewResult result = controller.Step1("hi") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			LanguageSummary summary = result.ModelFromActionResult<LanguageSummary>();
			Assert.NotNull(summary, "Null model");
			Assert.That(summary.Code, Is.EqualTo("hi"));

			Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo("hi"));
		}
	}
}