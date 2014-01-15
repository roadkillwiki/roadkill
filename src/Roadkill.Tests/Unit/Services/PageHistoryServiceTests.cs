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
using Roadkill.Core.Database;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PageHistoryServiceTests
	{
		public static string AdminEmail = "admin@localhost";
		public static string AdminUsername = "admin";
		public static string AdminPassword = "password";

		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageHistoryService _historyService;

		private IUserContext _context;
		private User _testUser;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;	
			_repository = _container.Repository;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;

			_testUser = new User();
			_testUser.IsActivated = true;
			_testUser.Id = Guid.NewGuid();
			_testUser.Email = AdminEmail;
			_testUser.Username = AdminUsername;
			_userService.Users.Add(_testUser);

			_context.CurrentUser = _testUser.Id.ToString();
		}

		[Test]
		public void CompareVersions_Has_Last_Two_Versions()
		{
			// Arrange
			DateTime createdDate = DateTime.Today.AddDays(-1);
			Page page = NewPage("admin");
			PageContent v1Content = _repository.AddNewPage(page, "v1 text", "admin", createdDate);
			PageContent v2Content = _repository.AddNewPageContentVersion(page, "v2 text", "admin", createdDate.AddHours(1), 2);
			PageContent v3Content = _repository.AddNewPageContentVersion(page, "v3 text", "admin", createdDate.AddHours(2), 3);
			PageContent v4Content = _repository.AddNewPageContentVersion(page, "v4 text", "admin", createdDate.AddHours(3), 4);

			// Act
			List<PageViewModel> versionList = _historyService.CompareVersions(v4Content.Id).ToList();

			// Assert
			Assert.That(versionList.Count, Is.EqualTo(2));
			Assert.That(versionList[0].Id, Is.EqualTo(v3Content.Page.Id));
			Assert.That(versionList[1].Id, Is.EqualTo(v4Content.Page.Id));
		}

		[Test]
		public void CompareVersions_With_One_Page_Version_Returns_One_Item()
		{
			// Arrange
			Page page = NewPage("admin");
			PageContent v1Content = _repository.AddNewPage(page, "v1 text", "admin", DateTime.Today.AddDays(-1));

			// Act
			List<PageViewModel> versionList = _historyService.CompareVersions(v1Content.Id).ToList();

			// Assert
			Assert.That(versionList.Count, Is.EqualTo(2));
			Assert.That(versionList[0].Id, Is.EqualTo(v1Content.Page.Id));
			Assert.That(versionList[1], Is.Null);
		}

		[Test]
		public void GetHistory_Returns_Correct_Items()
		{
			// Arrange
			Page page = NewPage("admin");
			PageContent v1Content = _repository.AddNewPage(page, "v1 text", "admin", DateTime.Today.AddDays(-1));
			PageContent v2Content = _repository.AddNewPageContentVersion(page, "v2 text", "admin", DateTime.Today.AddDays(-1).AddHours(1), 2);

			page = v2Content.Page; // update the id
			page.IsLocked = true;

			// Act
			List<PageHistoryViewModel> historyList = _historyService.GetHistory(v1Content.Page.Id).ToList();

			// Assert
			Assert.That(historyList.Count, Is.EqualTo(2));
			Assert.That(historyList[0].Id, Is.EqualTo(v2Content.Id));
			Assert.That(historyList[0].EditedBy, Is.EqualTo(v2Content.EditedBy));
			Assert.That(historyList[0].EditedOn, Is.EqualTo(v2Content.EditedOn));
			Assert.That(historyList[0].EditedOnWithOffset, Is.Not.Empty);
			Assert.That(historyList[0].IsPageAdminOnly, Is.EqualTo(page.IsLocked));
			Assert.That(historyList[0].PageId, Is.EqualTo(page.Id));
			Assert.That(historyList[0].VersionNumber, Is.EqualTo(v2Content.VersionNumber));
		}

		[Test]
		public void GetHistory_Returns_Items_In_Correct_Order()
		{
			// Arrange
			DateTime createdDate = DateTime.Today.AddDays(-1);
			Page page = NewPage("admin");
			PageContent v1Content = _repository.AddNewPage(page, "v1 text", "admin", createdDate);
			PageContent v2Content = _repository.AddNewPageContentVersion(page, "v2 text", "admin", createdDate.AddHours(1), 2);
			PageContent v3Content = _repository.AddNewPageContentVersion(page, "v3 text", "admin", createdDate.AddHours(2), 3);
			PageContent v4Content = _repository.AddNewPageContentVersion(page, "v4 text", "admin", createdDate.AddHours(3), 4);

			// Act
			List<PageHistoryViewModel> historyList = _historyService.GetHistory(v1Content.Page.Id).ToList();

			// Assert
			Assert.That(historyList.Count, Is.EqualTo(4));
			Assert.That(historyList[0].Id, Is.EqualTo(v4Content.Id));
			Assert.That(historyList[1].Id, Is.EqualTo(v3Content.Id));
			Assert.That(historyList[2].Id, Is.EqualTo(v2Content.Id));
			Assert.That(historyList[3].Id, Is.EqualTo(v1Content.Id));
		}

		[Test]
		public void MaxVersion_Returns_Correct_Version_Number()
		{
			// Arrange
			DateTime createdDate = DateTime.Today.AddDays(-1);
			Page page = NewPage("admin");
			PageContent v1Content = _repository.AddNewPage(page, "v1 text", "admin", createdDate);
			page = v1Content.Page;
			PageContent v2Content = _repository.AddNewPageContentVersion(page, "v2 text", "admin", createdDate.AddHours(1), 2);
			PageContent v3Content = _repository.AddNewPageContentVersion(page, "v3 text", "admin", createdDate.AddHours(2), 3);
			PageContent v4Content = _repository.AddNewPageContentVersion(page, "v4 text", "admin", createdDate.AddHours(3), 4);

			int expectedVersion = 4;

			// Act
			int actualVersion = _historyService.MaxVersion(page.Id);

			// Assert
			Assert.That(actualVersion, Is.EqualTo(expectedVersion));
		}

		[Test]
		public void RevertTo_With_VersionId_Should_Add_New_Version()
		{
			// Arrange
			DateTime createdDate = DateTime.Today.AddDays(-1);
			_context.CurrentUser = "someoneelse";
			Page page = NewPage("admin");
			PageContent v1Content = _repository.AddNewPage(page, "v1 text", "admin", createdDate);
			page = v1Content.Page;
			PageContent v2Content = _repository.AddNewPageContentVersion(page, "v2 text", "admin", createdDate.AddHours(1), 2);

			// Act
			_historyService.RevertTo(v1Content.Id, _context);
			PageContent actualContent = _repository.GetLatestPageContent(page.Id);

			// Assert
			Assert.That(actualContent.VersionNumber, Is.EqualTo(3));
			Assert.That(actualContent.Text, Is.EqualTo(v1Content.Text));
			Assert.That(actualContent.EditedBy, Is.EqualTo(_context.CurrentUsername));
		}

		[Test]
		public void RevertTo_With_PageId_Should_Add_New_Version()
		{
			// Arrange
			DateTime createdDate = DateTime.Today.AddDays(-1);
			Page page = NewPage("admin");
			PageContent v1Content = _repository.AddNewPage(page, "v1 text", "admin", createdDate);
			page = v1Content.Page;
			PageContent v2Content = _repository.AddNewPageContentVersion(page, "v2 text", "admin", createdDate.AddHours(1), 2);

			// Act
			_historyService.RevertTo(page.Id, 1);
			PageContent actualContent = _repository.GetLatestPageContent(page.Id);

			// Assert
			Assert.That(actualContent.VersionNumber, Is.EqualTo(3));
			Assert.That(actualContent.Text, Is.EqualTo(v1Content.Text));
			Assert.That(actualContent.EditedBy, Is.EqualTo("admin"));
		}

		private Page NewPage(string author, string tags = "tag1,tag2,tag3", string title = "Title")
		{
			Page page = new Page()
			{
				Title = title,
				CreatedOn = DateTime.Today,
				CreatedBy = author,
				ModifiedBy = author,
				ModifiedOn = DateTime.Today,
				Tags = tags
			};

			return page;
		}
	}
}
