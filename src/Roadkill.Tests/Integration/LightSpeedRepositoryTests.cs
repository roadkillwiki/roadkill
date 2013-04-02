using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;

namespace Roadkill.Tests.Integration
{
	public class LightSpeedRepositoryTests
	{
		private ApplicationSettings _applicationSettings;
		private LightSpeedRepository _repository;

		[Test]
		[Ignore]
		public void CreateDummyDatabase()
		{
			// Disable [Setup] before running this
			// Database is created when it's installed
			ApplicationSettings settings = new ApplicationSettings();
			//settings.ConnectionString = "Data Source=roadkill-repository-tests.sqlite;";
			settings.ConnectionString = "server=localhost;uid=root;pwd=Passw0rd;database=roadkill;";
			settings.DataStoreType = DataStoreType.MySQL;

			_repository = new LightSpeedRepository(settings);
			_repository.Startup(settings.DataStoreType,
								settings.ConnectionString,
								false);

			// Clear the database
			_repository.Install(settings.DataStoreType,
								settings.ConnectionString,
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
			_repository.SaveOrUpdateUser(new User() { Email = "admin@localhost", Username = "admin", Password = "password", Salt="123", IsAdmin = true });
			_repository.SaveOrUpdateUser(new User() { Email = "editor1@localhost", Username = "editor1", Password = "password", Salt = "123", IsEditor = true, });
			_repository.SaveOrUpdateUser(new User() { Email = "editor2@localhost", Username = "editor2", Password = "password", Salt = "123", IsEditor = true });
			_repository.SaveOrUpdateUser(new User() { Email = "editor3@localhost", Username = "editor3", Password = "password", Salt = "123", IsEditor = true });
			_repository.SaveOrUpdateUser(new User() { Email = "editor4@localhost", Username = "editor4", Password = "password", Salt = "123", IsEditor = true });

			// 5 Pages and their content
			Page page1 = CreatePage("admin", "homepage, newpage");
			PageContent pageContent1 = _repository.AddNewPage(page1, "text", "admin", DateTime.Now);
			_repository.AddNewPageContentVersion(pageContent1.Page, "v2", "admin", DateTime.Now, 1);

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
				CreatedOn = DateTime.Now,
				CreatedBy = author,
				ModifiedBy = author,
				ModifiedOn = DateTime.Now,
				Tags = tags
			};

			return page;
		}

		//[SetUp]
		public void Setup()
		{
			// Copy the data-filled database file for each test
			string sourceFile = Path.Combine(GlobalSetup.LIB_FOLDER, "Test-databases", "roadkill-repository-tests.sqlite");
			string destFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roadkill-repository-tests.sqlite");
			//File.Copy(sourceFile, destFile, true);

			_applicationSettings = new ApplicationSettings();
			_applicationSettings.ConnectionString = "server=localhost;uid=root;pwd=Passw0rd;database=roadkill;";
			_applicationSettings.DataStoreType = DataStoreType.MySQL;

			_repository = new LightSpeedRepository(_applicationSettings);
			_repository.Startup(_applicationSettings.DataStoreType,
								_applicationSettings.ConnectionString,
								false);

			//_repository.EnableSqlLogging();
		}

		#region IRepository Members
		[Test]
		[Ignore]
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
		[Ignore]
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

		public void DeleteUser(User user)
		{
			throw new NotImplementedException();
		}

		public void DeleteAllPages()
		{
			throw new NotImplementedException();
		}

		public void DeleteAllPageContent()
		{
			throw new NotImplementedException();
		}

		public void DeleteAllUsers()
		{
			throw new NotImplementedException();
		}

		public void SaveOrUpdatePage(Page page)
		{
			throw new NotImplementedException();
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			throw new NotImplementedException();
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			throw new NotImplementedException();
		}

		public void UpdatePageContent(PageContent content)
		{
			throw new NotImplementedException();
		}

		public void SaveOrUpdateUser(User user)
		{
			throw new NotImplementedException();
		}

		public void SaveSiteSettings(Core.Configuration.SiteSettings siteSettings)
		{
			throw new NotImplementedException();
		}

		public Core.Configuration.SiteSettings GetSiteSettings()
		{
			throw new NotImplementedException();
		}

		public void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			throw new NotImplementedException();
		}

		public void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			throw new NotImplementedException();
		}

		public void Test(DataStoreType dataStoreType, string connectionString)
		{
			throw new NotImplementedException();
		}

		public void Upgrade(Core.Configuration.ApplicationSettings applicationSettings)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IPageRepository Members

		public IEnumerable<Page> AllPages()
		{
			throw new NotImplementedException();
		}

		public Page GetPageById(int id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Page> FindPagesByCreatedBy(string username)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Page> FindPagesByModifiedBy(string username)
		{
			throw new NotImplementedException();
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

		public User GetAdminById(Guid id)
		{
			throw new NotImplementedException();
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

		#region IDisposable Members

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
