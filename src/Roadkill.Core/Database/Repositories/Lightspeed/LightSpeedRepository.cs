using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Caching;
using Mindscape.LightSpeed.Linq;
using Mindscape.LightSpeed.Logging;
using Mindscape.LightSpeed.Querying;
using Roadkill.Core.Common;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database.Schema;
using StructureMap;
using LSSitePreferencesEntity = Roadkill.Core.Database.LightSpeed.SitePreferencesEntity;

namespace Roadkill.Core.Database.LightSpeed
{
	public class LightSpeedRepository : Roadkill.Core.Database.IRepository
	{
		private ApplicationSettings _applicationSettings;

		internal IQueryable<PageEntity> Pages
		{
			get
			{
				return UnitOfWork.Query<PageEntity>().ToList().AsQueryable();
			}
		}

		internal IQueryable<PageContentEntity> PageContents
		{
			get
			{
				return UnitOfWork.Query<PageContentEntity>().ToList().AsQueryable();
			}
		}

		internal IQueryable<UserEntity> Users
		{
			get
			{
				return UnitOfWork.Query<UserEntity>().ToList().AsQueryable();
			}
		}

		public virtual LightSpeedContext Context
		{
			get
			{
				LightSpeedContext context = ObjectFactory.GetInstance<LightSpeedContext>();
				if (context == null)
					throw new DatabaseException("The context for Lightspeed is null - has Startup() been called?", null);

				return context;
			}
		}


		public virtual IUnitOfWork UnitOfWork
		{
			get
			{
				IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
				if (unitOfWork == null)
					throw new DatabaseException("The IUnitOfWork for Lightspeed is null - has Startup() been called?", null);

				return unitOfWork;
			}
		}

		public LightSpeedRepository(ApplicationSettings settings)
		{
			_applicationSettings = settings;
		}

		public void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			if (!string.IsNullOrEmpty(connectionString))
			{
				LightSpeedContext context = new LightSpeedContext();
				context.ConnectionString = connectionString;
				context.DataProvider = dataStoreType.LightSpeedDbType;
				context.IdentityMethod = IdentityMethod.GuidComb;
				context.CascadeDeletes = false;
				context.VerboseLogging = true;
				context.Cache = new CacheBroker(new DefaultCache());
				context.Logger = new TraceLogger();				

				ObjectFactory.Configure(x =>
				{
					x.For<LightSpeedContext>().Singleton().Use(context);
					x.For<IUnitOfWork>().HybridHttpOrThreadLocalScoped().Use(ctx => ctx.GetInstance<LightSpeedContext>().CreateUnitOfWork());
				});
			}
		}

		public void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			LightSpeedContext context = ObjectFactory.GetInstance<LightSpeedContext>();
			if (context == null)
				throw new InvalidOperationException("Repository.Install failed - LightSpeedContext was null from the ObjectFactory");

