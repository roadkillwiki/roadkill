using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Domain;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Search;
using Roadkill.Tests.Integration;
using StructureMap;

namespace Roadkill.Tests.Unit.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class HomeControllerTests
	{
		private IConfigurationContainer _config;
		private IRoadkillContext _context;
		private IRepository _repository;

		private UserManager _userManager;
		private PageManager _pageManager;
		private FakeSearchManager _searchManager;
		private HistoryManager _historyManager;
		private List<Page> _pages;
		private List<PageContent> _pagesContent;

		[SetUp]
		public void Init()
		{
			_pages = new List<Page>();
			_pagesContent = new List<PageContent>();

			_context = new Mock<IRoadkillContext>().Object;
			_config = new RoadkillSettings();
			_config.ApplicationSettings = new ApplicationSettings();
			_config.SitePreferences = new SitePreferences() { AllowedFileTypes = "png, jpg" };
			_config.ApplicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");
			
			// Dependencies for homecontroller
			Mock<IRepository> repositoryMock = new Mock<IRepository>();
			repositoryMock.Setup(x => x.Pages).Returns(_pages.AsQueryable());
			repositoryMock.Setup(x => x.PageContents).Returns(_pagesContent.AsQueryable());
			repositoryMock.Setup(x => x.GetLatestPageContent(It.IsAny<int>())).Returns<int>((id) => _pagesContent.FirstOrDefault(p => p.Page.Id == id));

			Mock<SearchManager> searchMock = new Mock<SearchManager>();

			_repository = repositoryMock.Object;
			_userManager = new Mock<UserManager>(_config, null).Object;
			_searchManager = new FakeSearchManager(_config, _repository);
			_searchManager.PageContents = _pagesContent;
			_searchManager.Pages = _pages;
			_historyManager = new HistoryManager(_config, _repository, _context);
			_pageManager = new PageManager(_config, _repository, _searchManager, _historyManager, _context);
		}

		[Test]
		public void Index_Should_Return_Default_Message_When_No_Homepage_Tag_Exists()
		{
			// Arrange
			HomeController homeController = new HomeController(_config, _userManager, new MarkupConverter(_config, _repository), _pageManager, _searchManager, _context);
			MvcMockContainer mocksContainer = homeController.SetFakeControllerContext();

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
			HomeController homeController = new HomeController(_config, _userManager, new MarkupConverter(_config, _repository), _pageManager, _searchManager, _context);
			MvcMockContainer mocksContainer = homeController.SetFakeControllerContext();
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
			_pages.Add(page1);
			_pagesContent.Add(page1Content);

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
			HomeController homeController = new HomeController(_config, _userManager, new MarkupConverter(_config, _repository), _pageManager, _searchManager, _context);
			MvcMockContainer mocksContainer = homeController.SetFakeControllerContext();
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
			_pages.Add(page1);
			_pagesContent.Add(page1Content);

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

		// This is simpler than mocking out Lucene.net, or putting the home=>search tests inside the integration tests.
		private class FakeSearchManager : SearchManager
		{
			public List<Page> Pages { get; set; }
			public List<PageContent> PageContents { get; set; }

			public FakeSearchManager(IConfigurationContainer configuration, IRepository repository) : base(configuration, repository)
			{
				Pages = new List<Page>();
				PageContents = new List<PageContent>();
			}

			public override IEnumerable<SearchResult> SearchIndex(string searchText)
			{
				List<SearchResult> results = new List<SearchResult>();

				foreach (Page page in Pages.Where(p => p.Title.ToLowerInvariant().Contains(searchText.ToLowerInvariant())))
				{
					results.Add(new SearchResult() 
					{ 
						Id = page.Id, 
						Title = page.Title, 
						ContentSummary =  PageContents.Single(p => p.Page == page).Text
					});
				}

				foreach (PageContent pageContent in PageContents.Where(p => p.Text.ToLowerInvariant().Contains(searchText.ToLowerInvariant())))
				{
					results.Add(new SearchResult() 
					{ 
						Id = pageContent.Page.Id, 
						ContentSummary = pageContent.Text, 
						Title = pageContent.Page.Title 
					});
				}

				return results;
			}
		}
	}
}
