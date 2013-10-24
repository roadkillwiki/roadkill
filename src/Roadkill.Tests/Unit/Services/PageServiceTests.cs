using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Tests.Unit
{
	/// <summary>
	/// Tests that the PageService methods correctly call the repository and return the data in a correct state.
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class PageServiceTests
	{
		public static string AdminEmail = "admin@localhost";
		public static string AdminUsername = "admin";
		public static string AdminPassword = "password";

		private User _testUser;

		private RepositoryMock _repositoryMock;
		private Mock<SearchService> _mockSearchService;
		private Mock<UserServiceBase> _mockUserManager;
		private ApplicationSettings _applicationSettings;
		private MarkupConverter _markupConverter;
		private PageHistoryService _historyService;
		private UserContext _context;
		private PageService _pageService;
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Setup()
		{
			// Repository stub
			_repositoryMock = new RepositoryMock();

			// Config stub
			_applicationSettings = new ApplicationSettings();
			_applicationSettings.ConnectionString = "connstring";
			_applicationSettings.Installed = true;

			_repositoryMock = new RepositoryMock();
			_repositoryMock.SiteSettings = new SiteSettings();
			_repositoryMock.SiteSettings.MarkupType = "Creole";

			// Cache
			ListCache listCache = new ListCache(_applicationSettings, MemoryCache.Default);
			PageViewModelCache pageViewModelCache = new PageViewModelCache(_applicationSettings, MemoryCache.Default);
			SiteCache siteCache = new SiteCache(_applicationSettings, MemoryCache.Default);

			// Services needed by the PageService
			_pluginFactory = new PluginFactoryMock();
			_markupConverter = new MarkupConverter(_applicationSettings, _repositoryMock, _pluginFactory);
			_mockSearchService = new Mock<SearchService>(_applicationSettings, _repositoryMock, _pluginFactory);
			_historyService = new PageHistoryService(_applicationSettings, _repositoryMock, _context, pageViewModelCache, _pluginFactory);

			// Usermanager stub
			_testUser = new User();
			_testUser.Id = Guid.NewGuid();
			_testUser.Email = AdminEmail;
			_testUser.Username = AdminUsername;
			Guid userId = _testUser.Id;

			_mockUserManager = new Mock<UserServiceBase>(_applicationSettings, _repositoryMock);
			_mockUserManager.Setup(x => x.GetUser(_testUser.Email, It.IsAny<bool>())).Returns(_testUser);
			_mockUserManager.Setup(x => x.GetUserById(userId, It.IsAny<bool>())).Returns(_testUser);
			_mockUserManager.Setup(x => x.Authenticate(_testUser.Email, "")).Returns(true);
			_mockUserManager.Setup(x => x.GetLoggedInUserName(It.IsAny<HttpContextBase>())).Returns(_testUser.Username);

			// Context stub
			_context = new UserContext(_mockUserManager.Object);
			_context.CurrentUser = userId.ToString();

			_pageService = new PageService(_applicationSettings, _repositoryMock, _mockSearchService.Object, _historyService, _context, listCache, pageViewModelCache, siteCache, _pluginFactory);
		}

		public PageViewModel AddToStubbedRepository(int id, string createdBy, string title, string tags, string textContent = "")
		{
			return AddToMockedRepository(id, createdBy, title, tags, DateTime.Today, textContent);
		}

		/// <summary>
		/// Adds a page to the mock repository (which is just a list of Page and PageContent objects in memory).
		/// </summary>
		public PageViewModel AddToMockedRepository(int id, string createdBy, string title, string tags, DateTime createdOn, string textContent = "")
		{
			Page page = new Page();
			page.Id = id;
			page.CreatedBy = createdBy;
			page.Title = title;
			page.Tags = tags;
			page.CreatedOn = createdOn;

			if (string.IsNullOrEmpty(textContent))
				textContent = title + "'s text";

			PageContent content = _repositoryMock.AddNewPage(page, textContent, createdBy, createdOn);
			PageViewModel summary = new PageViewModel()
			{
				Id = id,
				Title = title,
				Content = textContent,
				RawTags = tags,
				CreatedBy = createdBy,
				CreatedOn = createdOn
			};

			return summary;
		}

		[Test]
		public void AddPage_Should_Save_To_Repository()
		{
			// Arrange
			PageViewModel summary = new PageViewModel()
			{
				Id = 1,
				Title = "Homepage",
				Content = "**Homepage**",
				RawTags = "1;2;3;",
				CreatedBy = AdminUsername,
				CreatedOn = DateTime.UtcNow
			};

			// Act
			PageViewModel newSummary = _pageService.AddPage(summary);

			// Assert
			Assert.That(newSummary, Is.Not.Null);
			Assert.That(newSummary.Content, Is.EqualTo(summary.Content));
			Assert.That(_repositoryMock.Pages.Count, Is.EqualTo(1));
			Assert.That(_repositoryMock.PageContents.Count, Is.EqualTo(1));
		}

		[Test]
		public void AllTags_Should_Return_Correct_Items()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageViewModel page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageViewModel page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageViewModel page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			List<TagViewModel> summaries = _pageService.AllTags().OrderBy(t => t.Name).ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(4), "Tag summary count");
			Assert.That(summaries[0].Name, Is.EqualTo("animals"));
			Assert.That(summaries[1].Name, Is.EqualTo("homepage"));
			Assert.That(summaries[2].Name, Is.EqualTo("page2"));
			Assert.That(summaries[3].Name, Is.EqualTo("page3"));
		}

		[Test]
		public void DeletePage_Should_Remove_Correct_Page()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageViewModel page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageViewModel page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageViewModel page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			_pageService.DeletePage(page1.Id);
			_pageService.DeletePage(page2.Id);
			List<PageViewModel> summaries = _pageService.AllPages().ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(3), "Page count");
			Assert.That(summaries.FirstOrDefault(p => p.Title == "Homepage"), Is.Null);
			Assert.That(summaries.FirstOrDefault(p => p.Title == "page 2"), Is.Null);
		}

		[Test]
		public void AllPages_CreatedBy_Should_Have_Correct_Authors()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageViewModel page3 = AddToStubbedRepository(3, "bob", "page 3", "page3;");
			PageViewModel page4 = AddToStubbedRepository(4, "bob", "page 4", "animals;");
			PageViewModel page5 = AddToStubbedRepository(5, "bob", "page 5", "animals;");

			// Act
			List<PageViewModel> summaries = _pageService.AllPagesCreatedBy("bob").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(3), "Summary count");
			Assert.That(summaries.FirstOrDefault(p => p.CreatedBy == "admin"), Is.Null);
		}

		[Test]
		public void AllPages_Should_Have_Correct_Items()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageViewModel page3 = AddToStubbedRepository(3, "bob", "page 3", "page3;");
			PageViewModel page4 = AddToStubbedRepository(4, "bob", "page 4", "animals;");
			PageViewModel page5 = AddToStubbedRepository(5, "bob", "page 5", "animals;");

			// Act
			List<PageViewModel> summaries = _pageService.AllPages().ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(5), "Summary count");
		}

		[Test]
		public void FindByTags_For_Single_Tag_Returns_Single_Result()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageViewModel page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");

			// Act
			List<PageViewModel> summaries = _pageService.FindByTag("homepage").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(1), "Summary count");
			Assert.That(summaries[0].Title, Is.EqualTo("Homepage"), "Summary title");
			Assert.That(summaries[0].Tags.ToList()[0], Is.EqualTo("homepage"), "Summary tags");
		}

		[Test]
		public void FindByTags_For_Multiple_Tags_Returns_Many_Results()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageViewModel page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageViewModel page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageViewModel page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			List<PageViewModel> summaries = _pageService.FindByTag("animals").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(2), "Summary count");
		}

		[Test]
		public void FindByTitle_Should_Return_Correct_Page()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageViewModel page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageViewModel page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageViewModel page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			PageViewModel summary = _pageService.FindByTitle("page 3");

			// Assert
			Assert.That(summary.Title, Is.EqualTo("page 3"), "Page title");
		}

		[Test]
		public void GetById_Should_Return_Correct_Page()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageViewModel page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageViewModel page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageViewModel page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			PageViewModel summary = _pageService.GetById(page3.Id);

			// Assert
			Assert.That(summary.Id, Is.EqualTo(page3.Id), "Page id");
			Assert.That(summary.Title, Is.EqualTo("page 3"), "Page title");
		}

		[Test]
		public void ExportToXml_Should_Contain_Xml()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");

			// Act
			string xml = _pageService.ExportToXml();

			// Assert
			Assert.That(xml, Is.StringContaining("<?xml"));
			Assert.That(xml, Is.StringContaining("<ArrayOfPageViewModel"));
			Assert.That(xml, Is.StringContaining("<Id>1</Id>"));
			Assert.That(xml, Is.StringContaining("<Id>2</Id>"));
		}

		[Test]
		public void RenameTags_For_Multiple_Tags_Returns_Multiple_Results()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "animal;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "animal;");

			// Act
			_pageService.RenameTag("animal", "vegetable");
			List<PageViewModel> animalTagList = _pageService.FindByTag("animal").ToList();
			List<PageViewModel> vegetableTagList = _pageService.FindByTag("vegetable").ToList();

			// Assert
			Assert.That(animalTagList.Count, Is.EqualTo(0), "Old tag summary count");
			Assert.That(vegetableTagList.Count, Is.EqualTo(2), "New tag summary count");
		}

		[Test]
		public void UpdatePage_Should_Persist_To_Repository()
		{
			// Arrange
			PageViewModel summary = AddToStubbedRepository(1, "admin", "Homepage", "animal;");
			string expectedTags = "new,tags";

			// Act
			summary.RawTags = "new,tags,";
			summary.Title = "New title";
			summary.Content = "**New content**";

			_pageService.UpdatePage(summary);
			PageViewModel actual = _pageService.GetById(1);

			// Assert
			Assert.That(actual.Title, Is.EqualTo(summary.Title), "Title");
			Assert.That(actual.Tags, Is.EqualTo(summary.Tags), "Tags");

			Assert.That(_repositoryMock.Pages[0].Tags, Is.EqualTo(expectedTags));
			Assert.That(_repositoryMock.Pages[0].Title, Is.EqualTo(summary.Title));
			Assert.That(_repositoryMock.PageContents[1].Text, Is.EqualTo(summary.Content)); // "smells"
		}
	}
}