			using (IDbConnection connection = context.DataProviderObjectFactory.CreateConnection())
			{
				connection.ConnectionString = connectionString;
				connection.Open();

				IDbCommand command = context.DataProviderObjectFactory.CreateCommand();
				command.Connection = connection;

				dataStoreType.Schema.Drop(command);
				dataStoreType.Schema.Create(command);
			}
		}

		public void Test(DataStoreType dataStoreType, string connectionString)
		{
			LightSpeedContext context = ObjectFactory.GetInstance<LightSpeedContext>();
			if (context == null)
				throw new InvalidOperationException("Repository.Test failed - LightSpeedContext was null from the ObjectFactory");

			using (IDbConnection connection = context.DataProviderObjectFactory.CreateConnection())
			{
				connection.ConnectionString = connectionString;
				connection.Open();
			}
		}

		public void Upgrade(ApplicationSettings settings)
		{
			try
			{
				using (IDbConnection connection = Context.DataProviderObjectFactory.CreateConnection())
				{
					connection.ConnectionString = settings.ConnectionString;
					connection.Open();

					IDbCommand command = Context.DataProviderObjectFactory.CreateCommand();
					command.Connection = connection;

					settings.DataStoreType.Schema.Upgrade(command);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Upgrade failed: {0}", ex);
				throw new UpgradeException("A problem occurred upgrading the database schema.\n\n", ex);
			}

			try
			{
				SaveSiteSettings(new SiteSettings());
			}
			catch (Exception ex)
			{
				Log.Error("Upgrade failed: {0}", ex);
				throw new UpgradeException("A problem occurred saving the site preferences.\n\n", ex);
			}
		}

		public SiteSettings GetSiteSettings()
		{
			SiteSettings preferences = new SiteSettings();
			SitePreferencesEntity entity = UnitOfWork.FindById<SitePreferencesEntity>(SiteSettings.SiteSettingsId);

			if (entity != null)
			{
				preferences = SiteSettings.LoadFromJson(entity.Content);
			}
			else
			{
				Log.Warn("No configuration settings could be found in the database, using a default instance");
			}

			return preferences;
		}

		public void SaveSiteSettings(SiteSettings preferences)
		{
			SitePreferencesEntity entity = UnitOfWork.Find<SitePreferencesEntity>().FirstOrDefault();

			if (entity == null || entity.Id == Guid.Empty)
			{
				entity = new SitePreferencesEntity();
				entity.Version = ApplicationSettings.AssemblyVersion.ToString();
				entity.Content = preferences.GetJson();
				UnitOfWork.Add(entity);
			}
			else
			{
				entity.Version = ApplicationSettings.AssemblyVersion.ToString();
				entity.Content = preferences.GetJson();
			}

			UnitOfWork.SaveChanges();
		}

		public void DeletePage(Page page)
		{
			PageEntity entity = UnitOfWork.FindById<PageEntity>(page.Id);
			UnitOfWork.Remove(entity);
			UnitOfWork.SaveChanges();
		}

		public void DeletePageContent(PageContent pageContent)
		{
			PageContentEntity entity = UnitOfWork.FindById<PageContentEntity>(pageContent.Id);
			UnitOfWork.Remove(entity);
			UnitOfWork.SaveChanges();
		}

		public void DeleteUser(User user)
		{
			UserEntity entity = UnitOfWork.FindById<UserEntity>(user.Id);
			UnitOfWork.Remove(entity);
			UnitOfWork.SaveChanges();
		}

		public void DeleteAllPages()
		{
			UnitOfWork.Remove(new Query(typeof(PageEntity)));
			UnitOfWork.SaveChanges();
		}

		public void DeleteAllPageContent()
		{
			UnitOfWork.Remove(new Query(typeof(PageContentEntity)));
			UnitOfWork.SaveChanges();
		}

		public void DeleteAllUsers()
		{
			UnitOfWork.Remove(new Query(typeof(UserEntity)));
			UnitOfWork.SaveChanges();
		}

		public IEnumerable<Page> AllPages()
		{
			List<PageEntity> entities = Pages.ToList();
			return FromEntity.ToPageList(entities);
		}

		public Page GetPageById(int id)
		{
			PageEntity entity = Pages.FirstOrDefault(p => p.Id == id);
			return FromEntity.ToPage(entity);
		}

		public IEnumerable<Page> FindPagesByCreatedBy(string username)
		{
			List<PageEntity> entities = Pages.Where(p => p.CreatedBy == username).ToList();
			return FromEntity.ToPageList(entities);
		}

		public IEnumerable<Page> FindPagesByModifiedBy(string username)
		{
			List<PageEntity> entities = Pages.Where(p => p.ModifiedBy == username).ToList();
			return FromEntity.ToPageList(entities);
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			IEnumerable<PageEntity> entities = new List<PageEntity>();

			if (_applicationSettings.DataStoreType != DataStoreType.Postgres)
			{
				entities = Pages.Where(p => p.Tags.ToLower().Contains(tag.ToLower()));
			}
			else
			{
				// Temporary Lightspeed Postgres LIKE bug work around
				IDbCommand command = UnitOfWork.Context.DataProviderObjectFactory.CreateCommand();
				command.CommandText = "SELECT * FROM roadkill_pages WHERE tags LIKE @Tag"; // case sensitive column name
				IDbDataParameter parameter = command.CreateParameter();
				parameter.DbType = DbType.String;
				parameter.ParameterName = "@Tag";
				parameter.Value = "%" +tag+ "%";
				command.Parameters.Add(parameter);

				entities = UnitOfWork.FindBySql<PageEntity>(command);
			}

			return FromEntity.ToPageList(entities);
		}

		public IEnumerable<string> AllTags()
		{
			return new List<string>(Pages.Select(p => p.Tags));
		}

		public Page GetPageByTitle(string title)
		{
			PageEntity entity = Pages.FirstOrDefault(p => p.Title == title);
			return FromEntity.ToPage(entity);
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			PageContentEntity entity = PageContents.Where(x => x.Page.Id == pageId).OrderByDescending(x => x.EditedOn).FirstOrDefault();
			return FromEntity.ToPageContent(entity);
		}

		public PageContent GetPageContentById(Guid id)
		{
			PageContentEntity entity = PageContents.FirstOrDefault(p => p.Id == id);
			return FromEntity.ToPageContent(entity);
		}

		public PageContent GetPageContentByVersionId(Guid versionId)
		{
			PageContentEntity entity = PageContents.FirstOrDefault(p => p.Id == versionId);
			return FromEntity.ToPageContent(entity);
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			PageContentEntity entity = PageContents.FirstOrDefault(p => p.Page.Id == id && p.VersionNumber == versionNumber);
			return FromEntity.ToPageContent(entity);
		}

		public PageContent GetPageContentByEditedBy(string username)
		{
			PageContentEntity entity = PageContents.FirstOrDefault(p => p.EditedBy == username);
			return FromEntity.ToPageContent(entity);
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			List<PageContentEntity> entities = PageContents.Where(p => p.Page.Id == pageId).ToList();
			return FromEntity.ToPageContentList(entities);
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			List<PageContentEntity> entities = PageContents.ToList();
			return FromEntity.ToPageContentList(entities);
		}

		public User GetAdminById(Guid id)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Id == id && x.IsAdmin);
			return FromEntity.ToUser(entity);
		}

		public User GetUserByActivationKey(string key)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.ActivationKey == key && x.IsActivated == false);
			return FromEntity.ToUser(entity);
		}

		public User GetEditorById(Guid id)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Id == id && x.IsEditor);
			return FromEntity.ToUser(entity);
		}

		public User GetUserByEmail(string email, bool isActivated = true)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated);
			return FromEntity.ToUser(entity);
		}

		public User GetUserById(Guid id, bool isActivated = true)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated);
			return FromEntity.ToUser(entity);
		}

		public User GetUserByPasswordResetKey(string key)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.PasswordResetKey == key);
			return FromEntity.ToUser(entity);
		}

		public User GetUserByUsername(string username)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Username == username);
			return FromEntity.ToUser(entity);
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Username == username || x.Email == email);
			return FromEntity.ToUser(entity);
		}

		public IEnumerable<User> FindAllEditors()
		{
			List<UserEntity> entities = Users.Where(x => x.IsEditor).ToList();
			return FromEntity.ToUserList(entities);
		}

		public IEnumerable<User> FindAllAdmins()
		{
			List<UserEntity> entities = Users.Where(x => x.IsAdmin).ToList();
			return FromEntity.ToUserList(entities);
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			List<PageContentEntity> entities = PageContents.Where(p => p.EditedBy == username).ToList();
			return FromEntity.ToPageContentList(entities);
		}

		public void SaveOrUpdatePage(Page page)
		{
			PageEntity entity = UnitOfWork.FindById<PageEntity>(page.Id);
			if (entity == null)
			{
				entity = new PageEntity();
				ToEntity.FromPage(page, entity);
				UnitOfWork.Add(entity);
			}
			else
			{
				ToEntity.FromPage(page, entity);
			}

			UnitOfWork.SaveChanges();
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			PageEntity pageEntity = new PageEntity();
			ToEntity.FromPage(page, pageEntity);
			pageEntity.Id = 0;
			UnitOfWork.Add(pageEntity);

			PageContentEntity pageContentEntity = new PageContentEntity()
			{
				Id = Guid.NewGuid(),
				Page = pageEntity,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = 1,
			};

			UnitOfWork.Add(pageContentEntity);
			UnitOfWork.SaveChanges();

			PageContent pageContent = FromEntity.ToPageContent(pageContentEntity);
			pageContent.Page = FromEntity.ToPage(pageEntity);
			return pageContent;
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			PageEntity pageEntity = UnitOfWork.FindById<PageEntity>(page.Id);
			if (pageEntity != null)
			{
				PageContentEntity pageContentEntity = new PageContentEntity()
				{
					Id = Guid.NewGuid(),
					Page = pageEntity,
					Text = text,
					EditedBy = editedBy,
					EditedOn = editedOn,
					VersionNumber = version,
				};

				UnitOfWork.Add(pageContentEntity);
				UnitOfWork.SaveChanges();

				PageContent pageContent = FromEntity.ToPageContent(pageContentEntity);
				pageContent.Page = FromEntity.ToPage(pageEntity);
				return pageContent;
			}

			Log.Error("Unable to update page content for page id {0} (not found)", page.Id);
			return null;
		}

		public void SaveOrUpdateUser(User user)
		{
			UserEntity entity = UnitOfWork.FindById<UserEntity>(user.Id);
			if (entity == null)
			{
				entity = new UserEntity();
				ToEntity.FromUser(user, entity);
				UnitOfWork.Add(entity);
			}
			else
			{
				ToEntity.FromUser(user, entity);
			}

			UnitOfWork.SaveChanges();
		}

		public void UpdatePageContent(PageContent content)
		{
			PageContentEntity entity = UnitOfWork.FindById<PageContentEntity>(content.Id);
			if (entity != null)
			{
				ToEntity.FromPageContent(content, entity);
				UnitOfWork.SaveChanges();
			}
		}
		public void Dispose()
		{
			UnitOfWork.SaveChanges();
			UnitOfWork.Dispose();
		}
	}
}
