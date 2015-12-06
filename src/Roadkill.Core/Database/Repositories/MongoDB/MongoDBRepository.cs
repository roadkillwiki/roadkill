using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Core.Database.MongoDB
{
	public class MongoDBRepository : IRepository
	{
		internal readonly string ConnectionString;

		public IQueryable<Page> Pages => Queryable<Page>();
		public IQueryable<PageContent> PageContents => Queryable<PageContent>();
		public IQueryable<User> Users => Queryable<User>();

		public MongoDBRepository(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public void Wipe()
		{
			string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
			MongoClient client = new MongoClient(ConnectionString);
			MongoServer server = client.GetServer();
			MongoDatabase database = server.GetDatabase(databaseName);

			database.DropCollection(typeof(PageContent).Name);
			database.DropCollection(typeof(Page).Name);
			database.DropCollection(typeof(User).Name);
			database.DropCollection(typeof(SiteConfigurationEntity).Name);
		}

		private MongoCollection<T> GetCollection<T>()
		{
			string connectionString = ConnectionString;

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

		public SiteSettings GetSiteSettings()
		{
			SiteConfigurationEntity entity = Queryable<SiteConfigurationEntity>()
												.FirstOrDefault(x => x.Id == SiteSettings.SiteSettingsId);
			SiteSettings siteSettings = new SiteSettings();

			if (entity != null)
			{
				siteSettings = SiteSettings.LoadFromJson(entity.Content);
			}
			else
			{
				Log.Warn("MongoDB: No site settings could be found in the database, using a default SiteSettings");
			}

			return siteSettings;
		}

		public PluginSettings GetTextPluginSettings(Guid databaseId)
		{
			SiteConfigurationEntity entity = Queryable<SiteConfigurationEntity>()
												.FirstOrDefault(x => x.Id == databaseId);

			PluginSettings pluginSettings = null;

			if (entity != null)
			{
				pluginSettings = PluginSettings.LoadFromJson(entity.Content);
			}

			return pluginSettings;
		}

		public void SaveTextPluginSettings(TextPlugin plugin)
		{
			SiteConfigurationEntity entity = Queryable<SiteConfigurationEntity>()
												.FirstOrDefault(x => x.Id == plugin.DatabaseId);

			if (entity == null)
				entity = new SiteConfigurationEntity();

			entity.Id = plugin.DatabaseId;
			entity.Version = plugin.Version;
			entity.Content = plugin.Settings.GetJson();
			SaveOrUpdate<SiteConfigurationEntity>(entity);
		}

		public void SaveSiteSettings(SiteSettings preferences)
		{
			// Get the fresh db entity first
			SiteConfigurationEntity entity = Queryable<SiteConfigurationEntity>()
												.FirstOrDefault(x => x.Id == SiteSettings.SiteSettingsId);
			if (entity == null)
				entity = new SiteConfigurationEntity();

			entity.Id = SiteSettings.SiteSettingsId;
			entity.Version = ApplicationSettings.ProductVersion.ToString();
			entity.Content = preferences.GetJson();
			SaveOrUpdate<SiteConfigurationEntity>(entity);
		}

		public IEnumerable<Page> AllPages()
		{
			return Pages.ToList();
		}

		public Page GetPageById(int id)
		{
			return Pages.FirstOrDefault(p => p.Id == id);
		}

		public IEnumerable<Page> FindPagesCreatedBy(string username)
		{
			return Pages.Where(p => p.CreatedBy == username);
		}

		public IEnumerable<Page> FindPagesModifiedBy(string username)
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

		public IEnumerable<PageContent> GetPageContentByEditedBy(string username)
		{
			return PageContents.Where(p => p.EditedBy == username);
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

		public User GetUserByEmail(string email, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated.HasValue);
			else
				return Users.FirstOrDefault(x => x.Email == email);
		}

		public User GetUserById(Guid id, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated.Value);
			else
				return Users.FirstOrDefault(x => x.Id == id);
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

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			return PageContents.Where(p => p.EditedBy == username);
		}

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
			DeleteAll<PageContent>();
		}

		public void DeleteAllUsers()
		{
			DeleteAll<User>();
		}

		#region IRepository Members
		public Page SaveOrUpdatePage(Page page)
		{
			SaveOrUpdate<Page>(page);
			return page;
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
			page.ModifiedOn = editedOn;
			page.ModifiedBy = editedBy;
			SaveOrUpdate<Page>(page);

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

		public User SaveOrUpdateUser(User user)
		{
			SaveOrUpdate<User>(user);
			return user;
		}

		public void UpdatePageContent(PageContent content)
		{
			SaveOrUpdate<PageContent>(content);
		}
		#endregion
	}
}
