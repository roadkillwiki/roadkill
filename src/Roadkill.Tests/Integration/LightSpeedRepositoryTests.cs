using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;

namespace Roadkill.Tests.Integration
{
	[TestFixture]
	[Category("Integration")]
	public class LightSpeedRepositoryTests
	{
		//private string _connectionString = @"server=.\SQLEXPRESS;uid=sa;pwd=Passw0rd;database=roadkill;";
		//private DataStoreType _dataStoreType = DataStoreType.SqlServer2008;

		private string _connectionString = @"Data Source=roadkill-integrationtests.sqlite;";
		private DataStoreType _dataStoreType = DataStoreType.Sqlite;

		private ApplicationSettings _applicationSettings;
		private LightSpeedRepository _repository;
		private User _adminUser;
		private Guid _editorId;
		private Page _page1;
		private PageContent _pageContent1;
		private PageContent _pageContent2;

		[SetUp]
		public void Setup()
		{
			// Create dummy data.

			// Using the repository (just 4 methods) to test the repository isn't great, but it beats lots of
			// SQL scripts that are flakey for cross-database tests, and don't work for MongoDB or SimpleDB
			_applicationSettings = new ApplicationSettings();
			_applicationSettings.ConnectionString = _connectionString;
			_applicationSettings.DataStoreType = _dataStoreType;

			_repository = new LightSpeedRepository(_applicationSettings);
			_repository.Startup(_applicationSettings.DataStoreType,
								_applicationSettings.ConnectionString,
								false);

			// Clear the database
			_repository.Install(_applicationSettings.DataStoreType,
								_applicationSettings.ConnectionString,
								false);

			// Site settings
			_repository.SaveSiteSettings(new SiteSettings()
			{ 
				MarkupType = "Creole", 
				SiteName = "test", 
				SiteUrl = "http://localhost",
				Theme = "Mediawiki"
			});

			// 5 Users
			_adminUser = new User() { Email = "admin@localhost", Username = "admin", Password = "password", Salt = "123", IsAdmin = true };
			_adminUser = _repository.SaveOrUpdateUser(_adminUser);

			User editorUser = new User() { Email = "editor1@localhost", Username = "editor1", Password = "password", Salt = "123", IsEditor = true };
			editorUser = _repository.SaveOrUpdateUser(editorUser);
			_editorId = editorUser.Id;

			_repository.SaveOrUpdateUser(new User() { Email = "editor2@localhost", Username = "editor2", Password = "password", Salt = "123", IsEditor = true });
			_repository.SaveOrUpdateUser(new User() { Email = "editor3@localhost", Username = "editor3", Password = "password", Salt = "123", IsEditor = true });
			_repository.SaveOrUpdateUser(new User() { Email = "editor4@localhost", Username = "editor4", Password = "password", Salt = "123", IsEditor = true });

			// 5 Pages with 2 versions of content each
			_page1 = CreatePage("admin", "homepage, newpage");
			_pageContent1 = _repository.AddNewPage(_page1, "text", "admin", DateTime.Now);
			_page1 = _pageContent1.Page;
			_pageContent2 = _repository.AddNewPageContentVersion(_page1, "v2", "admin", DateTime.Now, 1);
			_page1 = _pageContent2.Page; // modified date

			Page page2 = CreatePage("editor1");
			PageContent pageContent2 = _repository.AddNewPage(page2, "text", "editor1", DateTime.Now);
			_repository.AddNewPageContentVersion(pageContent2.Page, "v2", "editor1", DateTime.Now, 1);

			Page page3 = CreatePage("editor2");
			PageContent pageContent3 = _repository.AddNewPage(page3, "text", "editor2", DateTime.Now);
			_repository.AddNewPageContentVersion(pageContent3.Page, "v2", "editor2", DateTime.Now, 1);

			Page page4 = CreatePage("editor3");
			PageContent pageContent4 = _repository.AddNewPage(page4, "text", "editor3", DateTime.Now);
			_repository.AddNewPageContentVersion(pageContent4.Page, "v2", "editor3", DateTime.Now, 1);

			Page page5 = CreatePage("editor4");
			PageContent pageContent5 = _repository.AddNewPage(page5, "text", "editor4", DateTime.Now);
			_repository.AddNewPageContentVersion(pageContent5.Page, "v2", "editor4", DateTime.Now, 1);
		}

		private Page CreatePage(string author, string tags = "tag1,tag2,tag3")
		{
			Page page = new Page()
			{
				Title = "Title",
				CreatedOn = DateTime.Today,
				CreatedBy = author,
				ModifiedBy = author,
				ModifiedOn = DateTime.Today,
				Tags = tags
			};

			return page;
		}

