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
		private User _editor;
		private User _inactiveUser;
		private Page _page1;
		private PageContent _pageContent1;
		private PageContent _pageContent2;

		[SetUp]
		public void Setup()
		{
			// Create dummy data.

			// ############################
			// Using the repository (just 4 methods) to test the repository isn't great, but it beats lots of
			// SQL scripts that are flakey for cross-database tests, and don't work for MongoDB or SimpleDB
			// ############################

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
			_adminUser = NewUser("admin@localhost", "admin", true, true);
			_adminUser = _repository.SaveOrUpdateUser(_adminUser);

			_editor = NewUser("editor1@localhost", "editor1", false, true);
			_editor = _repository.SaveOrUpdateUser(_editor);

			_inactiveUser = NewUser("editor2@localhost", "editor2", false, true, false);
			_inactiveUser = _repository.SaveOrUpdateUser(_inactiveUser);
			
			_repository.SaveOrUpdateUser(NewUser("editor3@localhost", "editor3", false, true));
			_repository.SaveOrUpdateUser(NewUser("editor4@localhost", "editor4", false, true, false));

			// 5 Pages with 2 versions of content each
			DateTime createdDate = DateTime.Today;
			DateTime editedDate = DateTime.Today.AddHours(1);

			_page1 = NewPage("admin", "homepage, newpage");
			_pageContent1 = _repository.AddNewPage(_page1, "text", "admin", createdDate);
			_page1 = _pageContent1.Page;
			_pageContent2 = _repository.AddNewPageContentVersion(_page1, "v2", "admin", editedDate, 2);
			_page1 = _pageContent2.Page; // update the modified date

			Page page2 = NewPage("editor1");
			PageContent pageContent2 = _repository.AddNewPage(page2, "text", "editor1", createdDate);
			_repository.AddNewPageContentVersion(pageContent2.Page, "v2", "editor1", editedDate, 1);

			Page page3 = NewPage("editor2");
			PageContent pageContent3 = _repository.AddNewPage(page3, "text", "editor2", createdDate);
			_repository.AddNewPageContentVersion(pageContent3.Page, "v2", "editor2", editedDate, 1);

			Page page4 = NewPage("editor3");
			PageContent pageContent4 = _repository.AddNewPage(page4, "text", "editor3", createdDate);
			_repository.AddNewPageContentVersion(pageContent4.Page, "v2", "editor3", editedDate, 1);

			Page page5 = NewPage("editor4");
			PageContent pageContent5 = _repository.AddNewPage(page5, "text", "editor4", createdDate);
			_repository.AddNewPageContentVersion(pageContent5.Page, "v2", "editor4", editedDate, 1);
		}

		private Page NewPage(string author, string tags = "tag1,tag2,tag3", string title="Title")
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

		private User NewUser(string email, string username, bool isAdmin, bool isEditor, bool isActive = true)
		{
			return new User()
			{
				Email = email,
				Username = username,
				Password = "password",
				Salt = "123",
				IsActivated = isActive,
				IsAdmin = isAdmin,
				IsEditor = isEditor,
				ActivationKey = Guid.NewGuid().ToString(),
				Firstname = "Firstname",
				Lastname = "Lastname",
				PasswordResetKey = Guid.NewGuid().ToString()
			};
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
			Page newPage = NewPage("admin", "tag1, 3, 4");
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
			Page newPage = NewPage("admin", "tag1,3,4");
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

			PageContent latestContent = _repository.GetPageContentById(newContent.Id);
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

		[Test]
		public void FindPagesContainingTag()
		{
			// Arrange


			// Act
			List<Page> actualPages = _repository.FindPagesContainingTag("tag1").ToList();


			// Assert
			Assert.That(actualPages.Count, Is.EqualTo(4));
		}

		[Test]
		public void AllTags()
		{
			// Arrange


			// Act
			List<string> actual = _repository.AllTags().ToList();

			// Assert
			Assert.That(actual.Count, Is.EqualTo(5)); // homepage, newpage, tag1, tag2, tag3
		}

		[Test]
		public void GetPageByTitle()
		{
			// Arrange
			string title = "page title";
			Page expectedPage = NewPage("admin", "tag1", title);
			PageContent newContent = _repository.AddNewPage(expectedPage, "sometext", "admin", DateTime.Today);
			expectedPage.Id = newContent.Page.Id; // get the new identity

			// Act
			Page actualPage = _repository.GetPageByTitle(title);

			// Assert
			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(expectedPage.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(expectedPage.Title));
		}

		[Test]
		public void GetLatestPageContent()
		{
			// Arrange
			PageContent expectedContent = _pageContent2;
			Page expectedPage = _pageContent2.Page;

			// Act
			PageContent actualContent = _repository.GetLatestPageContent(_pageContent2.Page.Id);
			Page actualPage = actualContent.Page;

			// Assert
			Assert.That(actualContent.EditedBy, Is.EqualTo(expectedContent.EditedBy));
			Assert.That(actualContent.EditedOn, Is.EqualTo(expectedContent.EditedOn));
			Assert.That(actualContent.Id, Is.EqualTo(expectedContent.Id));
			Assert.That(actualContent.Text, Is.EqualTo(expectedContent.Text));
			Assert.That(actualContent.VersionNumber, Is.EqualTo(expectedContent.VersionNumber));

			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(expectedPage.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(expectedPage.Title));
		}

		[Test]
		public void GetPageContentById()
		{
			// Arrange
			PageContent expectedContent = _pageContent2;
			Page expectedPage = _pageContent2.Page;

			// Act
			PageContent actualContent = _repository.GetPageContentById(expectedContent.Id);
			Page actualPage = actualContent.Page;

			// Assert
			Assert.That(actualContent.EditedBy, Is.EqualTo(expectedContent.EditedBy));
			Assert.That(actualContent.EditedOn, Is.EqualTo(expectedContent.EditedOn));
			Assert.That(actualContent.Id, Is.EqualTo(expectedContent.Id));
			Assert.That(actualContent.Text, Is.EqualTo(expectedContent.Text));
			Assert.That(actualContent.VersionNumber, Is.EqualTo(expectedContent.VersionNumber));

			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(expectedPage.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(expectedPage.Title));
		}

		[Test]
		public void GetPageContentByPageIdAndVersionNumber()
		{
			// Arrange
			PageContent expectedContent = _pageContent2;
			Page expectedPage = _pageContent2.Page;

			// Act
			PageContent actualContent = _repository.GetPageContentByPageIdAndVersionNumber(expectedPage.Id, expectedContent.VersionNumber);
			Page actualPage = actualContent.Page;

			// Assert
			Assert.That(actualContent.EditedBy, Is.EqualTo(expectedContent.EditedBy));
			Assert.That(actualContent.EditedOn, Is.EqualTo(expectedContent.EditedOn));
			Assert.That(actualContent.Id, Is.EqualTo(expectedContent.Id));
			Assert.That(actualContent.Text, Is.EqualTo(expectedContent.Text));
			Assert.That(actualContent.VersionNumber, Is.EqualTo(expectedContent.VersionNumber));

			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(expectedPage.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(expectedPage.Title));
		}

		[Test]
		public void GetPageContentByEditedBy()
		{
			// Arrange

			// Act
			List<PageContent> allContent = _repository.GetPageContentByEditedBy("admin").ToList();

			// Assert
			Assert.That(allContent.Count, Is.EqualTo(2));
		}

		[Test]
		public void FindPageContentsByPageId()
		{
			// Arrange


			// Act
			List<PageContent> pagesContents = _repository.FindPageContentsByPageId(_page1.Id).ToList();

			// Assert
			Assert.That(pagesContents.Count, Is.EqualTo(2));
			Assert.That(pagesContents[0], Is.Not.Null);

			PageContent expectedPageContent = pagesContents.FirstOrDefault(x => x.Id == _pageContent1.Id);
			Assert.That(expectedPageContent, Is.Not.Null);
		}

		[Test]
		public void FindPageContentsEditedBy()
		{
			// Arrange


			// Act
			List<PageContent> pagesContents = _repository.FindPageContentsEditedBy("admin").ToList();

			// Assert
			Assert.That(pagesContents.Count, Is.EqualTo(2));
			Assert.That(pagesContents[0], Is.Not.Null);

			PageContent expectedPageContent = pagesContents.FirstOrDefault(x => x.Id == _pageContent1.Id);
			Assert.That(expectedPageContent, Is.Not.Null);
		}

		[Test]
		public void AllPageContents()
		{
			// Arrange


			// Act
			List<PageContent> pagesContents = _repository.AllPageContents().ToList();

			// Assert
			Assert.That(pagesContents.Count, Is.EqualTo(10)); // five pages with 2 versions
			Assert.That(pagesContents[0], Is.Not.Null);

			PageContent expectedPageContent = pagesContents.FirstOrDefault(x => x.Id == _pageContent1.Id);
			Assert.That(expectedPageContent, Is.Not.Null);
		}

		#endregion

		#region IUserRepository Members

		[Test]
		public void GetAdminById()
		{
			// Arrange
			User expectedUser = _adminUser;

			// Act
			User noUser = _repository.GetAdminById(_editor.Id);
			User actualUser = _repository.GetAdminById(expectedUser.Id);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void GetUserByActivationKey_With_InactiveUser()
		{
			// Arrange
			User expectedUser = _inactiveUser;

			// Act
			User noUser = _repository.GetUserByActivationKey("badkey");
			User actualUser = _repository.GetUserByActivationKey(expectedUser.ActivationKey);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void GetUserByActivationKey_With_ActiveUser()
		{
			// Arrange
			User expectedUser = _adminUser;

			// Act
			User actualUser = _repository.GetUserByActivationKey(expectedUser.ActivationKey);

			// Assert
			Assert.That(actualUser, Is.Null);
		}

		[Test]
		public void GetEditorById()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = _repository.GetEditorById(Guid.Empty);
			User actualUser = _repository.GetEditorById(_editor.Id);
			User adminUser = _repository.GetEditorById(_adminUser.Id);

			// Assert
			Assert.That(noUser, Is.Null);
			Assert.That(adminUser, Is.Not.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void GetUserByEmail()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = _repository.GetUserByEmail("invalid@email.com");
			User actualUser = _repository.GetUserByEmail(_editor.Email);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void GetUserByEmail_With_Inactive_User()
		{
			// Arrange
			User expectedUser = null;

			// Act
			User noUser = _repository.GetUserByUsername("nobody");
			User actualUser = _repository.GetUserByEmail(_inactiveUser.Email);

			// Assert
			Assert.That(noUser, Is.Null);
			Assert.That(actualUser, Is.EqualTo(expectedUser));
		}

		[Test]
		public void GetUserByEmail_Not_Activated_With_Inactive_User()
		{
			// Arrange
			User expectedUser = _inactiveUser;

			// Act
			User actualUser = _repository.GetUserByEmail(_inactiveUser.Email, false);

			// Assert
			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}
		
		[Test]
		public void GetUserById()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = _repository.GetUserById(Guid.Empty);
			User actualUser = _repository.GetUserById(_editor.Id);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void GetUserById_With_Inactive_User()
		{
			// Arrange
			User expectedUser = null;

			// Act
			User actualUser = _repository.GetUserById(_inactiveUser.Id);

			// Assert
			Assert.That(actualUser, Is.EqualTo(expectedUser));
		}

		[Test]
		public void GetUserById_NotActivated_With_Inactive_User()
		{
			// Arrange
			User expectedUser = _inactiveUser;

			// Act
			User noUser = _repository.GetUserById(_editor.Id, false);
			User actualUser = _repository.GetUserById(_inactiveUser.Id, false);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void GetUserByPasswordResetKey()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = _repository.GetUserByUsername("badkey");
			User actualUser = _repository.GetUserByPasswordResetKey(_editor.PasswordResetKey);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void GetUserByUsername()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = _repository.GetUserByUsername("nobody");
			User actualUser = _repository.GetUserByUsername(_editor.Username);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void GetUserByUsernameOrEmail()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = _repository.GetUserByUsernameOrEmail("nobody", "nobody@nobody.com");
			User emailUser = _repository.GetUserByUsernameOrEmail("nousername", _editor.Email);
			User actualUser = _repository.GetUserByUsernameOrEmail(_editor.Username, "doesntexist@email.com");

			// Assert
			Assert.That(noUser, Is.Null);
			Assert.That(emailUser, Is.Not.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void FindAllEditors()
		{
			// Arrange


			// Act
			List<User> allEditors = _repository.FindAllEditors().ToList();

			// Assert
			Assert.That(allEditors.Count, Is.EqualTo(5)); // includes the admin
		}

		[Test]
		public void FindAllAdmins()
		{
			// Arrange


			// Act
			List<User> allEditors = _repository.FindAllAdmins().ToList();

			// Assert
			Assert.That(allEditors.Count, Is.EqualTo(1));
		}

		#endregion
	}
}
