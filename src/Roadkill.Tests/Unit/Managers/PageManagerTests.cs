using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Search;

namespace Roadkill.Tests.Unit
{
	/// <summary>
	/// Tests that the PageManager methods call the repository and return the data in a 
	/// correct state.
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class PageManagerTests
	{
		public static string AdminEmail = "admin@localhost";
		public static string AdminUsername = "admin";
		public static string AdminPassword = "password";

		private User _testUser;

		private RepositoryStub _repositoryStub;
		private Mock<SearchManager> _mockSearchManager;
		private Mock<UserManager> _mockUserManager;
		private IConfigurationContainer _config;
		private MarkupConverter _markupConverter;
		private HistoryManager _historyManager;
		private RoadkillContext _context;
		private PageManager _pageManager;

		[SetUp]
		public void SearchSetup()
		{
			// Repository stub
			_repositoryStub = new RepositoryStub();

			// Config stub
			_config = new ConfigurationContainer();
			_config.ApplicationSettings = new ApplicationSettings();
			_config.ApplicationSettings.Installed = true;
			_config.SitePreferences = new SiteSettings() { MarkupType = "Creole" };

			// Cache
			ListCache listCache = new ListCache(_config);
			PageSummaryCache pageSummaryCache = new PageSummaryCache(_config);

			// Managers needed by the PageManager
			_markupConverter = new MarkupConverter(_config, _repositoryStub);
			_mockSearchManager = new Mock<SearchManager>(_config, _repositoryStub);
			_historyManager = new HistoryManager(_config, _repositoryStub, _context, pageSummaryCache);

			// Usermanager stub
			_testUser = new User();
			_testUser.Id = Guid.NewGuid();
			_testUser.Email = AdminEmail;
			_testUser.Username = AdminUsername;
			Guid userId = _testUser.Id;

			_mockUserManager = new Mock<UserManager>(_config, _repositoryStub);
			_mockUserManager.Setup(x => x.GetUser(_testUser.Email, It.IsAny<bool>())).Returns(_testUser);//GetUserById
			_mockUserManager.Setup(x => x.GetUserById(userId, It.IsAny<bool>())).Returns(_testUser);
			_mockUserManager.Setup(x => x.Authenticate(_testUser.Email, "")).Returns(true);
			_mockUserManager.Setup(x => x.GetLoggedInUserName(It.IsAny<HttpContextBase>())).Returns(_testUser.Username);

			// Context stub
			_context = new RoadkillContext(_mockUserManager.Object);
			_context.CurrentUser = userId.ToString();

			// And finally the IoC setup
			IoCSetup iocSetup = new IoCSetup(_config, _repositoryStub, _context);
			iocSetup.Run();

			_pageManager = new PageManager(_config, _repositoryStub, _mockSearchManager.Object, _historyManager, _context, listCache, pageSummaryCache);
		}

		public PageSummary AddToStubbedRepository(int id, string createdBy, string title, string tags, string textContent = "")
		{
			return AddToStubbedRepository(id, createdBy, title, tags, DateTime.Today, textContent);
		}

		/// <summary>
		/// Adds a page to the mock repository (which is just a list of Page and PageContent objects in memory).
		/// </summary>
		public PageSummary AddToStubbedRepository(int id, string createdBy, string title, string tags, DateTime createdOn, string textContent = "")
		{
			Page page = new Page();
			page.Id = id;
			page.CreatedBy = createdBy;
			page.Title = title;
			page.Tags = tags;
			page.CreatedOn = createdOn;

			if (string.IsNullOrEmpty(textContent))
				textContent = title + "'s text";

			PageContent content = _repositoryStub.AddNewPage(page, textContent, createdBy, createdOn);
			PageSummary summary = new PageSummary()
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
			PageSummary summary = new PageSummary()
			{
				Id = 1,
				Title = "Homepage",
				Content = "**Homepage**",
				RawTags = "1;2;3;",
				CreatedBy = AdminUsername,
				CreatedOn = DateTime.Now
			};

			// Act
			PageSummary newSummary = _pageManager.AddPage(summary);

			// Assert
			Assert.That(newSummary, Is.Not.Null);
			Assert.That(newSummary.Content, Is.EqualTo(summary.Content));
			Assert.That(_repositoryStub.Pages.Count, Is.EqualTo(1));
			Assert.That(_repositoryStub.PageContents.Count, Is.EqualTo(1));
		}

		[Test]
		public void AllTags_Should_Return_Correct_Items()
		{
			// Arrange
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			List<TagSummary> summaries = _pageManager.AllTags().OrderBy(t => t.Name).ToList();

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
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			_pageManager.DeletePage(page1.Id);
			_pageManager.DeletePage(page2.Id);
			List<PageSummary> summaries = _pageManager.AllPages().ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(3), "Page count");
			Assert.That(summaries.FirstOrDefault(p => p.Title == "Homepage"), Is.Null);
			Assert.That(summaries.FirstOrDefault(p => p.Title == "page 2"), Is.Null);
		}