		#region IRepository Members
		[Test]
		public void DeletePage_Test()
		{
			// Arrange
			Page page = _repository.GetPageById(1);

			// Act
			_repository.DeletePage(page); 

			// Assert
			Assert.That(page, Is.Not.Null);
			Assert.That(_repository.GetPageById(1), Is.Null);
		}

		[Test]
		public void DeletePageContent()
		{
			// Arrange
			PageContent pageContent = _repository.GetLatestPageContent(1);
			Guid id = pageContent.Id;

			// Act
			_repository.DeletePageContent(pageContent);

			// Assert
			Assert.That(_repository.GetPageContentById(id), Is.Null);
		}

		[Test]
		public void DeleteUser()
		{
			// Arrange
			User user = _repository.GetUserByUsername("admin");
			Guid id = user.Id;

			// Act
			_repository.DeleteUser(user);

			// Assert
			Assert.That(_repository.GetUserById(user.Id), Is.Null);
		}

		[Test]
		public void DeleteAllPages()
		{
			// Arrange


			// Act
			_repository.DeleteAllPages();

			// Assert
			Assert.That(_repository.AllPages().Count(), Is.EqualTo(0));
			Assert.That(_repository.AllPageContents().Count(), Is.EqualTo(0));
		}

		[Test]
		public void DeleteAllPageContent()
		{
			// Arrange


			// Act
			// TODO: should remove the pages too?
			_repository.DeleteAllPageContent();

			// Assert
			Assert.That(_repository.AllPages().Count(), Is.EqualTo(5));
			Assert.That(_repository.AllPageContents().Count(), Is.EqualTo(0));
		}

		[Test]
		public void DeleteAllUsers()
		{
			// Arrange


			// Act
			_repository.DeleteAllUsers();

			// Assert
			Assert.That(_repository.FindAllAdmins().Count(), Is.EqualTo(0));
			Assert.That(_repository.FindAllEditors().Count(), Is.EqualTo(0));
		}

