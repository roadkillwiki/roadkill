using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class HistoryManagerTests
	{
		public static string AdminEmail = "admin@localhost";
		public static string AdminUsername = "admin";
		public static string AdminPassword = "password";

		private User _testUser;

		private RepositoryMock _repositoryMock;
		private ApplicationSettings _settings;
		private Mock<UserManagerBase> _mockUserManager;
		private UserContext _context;
		private HistoryManager _historyManager;

		[SetUp]
		public void Setup()
		{
			_repositoryMock = new RepositoryMock();
			_settings = new ApplicationSettings();
			_settings.Installed = true;

			_testUser = new User();
			_testUser.Id = Guid.NewGuid();
			_testUser.Email = AdminEmail;
			_testUser.Username = AdminUsername;
			Guid userId = _testUser.Id;

			_mockUserManager = new Mock<UserManagerBase>(_settings, _repositoryMock);
			_mockUserManager.Setup(x => x.GetUser(_testUser.Email, It.IsAny<bool>())).Returns(_testUser);
			_mockUserManager.Setup(x => x.GetUserById(userId, It.IsAny<bool>())).Returns(_testUser);
			_mockUserManager.Setup(x => x.Authenticate(_testUser.Email, "")).Returns(true);
			_mockUserManager.Setup(x => x.GetLoggedInUserName(It.IsAny<HttpContextBase>())).Returns(_testUser.Username);

			// Context stub
			_context = new UserContext(_mockUserManager.Object);
			_context.CurrentUser = userId.ToString();

			_historyManager = new HistoryManager(_settings, _repositoryMock, _context, new PageSummaryCache(_settings));
		}

		[Test]
		public void CompareVersions_Has_Last_Two_Versions()
		{
			// Arrange
			Page page = NewPage("admin");
			PageContent v1Content = _repositoryMock.AddNewPage(page, "v1 text", "admin", DateTime.Today);
			PageContent v2Content = _repositoryMock.AddNewPageContentVersion(page, "v2 text", "admin", DateTime.Today.AddHours(1), 2);
			PageContent v3Content = _repositoryMock.AddNewPageContentVersion(page, "v3 text", "admin", DateTime.Today.AddHours(2), 3);
			PageContent v4Content = _repositoryMock.AddNewPageContentVersion(page, "v4 text", "admin", DateTime.Today.AddHours(3), 4);

			// Act
			List<PageSummary> versionList = _historyManager.CompareVersions(v4Content.Id).ToList();

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
			PageContent v1Content = _repositoryMock.AddNewPage(page, "v1 text", "admin", DateTime.Today);

			// Act
			List<PageSummary> versionList = _historyManager.CompareVersions(v1Content.Id).ToList();

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
			PageContent v1Content = _repositoryMock.AddNewPage(page, "v1 text", "admin", DateTime.Today);
			PageContent v2Content = _repositoryMock.AddNewPageContentVersion(page, "v2 text", "admin", DateTime.Today.AddHours(1), 2);

			page = v2Content.Page; // update the id
			page.IsLocked = true;

			// Act
			List<HistorySummary> historyList = _historyManager.GetHistory(v1Content.Page.Id).ToList();

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
			Page page = NewPage("admin");
			PageContent v1Content = _repositoryMock.AddNewPage(page, "v1 text", "admin", DateTime.Today);
			PageContent v2Content = _repositoryMock.AddNewPageContentVersion(page, "v2 text", "admin", DateTime.Today.AddHours(1), 2);
			PageContent v3Content = _repositoryMock.AddNewPageContentVersion(page, "v3 text", "admin", DateTime.Today.AddHours(2), 3);
			PageContent v4Content = _repositoryMock.AddNewPageContentVersion(page, "v4 text", "admin", DateTime.Today.AddHours(3), 4);

			// Act
			List<HistorySummary> historyList = _historyManager.GetHistory(v1Content.Page.Id).ToList();

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
			Page page = NewPage("admin");
			PageContent v1Content = _repositoryMock.AddNewPage(page, "v1 text", "admin", DateTime.Today);
			page = v1Content.Page;
			PageContent v2Content = _repositoryMock.AddNewPageContentVersion(page, "v2 text", "admin", DateTime.Today.AddHours(1), 2);
			PageContent v3Content = _repositoryMock.AddNewPageContentVersion(page, "v3 text", "admin", DateTime.Today.AddHours(2), 3);
			PageContent v4Content = _repositoryMock.AddNewPageContentVersion(page, "v4 text", "admin", DateTime.Today.AddHours(3), 4);

			int expectedVersion = 4;

			// Act
			int actualVersion = _historyManager.MaxVersion(page.Id);

			// Assert
			Assert.That(actualVersion, Is.EqualTo(expectedVersion));
		}

		[Test]
		public void RevertTo_With_VersionId_Should_Add_New_Version()
		{
			// Arrange
			_context.CurrentUser = "someoneelse";
			Page page = NewPage("admin");
			PageContent v1Content = _repositoryMock.AddNewPage(page, "v1 text", "admin", DateTime.Today);
			page = v1Content.Page;
			PageContent v2Content = _repositoryMock.AddNewPageContentVersion(page, "v2 text", "admin", DateTime.Today.AddHours(1), 2);

			// Act
			_historyManager.RevertTo(v1Content.Id, _context);
			PageContent actualContent = _repositoryMock.GetLatestPageContent(page.Id);

			// Assert
			Assert.That(actualContent.VersionNumber, Is.EqualTo(3));
			Assert.That(actualContent.Text, Is.EqualTo(v1Content.Text));
			Assert.That(actualContent.EditedBy, Is.EqualTo(_context.CurrentUsername));
		}

		[Test]
		public void RevertTo_With_PageId_Should_Add_New_Version()
		{
			// Arrange
			Page page = NewPage("admin");
			PageContent v1Content = _repositoryMock.AddNewPage(page, "v1 text", "admin", DateTime.Today);
			page = v1Content.Page;
			PageContent v2Content = _repositoryMock.AddNewPageContentVersion(page, "v2 text", "admin", DateTime.Today.AddHours(1), 2);

			// Act
			_historyManager.RevertTo(page.Id, 1);
			PageContent actualContent = _repositoryMock.GetLatestPageContent(page.Id);

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
