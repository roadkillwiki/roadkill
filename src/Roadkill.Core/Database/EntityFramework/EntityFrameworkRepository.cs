using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Database.EntityFramework
{
	public class EntityFrameworkRepository : IRepository
	{
		private IConfigurationContainer _configuration;
		private RoadkillDbContext _context;

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


		public EntityFrameworkRepository(IConfigurationContainer config)
		{
			_configuration = config;
			_context = RoadkillDbContext.Create(config);
		}

		public void Delete<T>(T obj) where T : DataStoreEntity
		{
			Page page = obj as Page;
			if (page != null)
			{
				_context.Set<T>().Remove(obj);
			}
		}

		public void DeleteAll<T>() where T : DataStoreEntity
		{
			
		}

		public IQueryable<T> Queryable<T>() where T : DataStoreEntity
		{
			return _context.Set<T>().AsQueryable();
		}

		public void SaveOrUpdate<T>(T obj) where T : DataStoreEntity
		{
			T entity = _context.Entry<T>(obj).Entity;

			if (entity.ObjectId == Guid.Empty)
				_context.Set<T>().Add(obj);

			_context.SaveChanges();
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
				preferences = SitePreferences.LoadFromXml(entity.Xml);
			}
			else
			{
				Log.Warn("No configuration settings could be found in the database, using a default instance");
			}

			return preferences;
		}

		public void SaveSitePreferences(SitePreferences preferences)
		{
			// Get the fresh db entity first
			SitePreferencesEntity entity = _context.Set<SitePreferencesEntity>().FirstOrDefault();
			if (entity == null)
				entity = new SitePreferencesEntity();

			entity.Version = ApplicationSettings.Version.ToString();
			entity.Xml = preferences.GetXml();
			SaveOrUpdate<SitePreferencesEntity>(entity);
		}

		public void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			// Nothing to do here
		}

		public void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			
		}

		public void Test(DataStoreType dataStoreType, string connectionString)
		{
			
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

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