		[Test]
		public void SaveOrUpdatePage()
		{
			// Arrange
			Page newPage = CreatePage("admin", "tag1, 3, 4");
			DateTime modifiedDate = DateTime.Today.AddMinutes(1);

			Page existingPage = _page1;
			existingPage.Title = "new title";
			existingPage.ModifiedBy = "editor1";
			existingPage.ModifiedOn = modifiedDate;

			// Act
			_repository.SaveOrUpdatePage(newPage);
			_repository.SaveOrUpdatePage(existingPage);

			// Assert
			Assert.That(_repository.AllPages().Count(), Is.EqualTo(6));

			Page actualPage = _repository.GetPageById(existingPage.Id);
			Assert.That(actualPage.Title, Is.EqualTo("new title"));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo("editor1"));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(modifiedDate));
		}

		[Test]
		public void AddNewPage()
		{
			// Arrange
			DateTime createdDate = DateTime.Today;
			DateTime editedDate = DateTime.Today.AddMinutes(1);
			Page newPage = CreatePage("admin", "tag1,3,4");
			newPage.ModifiedOn = editedDate;

			// Act
			PageContent pageContent = _repository.AddNewPage(newPage, "my text", "admin", editedDate);

			// Assert
			Assert.That(_repository.AllPages().Count(), Is.EqualTo(6));
			Assert.That(pageContent, Is.Not.Null);
			Assert.That(pageContent.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(pageContent.Text, Is.EqualTo("my text"));
			Assert.That(pageContent.EditedOn, Is.EqualTo(editedDate));
			Assert.That(pageContent.VersionNumber, Is.EqualTo(1));

			Page actualPage = _repository.GetPageById(pageContent.Page.Id);
			Assert.That(actualPage.Title, Is.EqualTo("Title"));
			Assert.That(actualPage.Tags, Is.EqualTo("tag1,3,4"));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(createdDate));
			Assert.That(actualPage.CreatedBy, Is.EqualTo("admin"));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo("admin"));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(editedDate));
		}

		[Test]
		public void AddNewPageContentVersion()
		{
			// Arrange
			Page existingPage = _page1;
			DateTime createdDate = DateTime.Today.AddMinutes(1);

			// Act
			PageContent newContent = _repository.AddNewPageContentVersion(existingPage, "new text", "admin", createdDate, 2);

			// Assert
			Assert.That(_repository.AllPageContents().Count(), Is.EqualTo(11));
			Assert.That(newContent, Is.Not.Null);
			Assert.That(newContent.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(newContent.Text, Is.EqualTo("new text"));
			Assert.That(newContent.EditedOn, Is.EqualTo(createdDate));
			Assert.That(newContent.VersionNumber, Is.EqualTo(2));

			PageContent latestContent = _repository.GetPageContentByVersionId(newContent.Id);
			Assert.That(latestContent.Id, Is.EqualTo(newContent.Id));
			Assert.That(latestContent.Text, Is.EqualTo(newContent.Text));
			Assert.That(latestContent.EditedOn, Is.EqualTo(newContent.EditedOn));
			Assert.That(latestContent.VersionNumber, Is.EqualTo(newContent.VersionNumber));
		}

		[Test]
		public void UpdatePageContent()
		{
			// Arrange
			DateTime editedDate = DateTime.Today.AddMinutes(10);

			PageContent existingContent = _pageContent1;
			int versionNumber = 2;
			int pageId = existingContent.Page.Id;

			existingContent.Text = "new text";
			existingContent.EditedBy = "editor1";
			existingContent.EditedOn = editedDate;
			existingContent.VersionNumber = versionNumber;

			// Act
			_repository.UpdatePageContent(existingContent);
			PageContent actualContent = _repository.GetPageContentById(existingContent.Id);

			// Assert
			Assert.That(actualContent, Is.Not.Null);
			Assert.That(actualContent.Text, Is.EqualTo("new text"));
			Assert.That(actualContent.EditedBy, Is.EqualTo("editor1"));
			Assert.That(actualContent.EditedOn, Is.EqualTo(editedDate));
			Assert.That(actualContent.VersionNumber, Is.EqualTo(versionNumber));
		}

		[Test]
		public void SaveOrUpdateUser()
		{
			// Arrange
			User user = _adminUser;
			user.ActivationKey = "2key";
			user.Email = "2email@email.com";
			user.Firstname = "2firstname";
			user.IsActivated = true;
			user.IsEditor = true;
			user.Lastname = "2lastname";
			user.Password = "2password";
			user.PasswordResetKey = "2passwordkey";
			user.Salt = "2salt";
			user.Username = "2username";

			// Act
			_repository.SaveOrUpdateUser(user);
			User actualUser = _repository.GetUserById(user.Id);

			// Assert
			Assert.That(actualUser.Id, Is.EqualTo(user.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(user.ActivationKey));
			Assert.That(actualUser.Firstname, Is.EqualTo(user.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(user.IsActivated));
			Assert.That(actualUser.IsEditor, Is.EqualTo(user.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(user.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(user.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(user.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(user.Salt));
			Assert.That(actualUser.Username, Is.EqualTo(user.Username));
		}

		[Test]
		public void Get_And_SaveSiteSettings()
		{
			// Arrange
			SiteSettings expectedSettings = new SiteSettings()
			{
				AllowedFileTypes = "exe, virus, trojan",
				AllowUserSignup = true,
				IsRecaptchaEnabled = true,
				MarkupType = "Test",
				RecaptchaPrivateKey = "RecaptchaPrivateKey",
				RecaptchaPublicKey = "RecaptchaPublicKey",
				SiteName = "NewSiteName",
				SiteUrl = "http://sitename",
				Theme = "newtheme"
			};

			// Act
			_repository.SaveSiteSettings(expectedSettings);
			SiteSettings actualSettings = _repository.GetSiteSettings();

			// Assert
			Assert.That(actualSettings.AllowedFileTypes, Is.EqualTo(expectedSettings.AllowedFileTypes));
			Assert.That(actualSettings.AllowUserSignup, Is.EqualTo(expectedSettings.AllowUserSignup));
			Assert.That(actualSettings.IsRecaptchaEnabled, Is.EqualTo(expectedSettings.IsRecaptchaEnabled));
			Assert.That(actualSettings.MarkupType, Is.EqualTo(expectedSettings.MarkupType));
			Assert.That(actualSettings.RecaptchaPrivateKey, Is.EqualTo(expectedSettings.RecaptchaPrivateKey));
			Assert.That(actualSettings.RecaptchaPublicKey, Is.EqualTo(expectedSettings.RecaptchaPublicKey));
			Assert.That(actualSettings.SiteName, Is.EqualTo(expectedSettings.SiteName));
			Assert.That(actualSettings.SiteUrl, Is.EqualTo(expectedSettings.SiteUrl));
			Assert.That(actualSettings.Theme, Is.EqualTo(expectedSettings.Theme));

		}

		[Test]
		public void Install()
		{
			// Arrange


			// Act
			_repository.Install(_dataStoreType, _connectionString, false);

			// Assert
			Assert.That(_repository.AllPages().Count(), Is.EqualTo(0));
			Assert.That(_repository.AllPageContents().Count(), Is.EqualTo(0));
			Assert.That(_repository.FindAllAdmins().Count(), Is.EqualTo(0));
			Assert.That(_repository.FindAllEditors().Count(), Is.EqualTo(0));
			Assert.That(_repository.GetSiteSettings(), Is.Not.Null);
		}

		[Test]
		public void TestConnection_With_Valid_Connection_String()
		{
			// Arrange


			// Act
			_repository.TestConnection(_dataStoreType, _connectionString);

			// Assert (no exception)
		}

		[Test]
		public void TestConnection_With_Invalid_Connection_String()
		{
			// [expectedexception] can't handle exception heirachies

			// Arrange

			try
			{
				// Act
				_repository.TestConnection(_dataStoreType, "server=(local);uid=none;pwd=none;database=doesntexist;Connect Timeout=5");
			}
			catch (DbException) 
			{
				// Assert
				Assert.Pass();
			}
			catch (ArgumentException)
			{
				Assert.Pass();
			}
			catch (Exception)
			{
				Assert.Fail();
			}
		}
		#endregion

		#region IPageRepository Members

		[Test]
		public void AllPages()
		{
			// Arrange


			// Act
			List<Page> actualList = _repository.AllPages().ToList();

			// Assert
			Assert.That(actualList.Count, Is.EqualTo(5));
			Assert.That(actualList[0], Is.Not.Null);
			Assert.That(actualList[1], Is.Not.Null);
			Assert.That(actualList[2], Is.Not.Null);
			Assert.That(actualList[3], Is.Not.Null);
			Assert.That(actualList[4], Is.Not.Null);
		}

		[Test]
		public void GetPageById()
		{
			// Arrange


			// Act
			Page actualPage = _repository.GetPageById(_page1.Id);

			// Assert
			Assert.That(actualPage, Is.Not.Null);
			Assert.That(actualPage.Id, Is.EqualTo(_page1.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(_page1.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(_page1.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(_page1.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(_page1.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(_page1.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(_page1.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(_page1.Title));
		}

		[Test]
		public void FindPagesCreatedBy()
		{
			// Arrange


			// Act
			List<Page> actualPages = _repository.FindPagesCreatedBy("admin").ToList();

			// Assert
			Assert.That(actualPages.Count, Is.EqualTo(1));
			Assert.That(actualPages[0].Id, Is.EqualTo(_page1.Id));
			Assert.That(actualPages[0].CreatedBy, Is.EqualTo(_page1.CreatedBy));
			Assert.That(actualPages[0].CreatedOn, Is.EqualTo(_page1.CreatedOn));
			Assert.That(actualPages[0].IsLocked, Is.EqualTo(_page1.IsLocked));
			Assert.That(actualPages[0].ModifiedBy, Is.EqualTo(_page1.ModifiedBy));
			Assert.That(actualPages[0].ModifiedOn, Is.EqualTo(_page1.ModifiedOn));
			Assert.That(actualPages[0].Tags, Is.EqualTo(_page1.Tags));
			Assert.That(actualPages[0].Title, Is.EqualTo(_page1.Title));
		}

		[Test]
		public void FindPagesByModifiedBy()
		{
			// Arrange
			PageContent newContent = _repository.AddNewPageContentVersion(_page1, "new text", "bob", DateTime.Now, 3);
			Page expectedPage = newContent.Page;

			// Act
			List<Page> actualPages = _repository.FindPagesModifiedBy("bob").ToList();

			// Assert
			Assert.That(actualPages.Count, Is.EqualTo(1));
			Assert.That(actualPages[0].Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPages[0].CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPages[0].CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPages[0].IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPages[0].ModifiedBy, Is.EqualTo("bob"));
			Assert.That(actualPages[0].ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPages[0].Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPages[0].Title, Is.EqualTo(expectedPage.Title));
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> AllTags()
		{
			throw new NotImplementedException();
		}

		public Page GetPageByTitle(string title)
		{
			throw new NotImplementedException();
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			throw new NotImplementedException();
		}

		public PageContent GetPageContentById(Guid id)
		{
			throw new NotImplementedException();
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			throw new NotImplementedException();
		}

		public PageContent GetPageContentByVersionId(Guid versionId)
		{
			throw new NotImplementedException();
		}

		public PageContent GetPageContentByEditedBy(string username)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserRepository Members

		[Test]
		public void GetAdminById()
		{
			// Arrange
			Guid adminId = _adminUser.Id;

			// Act
			User actualUser = _repository.GetAdminById(adminId);

			// Assert
			Assert.That(actualUser.Id, Is.EqualTo(adminId));
		}

		public User GetUserByActivationKey(string key)
		{
			throw new NotImplementedException();
		}

		public User GetEditorById(Guid id)
		{
			throw new NotImplementedException();
		}

		public User GetUserByEmail(string email, bool isActivated = true)
		{
			throw new NotImplementedException();
		}

		public User GetUserById(Guid id, bool isActivated = true)
		{
			throw new NotImplementedException();
		}

		public User GetUserByPasswordResetKey(string key)
		{
			throw new NotImplementedException();
		}

		public User GetUserByUsername(string username)
		{
			throw new NotImplementedException();
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<User> FindAllEditors()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<User> FindAllAdmins()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