		[Test]
		public void AllPages_CreatedBy_Should_Have_Correct_Authors()
		{
			// Arrange
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToStubbedRepository(3, "bob", "page 3", "page3;");
			PageSummary page4 = AddToStubbedRepository(4, "bob", "page 4", "animals;");
			PageSummary page5 = AddToStubbedRepository(5, "bob", "page 5", "animals;");

			// Act
			List<PageSummary> summaries = _pageManager.AllPagesCreatedBy("bob").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(3), "Summary count");
			Assert.That(summaries.FirstOrDefault(p => p.CreatedBy == "admin"), Is.Null);
		}

		[Test]
		public void AllPages_Should_Have_Correct_Items()
		{
			// Arrange
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToStubbedRepository(3, "bob", "page 3", "page3;");
			PageSummary page4 = AddToStubbedRepository(4, "bob", "page 4", "animals;");
			PageSummary page5 = AddToStubbedRepository(5, "bob", "page 5", "animals;");

			// Act
			List<PageSummary> summaries = _pageManager.AllPages().ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(5), "Summary count");
		}

		[Test]
		public void FindByTags_For_Single_Tag_Returns_Single_Result()
		{
			// Arrange
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");

			// Act
			List<PageSummary> summaries = _pageManager.FindByTag("homepage").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(1), "Summary count");
			Assert.That(summaries[0].Title, Is.EqualTo("Homepage"), "Summary title");
			Assert.That(summaries[0].Tags.ToList()[0], Is.EqualTo("homepage"), "Summary tags");
		}

		[Test]
		public void FindByTags_For_Multiple_Tags_Returns_Many_Results()
		{
			// Arrange
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			List<PageSummary> summaries = _pageManager.FindByTag("animals").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(2), "Summary count");
		}

		[Test]
		public void FindByTitle_Should_Return_Correct_Page()
		{
			// Arrange
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			PageSummary summary = _pageManager.FindByTitle("page 3");

			// Assert
			Assert.That(summary.Title, Is.EqualTo("page 3"), "Page title");
		}

		[Test]
		public void GetById_Should_Return_Correct_Page()
		{
			// Arrange
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToStubbedRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToStubbedRepository(5, "admin", "page 5", "animals;");

			// Act
			PageSummary summary = _pageManager.GetById(page3.Id);

			// Assert
			Assert.That(summary.Id, Is.EqualTo(page3.Id), "Page id");
			Assert.That(summary.Title, Is.EqualTo("page 3"), "Page title");
		}

		[Test]
		public void ExportToXml_Should_Contain_Xml()
		{
			// Arrange
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");

			// Act
			string xml = _pageManager.ExportToXml();

			// Assert
			Assert.That(xml, Is.StringContaining("<?xml"));
			Assert.That(xml, Is.StringContaining("<ArrayOfPageSummary"));
			Assert.That(xml, Is.StringContaining("<Id>1</Id>"));
			Assert.That(xml, Is.StringContaining("<Id>2</Id>"));
		}

		[Test]
		public void RenameTags_For_Multiple_Tags_Returns_Multiple_Results()
		{
			// Arrange
			PageSummary page1 = AddToStubbedRepository(1, "admin", "Homepage", "animal;");
			PageSummary page2 = AddToStubbedRepository(2, "admin", "page 2", "animal;");

			// Act
			_pageManager.RenameTag("animal", "vegetable");
			List<PageSummary> animalTagList = _pageManager.FindByTag("animal").ToList();
			List<PageSummary> vegetableTagList = _pageManager.FindByTag("vegetable").ToList();

			// Assert
			Assert.That(animalTagList.Count, Is.EqualTo(0), "Old tag summary count");
			Assert.That(vegetableTagList.Count, Is.EqualTo(2), "New tag summary count");
		}

		[Test]
		public void UpdatePage_Should_Persist_To_Repository()
		{
			// Arrange
			PageSummary summary = AddToStubbedRepository(1, "admin", "Homepage", "animal;");

			// Act
			summary.RawTags = "new,tags,";
			summary.Title = "New title";
			summary.Content = "**New content**";

			_pageManager.UpdatePage(summary);
			PageSummary actual = _pageManager.GetById(1);

			// Assert
			Assert.That(actual.Title, Is.EqualTo(summary.Title), "Title");
			Assert.That(actual.Tags, Is.EqualTo(summary.Tags), "Tags");

			Assert.That(_repositoryStub.Pages[0].Tags, Is.EqualTo(summary.RawTags));
			Assert.That(_repositoryStub.Pages[0].Title, Is.EqualTo(summary.Title));
			Assert.That(_repositoryStub.PageContents[1].Text, Is.EqualTo(summary.Content)); // smells
		}
	}
}
