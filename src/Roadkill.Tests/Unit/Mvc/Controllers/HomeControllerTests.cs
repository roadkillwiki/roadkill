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
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class HomeControllerTests
	{
		private ApplicationSettings _settings;
		private IUserContext _context;
		private RepositoryMock _repository;

		private UserManagerBase _userManager;
		private PageManager _pageManager;
		private SearchManagerMock _searchManager;
		private HistoryManager _historyManager;
		private SettingsManager _settingsManager;

		[SetUp]
		public void Init()
		{
			_context = new Mock<IUserContext>().Object;
			_settings = new ApplicationSettings();
			_settings.Installed = true;
			_settings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");

			// Cache
			ListCache listCache = new ListCache(_settings);
			PageSummaryCache pageSummaryCache = new PageSummaryCache(_settings);

			// Dependencies for PageManager
			Mock<SearchManager> searchMock = new Mock<SearchManager>();

			_repository = new RepositoryMock();
			_settingsManager = new SettingsManager(_settings, _repository);
			_userManager = new Mock<UserManagerBase>(_settings, null).Object;
			_searchManager = new SearchManagerMock(_settings, _repository);
			_searchManager.PageContents = _repository.PageContents;
			_searchManager.Pages = _repository.Pages;
			_historyManager = new HistoryManager(_settings, _repository, _context, pageSummaryCache);
			_pageManager = new PageManager(_settings, _repository, _searchManager, _historyManager, _context, listCache, pageSummaryCache);
		}

		[Test]
		public void Index_Should_Return_Default_Message_When_No_Homepage_Tag_Exists()
		{
			// Arrange
			HomeController homeController = new HomeController(_settings, _userManager, new MarkupConverter(_settings, _repository), _pageManager, _searchManager, _context, _settingsManager);
			homeController.SetFakeControllerContext();

			// Act
			ActionResult result = homeController.Index();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			PageSummary summary = result.ModelFromActionResult<PageSummary>();
			Assert.NotNull(summary, "Null model");
			Assert.That(summary.Title, Is.EqualTo(SiteStrings.NoMainPage_Title));
			Assert.That(summary.Content, Is.EqualTo(SiteStrings.NoMainPage_Label));
		}

		[Test]
		public void Index_Should_Return_Homepage_When_Tag_Exists()
		{
			// Arrange
			HomeController homeController = new HomeController(_settings, _userManager, new MarkupConverter(_settings, _repository), _pageManager, _searchManager, _context, _settingsManager);
			homeController.SetFakeControllerContext();
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
			ActionResult result = homeController.Index();
			
			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			PageSummary summary = result.ModelFromActionResult<PageSummary>();
			Assert.NotNull(summary, "Null model");
			Assert.That(summary.Title, Is.EqualTo(page1.Title));
			Assert.That(summary.Content, Is.EqualTo(page1Content.Text));
		}

		[Test]
		public void Search_Should_Return_Some_Results_With_Unicode_Content()
		{
			// Arrange
			HomeController homeController = new HomeController(_settings, _userManager, new MarkupConverter(_settings, _repository), _pageManager, _searchManager, _context, _settingsManager);
			homeController.SetFakeControllerContext();
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
			ActionResult result = homeController.Search("ОШИБКА: неверная последовательность байт для кодировки");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			List<SearchResult> searchResults = result.ModelFromActionResult<IEnumerable<SearchResult>>().ToList();
			Assert.NotNull(searchResults, "Null model");
			Assert.That(searchResults.Count(), Is.EqualTo(1));
			Assert.That(searchResults[0].Title, Is.EqualTo(page1.Title));
			Assert.That(searchResults[0].ContentSummary, Contains.Substring(page1Content.Text));
		}
	}
}
