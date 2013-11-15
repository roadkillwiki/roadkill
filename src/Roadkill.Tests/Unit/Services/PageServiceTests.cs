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
using Roadkill.Tests.Unit.StubsAndMocks;

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

		public static string EditorEmail = "editor@localhost";
		public static string EditorUsername = "editor";
		public static string EditorPassword = "password";

		private User _adminUser;
		private User _editorUser;

		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.ConnectionString = "connstring";
			_context = _container.UserContext;
			_repository = _container.Repository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;

			// User setup
			_editorUser = new User();
			_editorUser.Id = Guid.NewGuid();
			_editorUser.Email = EditorEmail;
			_editorUser.Username = EditorUsername;
			_editorUser.IsAdmin = false;
			_editorUser.IsEditor = true;

			_adminUser = new User();
			_adminUser.Id = Guid.NewGuid();
			_adminUser.Email = AdminEmail;
			_adminUser.Username = AdminUsername;
			_adminUser.IsAdmin = true;
			_adminUser.IsEditor = true;

			_userService.Users.Add(_editorUser);
			_userService.Users.Add(_adminUser);
			SetUserContext(_adminUser);
		}

		private void SetUserContext(User user)
		{
			_context.CurrentUser = user.Id.ToString();
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

			PageContent content = _repository.AddNewPage(page, textContent, createdBy, createdOn);
			PageViewModel model = new PageViewModel()
			{
				Id = id,
				Title = title,
				Content = textContent,
				RawTags = tags,
				CreatedBy = createdBy,
				CreatedOn = createdOn
			};

			return model;
		}

		[Test]
		public void AddPage_Should_Save_To_Repository_And_Set_Locked_If_User_Is_Admin()
		{
			// Arrange
			PageViewModel model = new PageViewModel()
			{
				Id = 1,
				Title = "Homepage",
				Content = "**Homepage**",
				RawTags = "1;2;3;",
				CreatedBy = AdminUsername,
				CreatedOn = DateTime.UtcNow,
				IsLocked = true
			};

			// Act
			PageViewModel actualModel = _pageService.AddPage(model);

			// Assert
			Assert.That(actualModel, Is.Not.Null);
			Assert.That(actualModel.Content, Is.EqualTo(model.Content));
			Assert.That(actualModel.IsLocked, Is.True);
			Assert.That(_repository.Pages.Count, Is.EqualTo(1));
			Assert.That(_repository.PageContents.Count, Is.EqualTo(1));
		}

		[Test]
		public void AddPage_Should_Not_Set_IsLocked_If_User_Is_Editor()
		{
			// Arrange
			PageViewModel model = new PageViewModel()
			{
				Id = 1,
				Title = "Homepage",
				Content = "**Homepage**",
				RawTags = "1;2;3;",
				CreatedBy = AdminUsername,
				CreatedOn = DateTime.UtcNow,
				IsLocked = true
			};

			SetUserContext(_editorUser);	

			// Act
			PageViewModel newModel = _pageService.AddPage(model);

			// Assert
			Assert.That(newModel.IsLocked, Is.False);
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
			List<TagViewModel> models = _pageService.AllTags().OrderBy(t => t.Name).ToList();

			// Assert
			Assert.That(models.Count, Is.EqualTo(4), "Tag summary count");
			Assert.That(models[0].Name, Is.EqualTo("animals"));
			Assert.That(models[1].Name, Is.EqualTo("homepage"));
			Assert.That(models[2].Name, Is.EqualTo("page2"));
			Assert.That(models[3].Name, Is.EqualTo("page3"));
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
			List<PageViewModel> models = _pageService.AllPages().ToList();

			// Assert
			Assert.That(models.Count, Is.EqualTo(3), "Page count");
			Assert.That(models.FirstOrDefault(p => p.Title == "Homepage"), Is.Null);
			Assert.That(models.FirstOrDefault(p => p.Title == "page 2"), Is.Null);
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
			List<PageViewModel> models = _pageService.AllPagesCreatedBy("bob").ToList();

			// Assert
			Assert.That(models.Count, Is.EqualTo(3), "Summary count");
			Assert.That(models.FirstOrDefault(p => p.CreatedBy == "admin"), Is.Null);
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
			List<PageViewModel> models = _pageService.AllPages().ToList();

			// Assert
			Assert.That(models.Count, Is.EqualTo(5), "Summary count");
		}

		[Test]
		public void FindByTags_For_Single_Tag_Returns_Single_Result()
		{
			// Arrange
			PageViewModel page1 = AddToStubbedRepository(1, "admin", "Homepage", "homepage;");
			PageViewModel page2 = AddToStubbedRepository(2, "admin", "page 2", "page2;");
			PageViewModel page3 = AddToStubbedRepository(3, "admin", "page 3", "page3;");

			// Act
			List<PageViewModel> models = _pageService.FindByTag("homepage").ToList();

			// Assert
			Assert.That(models.Count, Is.EqualTo(1), "Summary count");
			Assert.That(models[0].Title, Is.EqualTo("Homepage"), "Summary title");
			Assert.That(models[0].Tags.ToList()[0], Is.EqualTo("homepage"), "Summary tags");
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
			List<PageViewModel> models = _pageService.FindByTag("animals").ToList();

			// Assert
			Assert.That(models.Count, Is.EqualTo(2), "Summary count");
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
			PageViewModel model = _pageService.FindByTitle("page 3");

			// Assert
			Assert.That(model.Title, Is.EqualTo("page 3"), "Page title");
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
			PageViewModel model = _pageService.GetById(page3.Id);

			// Assert
			Assert.That(model.Id, Is.EqualTo(page3.Id), "Page id");
			Assert.That(model.Title, Is.EqualTo("page 3"), "Page title");
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
			PageViewModel model = AddToStubbedRepository(1, "admin", "Homepage", "animal;");
			string expectedTags = "new,tags";

			// Act
			model.RawTags = "new,tags,";
			model.Title = "New title";
			model.Content = "**New content**";

			_pageService.UpdatePage(model);
			PageViewModel actual = _pageService.GetById(1);

			// Assert
			Assert.That(actual.Title, Is.EqualTo(model.Title), "Title");
			Assert.That(actual.Tags, Is.EqualTo(model.Tags), "Tags");

			Assert.That(_repository.Pages[0].Tags, Is.EqualTo(expectedTags));
			Assert.That(_repository.Pages[0].Title, Is.EqualTo(model.Title));
			Assert.That(_repository.PageContents[1].Text, Is.EqualTo(model.Content)); // "smells"
		}
	}
}
