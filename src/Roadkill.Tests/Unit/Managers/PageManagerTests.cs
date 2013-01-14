using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Moq;
using NHibernate;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
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
		private List<PageContent> _contentList;
		private List<Page> _pageList;

		private Mock<IRepository> _mockRepository;
		private Mock<SearchManager> _mockSearchManager;
		private Mock<UserManager> _mockUserManager;
		private IConfigurationContainer _config;
		private MarkupConverter _markupConverter;
		private HistoryManager _historyManager;
		private RoadkillContext _context;

		[SetUp]
		public void SearchSetup()
		{
			// All pages and content
			_contentList = new List<PageContent>();
			_pageList = new List<Page>();

			// Repository stub
			_mockRepository = new Mock<IRepository>();
			_mockRepository.Setup(x => x.Pages).Returns(_pageList.AsQueryable());
			_mockRepository.Setup(x => x.PageContents).Returns(_contentList.AsQueryable());

			// Config stub
			_config = new RoadkillSettings();
			_config.ApplicationSettings = new ApplicationSettings();
			_config.SitePreferences = new SitePreferences() { MarkupType = "Creole" };

			// Managers needed by  the PageManager
			_markupConverter = new MarkupConverter(_config, _mockRepository.Object);
			_mockSearchManager = new Mock<SearchManager>(_config, _mockRepository.Object);
			_historyManager = new HistoryManager(_config, _mockRepository.Object, _context);

			// Usermanager stub
			Mock<User> mockAdminUser = new Mock<User>();
			mockAdminUser.SetupProperty<Guid>(x => x.Id, Guid.NewGuid());
			mockAdminUser.SetupProperty<string>(x => x.Email, AdminEmail);
			mockAdminUser.SetupProperty<string>(x => x.Username, AdminUsername);
			_testUser = mockAdminUser.Object;
			Guid userId = mockAdminUser.Object.Id;

			_mockUserManager = new Mock<UserManager>(_config, _mockRepository.Object);
			_mockUserManager.Setup(x => x.GetUser(_testUser.Email)).Returns(mockAdminUser.Object);//GetUserById
			_mockUserManager.Setup(x => x.GetUserById(userId)).Returns(mockAdminUser.Object);
			_mockUserManager.Setup(x => x.Authenticate(_testUser.Email, "")).Returns(true);
			_mockUserManager.Setup(x => x.GetLoggedInUserName(It.IsAny<HttpContextBase>())).Returns(_testUser.Username);

			// Context stub
			_context = new RoadkillContext(_mockUserManager.Object);
			_context.CurrentUser = userId.ToString();

			// And finally the IoC objects
			RoadkillApplication.SetupIoC(_config, _mockRepository.Object, _context);
		}

		public PageSummary AddToMockRepository(int id, string createdBy, string title, string tags, string textContent = "")
		{
			return AddToMockRepository(id, createdBy, title, tags, DateTime.Today, textContent);
		}

		/// <summary>
		/// Adds a page to the mock repository (which is just a list of Page and PageContent objects in memory).
		/// </summary>
		public PageSummary AddToMockRepository(int id, string createdBy, string title, string tags, DateTime createdOn, string textContent = "")
		{
			var pageMock = new Mock<Page>() { CallBase = true };
			pageMock.SetupProperty(x => x.Id, id);
			pageMock.SetupProperty(x => x.CreatedBy, createdBy);
			pageMock.SetupProperty(x => x.Title, title);
			pageMock.SetupProperty(x => x.Tags, tags);
			pageMock.SetupProperty(x => x.CreatedOn, createdOn);

			PageContent pageContent = new PageContent();
			pageContent.Page = pageMock.Object;
			pageContent.Id = Guid.NewGuid();
			pageContent.VersionNumber = 1;
			pageContent.EditedBy = createdBy;
			pageContent.EditedOn = DateTime.Now;

			if (string.IsNullOrEmpty(textContent))
				pageContent.Text = title + "'s text";
			else
				pageContent.Text = textContent;

			_mockRepository.Setup(x => x.Delete(pageMock.Object)).Callback(() => _pageList.Remove(pageMock.Object));
			_mockRepository.Setup(x => x.Delete(pageContent)).Callback(() => _contentList.Remove(pageContent));
			_mockRepository.Setup(x => x.SaveOrUpdate<Page>(It.Is<Page>(p => p.Id == id))).Callback<Page>(p => 
				{
					Page page = _pageList.Single(item => item.Id == p.Id);
					page.Title = p.Title;
					page.Tags = p.Tags;
					page.CreatedBy = p.CreatedBy;
					page.ModifiedBy = p.ModifiedBy;
					page.ModifiedOn = p.ModifiedOn;
				}
			);
			_mockRepository.Setup(x => x.SaveOrUpdate<PageContent>(It.Is<PageContent>(p => p.Page.Id == id))).Callback<PageContent>(p =>
				{
					PageContent page = _contentList.Single(item => item.Page.Id == p.Page.Id);
					page.Text = p.Text;
					page.EditedBy = p.EditedBy;
					page.EditedOn = p.EditedOn;
				}
			);

			_mockRepository.Setup(r => r.FindPageByTitle(title)).Returns(pageMock.Object);
			_mockRepository.Setup(x => x.SaveOrUpdate<PageContent>(pageContent));
			_mockRepository.Setup(x => x.GetLatestPageContent(id)).Returns(pageContent);

			_contentList.Add(pageContent);
			_pageList.Add(pageMock.Object);

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

		// -------- End stub setups

		[Test]
		public void AddPage_Should_Save_To_Repository()
		{
			// Arrange
			// - Track the repository save events
			PageSummary summary = AddToMockRepository(1,AdminUsername, "Homepage", "1;2;3;",DateTime.Now, "**Homepage**");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			PageSummary newSummary = manager.AddPage(summary);

			// Assert
			Assert.That(newSummary, Is.Not.Null);
			Assert.That(newSummary.Content, Is.EqualTo(summary.Content));
			_mockRepository.Verify
			(
				x => x.SaveOrUpdate<Page>(It.Is<Page>(p => p.Title == summary.Title && p.CreatedBy == AdminUsername && p.Tags == summary.CommaDelimitedTags()))
			);

			_mockRepository.Verify(x => x.SaveOrUpdate<PageContent>(It.Is<PageContent>(p => p.Text == summary.Content)));
		}

		[Test]
		public void AllTags_Should_Return_Correct_Items()
		{
			// Arrange
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToMockRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToMockRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToMockRepository(5, "admin", "page 5", "animals;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			List<TagSummary> summaries = manager.AllTags().OrderBy(t => t.Name).ToList();

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
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToMockRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToMockRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToMockRepository(5, "admin", "page 5", "animals;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			manager.DeletePage(page1.Id);
			manager.DeletePage(page2.Id);
			List<PageSummary> summaries = manager.AllPages().ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(3), "Page count");
			Assert.That(summaries.FirstOrDefault(p => p.Title == "Homepage"), Is.Null);
			Assert.That(summaries.FirstOrDefault(p => p.Title == "page 2"), Is.Null);
		}

		[Test]
		public void AllPages_CreatedBy_Should_Have_Correct_Authors()
		{
			// Arrange
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToMockRepository(3, "bob", "page 3", "page3;");
			PageSummary page4 = AddToMockRepository(4, "bob", "page 4", "animals;");
			PageSummary page5 = AddToMockRepository(5, "bob", "page 5", "animals;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			List<PageSummary> summaries = manager.AllPagesCreatedBy("bob").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(3), "Summary count");
			Assert.That(summaries.FirstOrDefault(p => p.CreatedBy == "admin"), Is.Null);
		}

		[Test]
		public void AllPages_Should_Have_Correct_Items()
		{
			// Arrange
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToMockRepository(3, "bob", "page 3", "page3;");
			PageSummary page4 = AddToMockRepository(4, "bob", "page 4", "animals;");
			PageSummary page5 = AddToMockRepository(5, "bob", "page 5", "animals;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			List<PageSummary> summaries = manager.AllPages().ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(5), "Summary count");
		}

		[Test]
		public void FindByTags_For_Single_Tag_Returns_Single_Result()
		{
			// Arrange
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToMockRepository(3, "admin", "page 3", "page3;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			List<PageSummary> summaries = manager.FindByTag("homepage").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(1), "Summary count");
			Assert.That(summaries[0].Title, Is.EqualTo("Homepage"), "Summary title");
			Assert.That(summaries[0].Tags.ToList()[0], Is.EqualTo("homepage"), "Summary tags");
		}

		[Test]
		public void FindByTags_For_Multiple_Tags_Returns_Many_Results()
		{
			// Arrange
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToMockRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToMockRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToMockRepository(5, "admin", "page 5", "animals;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			List<PageSummary> summaries = manager.FindByTag("animals").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(2), "Summary count");
		}

		[Test]
		public void FindByTitle_Should_Return_Correct_Page()
		{
			// Arrange
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToMockRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToMockRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToMockRepository(5, "admin", "page 5", "animals;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			PageSummary summary = manager.FindByTitle("page 3");

			// Assert
			Assert.That(summary.Title, Is.EqualTo("page 3"), "Page title");
		}

		[Test]
		public void GetById_Should_Return_Correct_Page()
		{
			// Arrange
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "page2;");
			PageSummary page3 = AddToMockRepository(3, "admin", "page 3", "page3;");
			PageSummary page4 = AddToMockRepository(4, "admin", "page 4", "animals;");
			PageSummary page5 = AddToMockRepository(5, "admin", "page 5", "animals;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			PageSummary summary = manager.GetById(page3.Id);

			// Assert
			Assert.That(summary.Id, Is.EqualTo(page3.Id), "Page id");
			Assert.That(summary.Title, Is.EqualTo("page 3"), "Page title");
		}

		[Test]
		public void ExportToXml_Should_Contain_Xml()
		{
			// Arrange
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "homepage;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "page2;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			string xml = manager.ExportToXml();

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
			PageSummary page1 = AddToMockRepository(1, "admin", "Homepage", "animal;");
			PageSummary page2 = AddToMockRepository(2, "admin", "page 2", "animal;");

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			manager.RenameTag("animal", "vegetable");
			List<PageSummary> animalTagList = manager.FindByTag("animal").ToList();
			List<PageSummary> vegetableTagList = manager.FindByTag("vegetable").ToList();

			// Assert
			Assert.That(animalTagList.Count, Is.EqualTo(0), "Old tag summary count");
			Assert.That(vegetableTagList.Count, Is.EqualTo(2), "New tag summary count");
		}

		[Test]
		public void UpdatePage_Should_Persist_To_Repository()
		{
			// Arrange
			PageSummary summary = AddToMockRepository(1, "admin", "Homepage", "animal;");
			summary.RawTags = "new,tags,";
			summary.Title = "New title";
			summary.Content = "**New content**";

			// Act
			PageManager manager = new PageManager(_config, _mockRepository.Object, _mockSearchManager.Object, _historyManager, _context);
			manager.UpdatePage(summary); // assumes it's already in the repository
			PageSummary actual = manager.GetById(1);

			// Assert
			Assert.That(actual.Title, Is.EqualTo(summary.Title), "Title");
			Assert.That(actual.Tags, Is.EqualTo(summary.Tags), "Tags");
			_mockRepository.Verify
			(
				x => x.SaveOrUpdate<Page>(It.Is<Page>(p => p.Id == summary.Id && p.Title == summary.Title && p.CreatedBy == AdminUsername && p.Tags == summary.CommaDelimitedTags()))
			);
			_mockRepository.Verify
			(
				x => x.SaveOrUpdate<PageContent>(It.Is<PageContent>(p => p.Page.Id == summary.Id && p.Text == summary.Content))
			);
		}
	}
}
