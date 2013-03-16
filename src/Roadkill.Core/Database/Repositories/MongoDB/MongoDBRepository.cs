using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Roadkill.Core.Configuration;
using StructureMap.Attributes;

namespace Roadkill.Core.Database.MongoDB
{
	public class MongoDBRepository : IRepository
	{
		private IConfigurationContainer _configuration;

		public IQueryable<Page> Pages
		{
			get
			{
				return Queryable<Page>();
			}
		}

		public IQueryable<PageContent> PageContents
		{
			get
			{
				return Queryable<PageContent>();
			}
		}

		public IQueryable<User> Users
		{
			get
			{
				return Queryable<User>();
			}
		}


		public MongoDBRepository(IConfigurationContainer config)
		{
			_configuration = config;
		}

		private MongoCollection<T> GetCollection<T>()
		{
			string connectionString = _configuration.ApplicationSettings.ConnectionString;

			string databaseName = MongoUrl.Create(connectionString).DatabaseName;
			MongoClient client = new MongoClient(connectionString);
			MongoServer server = client.GetServer();
			MongoDatabase database = server.GetDatabase(databaseName);

			return database.GetCollection<T>(typeof(T).Name);
		}

		public void Delete<T>(T obj) where T : IDataStoreEntity
		{
			MongoCollection<T> collection = GetCollection<T>();
			IMongoQuery query = Query.EQ("ObjectId", obj.ObjectId);
			collection.Remove(query);
		}

		public void DeleteAll<T>() where T : IDataStoreEntity
		{
			MongoCollection<T> collection = GetCollection<T>();
			collection.RemoveAll();
		}

		public IQueryable<T> Queryable<T>() where T : IDataStoreEntity
		{
			return GetCollection<T>().AsQueryable();
		}

		public void SaveOrUpdate<T>(T obj) where T : IDataStoreEntity
		{
			// Implement autoincrement identity(1,1) for MongoDB, for Page objects
			Page page = obj as Page;
			if (page != null && page.Id == 0)
			{
				int newId = 1;
				Page recentPage = Queryable<Page>().OrderByDescending(x => x.Id).FirstOrDefault();
				if (recentPage != null)
				{
					newId = recentPage.Id + 1;
				}

				obj.ObjectId = Guid.NewGuid();
				page.Id = newId;
			}

			MongoCollection<T> collection = GetCollection<T>();
			collection.Save<T>(obj);
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			return Queryable<PageContent>()
				.Where(x => x.Page.Id == pageId)
				.OrderByDescending(x => x.EditedOn)
				.FirstOrDefault();
		}

		public SitePreferences GetSitePreferences()
		{
			SitePreferencesEntity entity = Queryable<SitePreferencesEntity>().FirstOrDefault();
			SitePreferences preferences = new SitePreferences();

			if (entity != null)
			{
				preferences = SitePreferences.LoadFromJson(entity.Content);
			}
			else
			{
				Log.Warn("MongoDB: No configuration settings could be found in the database, using a default SitePreferences");
			}

			return preferences;
		}

		public void SaveSitePreferences(SitePreferences preferences)
		{
			// Get the fresh db entity first
			SitePreferencesEntity entity = Queryable<SitePreferencesEntity>().FirstOrDefault();
			if (entity == null)
				entity = new SitePreferencesEntity();

			entity.Version = ApplicationSettings.AssemblyVersion.ToString();
			entity.Content = preferences.GetJson();
			SaveOrUpdate<SitePreferencesEntity>(entity);
		}

		public void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			// Nothing to do here
		}

		public void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			string databaseName = MongoUrl.Create(connectionString).DatabaseName;
			MongoClient client = new MongoClient(connectionString);
			MongoServer server = client.GetServer();
			MongoDatabase database = server.GetDatabase(databaseName);
			database.DropCollection("Pages");
			database.DropCollection("PageContents");
			database.DropCollection("Users");
			database.DropCollection("SitePreferences");
		}

		public void Test(DataStoreType dataStoreType, string connectionString)
		{
			string databaseName = MongoUrl.Create(connectionString).DatabaseName;
			MongoClient client = new MongoClient(connectionString);
			MongoServer server = client.GetServer();
			MongoDatabase database = server.GetDatabase(databaseName);
			database.GetCollectionNames();
		}

		public void Upgrade(IConfigurationContainer configuration)
		{
			// Delete the SitePreferences instance
		}

		public IEnumerable<Page> AllPages()
		{
			return Pages.ToList();
		}

		public Page GetPageById(int id)
		{
			return Pages.FirstOrDefault(p => p.Id == id);
		}

		public IEnumerable<Page> FindPagesByCreatedBy(string username)
		{
			return Pages.Where(p => p.CreatedBy == username);
		}

		public IEnumerable<Page> FindPagesByModifiedBy(string username)
		{
			return Pages.Where(p => p.ModifiedBy == username);
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			return Pages.Where(p => p.Tags.ToLower().Contains(tag.ToLower()));
		}

		public IEnumerable<string> AllTags()
		{
			return new List<string>(Pages.Select(p => p.Tags));
		}

		public Page GetPageByTitle(string title)
		{
			if (string.IsNullOrEmpty(title))
				return null;

			return Pages.FirstOrDefault(p => p.Title == title);
		}

		public PageContent GetPageContentById(Guid id)
		{
			return PageContents.FirstOrDefault(p => p.Id == id);
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			return PageContents.FirstOrDefault(p => p.Page.Id == id && p.VersionNumber == versionNumber);
		}

		public PageContent GetPageContentByEditedBy(string username)
		{
			return PageContents.FirstOrDefault(p => p.EditedBy == username);
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			return PageContents.Where(p => p.Page.Id == pageId);
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			return PageContents.ToList();
		}

		public User GetAdminById(Guid id)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsAdmin);
		}

		public User GetUserByActivationKey(string key)
		{
			return Users.FirstOrDefault(x => x.ActivationKey == key && x.IsActivated == false);
		}

		public User GetEditorById(Guid id)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsEditor);
		}

		public User GetUserByEmail(string email, bool isActivated = true)
		{
			return Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated);
		}

		public User GetUserById(Guid id, bool isActivated = true)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated);
		}

		public User GetUserByPasswordResetKey(string key)
		{
			return Users.FirstOrDefault(x => x.PasswordResetKey == key);
		}

		public User GetUserByUsername(string username)
		{
			return Users.FirstOrDefault(x => x.Username == username);
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			return Users.FirstOrDefault(x => x.Username == username || x.Email == email);
		}

		public IEnumerable<User> FindAllEditors()
		{
			return Users.Where(x => x.IsEditor);
		}

		public IEnumerable<User> FindAllAdmins()
		{
			return Users.Where(x => x.IsAdmin);
		}

		public PageContent GetPageContentByVersionId(Guid versionId)
		{
			return PageContents.FirstOrDefault(p => p.Id == versionId);
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			return PageContents.Where(p => p.EditedBy == username);
		}

		//-------------

		public void Dispose()
		{
			
		}

		public void DeletePage(Page page)
		{
			Delete<Page>(page);
		}

		public void DeleteUser(User user)
		{
			Delete<User>(user);
		}

		public void DeletePageContent(PageContent pageContent)
		{
			Delete<PageContent>(pageContent);
		}

		public void DeleteAllPages()
		{
			DeleteAll<Page>();
		}

		public void DeleteAllUsers()
		{
			DeleteAll<User>();
		}

		public void DeleteAllPageContent()
		{
			DeleteAll<PageContent>();
		}

		#region IRepository Members
		public void SaveOrUpdatePage(Page page)
		{
			SaveOrUpdate<Page>(page);
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			SaveOrUpdate<Page>(page);

			PageContent pageContent = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = 1,
			};

			SaveOrUpdate<PageContent>(pageContent);
			return pageContent;
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			PageContent pageContent = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = version,
			};

			SaveOrUpdate<PageContent>(pageContent);
			return pageContent;
		}

		public void SaveOrUpdateUser(User user)
		{
			SaveOrUpdate<User>(user);
		}

		public void UpdatePageContent(PageContent content)
		{
			SaveOrUpdate<PageContent>(content);
		}
		#endregion
	}
}
