using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Configuration;
using StructureMap;
using Mindscape.LightSpeed;
using AutoMapper;
using LSSitePreferencesEntity = Roadkill.Core.Database.LightSpeed.SitePreferencesEntity;
using Mindscape.LightSpeed.Querying;
using System.Data;
using Mindscape.LightSpeed.Logging;
using Mindscape.LightSpeed.Caching;
using Mindscape.LightSpeed.Linq;
using Roadkill.Core.Database.Schema;
using Roadkill.Core.Common;

namespace Roadkill.Core.Database.LightSpeed
{
	public class LightSpeedRepository : Roadkill.Core.Database.IRepository
	{
		internal IQueryable<PageEntity> Pages
		{
			get
			{
				return UnitOfWork.Query<PageEntity>();
			}
		}

		internal IQueryable<PageContentEntity> PageContents
		{
			get
			{
				return UnitOfWork.Query<PageContentEntity>();
			}
		}

		internal IQueryable<UserEntity> Users
		{
			get
			{
				return UnitOfWork.Query<UserEntity>();
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

		static LightSpeedRepository()
		{
			Mapper.CreateMap<PageEntity, Page>().ReverseMap();
			Mapper.CreateMap<PageContentEntity, PageContent>().ReverseMap();
			Mapper.CreateMap<UserEntity, User>().ReverseMap();
			Mapper.CreateMap<SitePreferencesEntity, LSSitePreferencesEntity>().ReverseMap();
		}

		public void DeletePage(Page page)
		{
			PageEntity entity = UnitOfWork.FindById<PageEntity>(page.Id);
			UnitOfWork.Remove(entity);
		}

		public void DeletePageContent(PageContent pageContent)
		{
			PageContentEntity entity = UnitOfWork.FindById<PageContentEntity>(pageContent.Id);
			UnitOfWork.Remove(entity);
		}

		public void DeleteUser(User user)
		{
			UserEntity entity = UnitOfWork.FindById<UserEntity>(user.Id);
			UnitOfWork.Remove(entity);
		}

		public void DeleteAllPages()
		{
			UnitOfWork.Remove(new Query(typeof(PageEntity)));
		}

		public void DeleteAllPageContent()
		{
			UnitOfWork.Remove(new Query(typeof(PageContentEntity)));
		}

		public void DeleteAllUsers()
		{
			UnitOfWork.Remove(new Query(typeof(UserEntity)));
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			var source = PageContents.Where(x => x.Page.Id == pageId).OrderByDescending(x => x.EditedOn).FirstOrDefault();
			return Mapper.Map<PageContent>(source);
		}

		public SitePreferences GetSitePreferences()
		{
			SitePreferencesEntity entity = UnitOfWork.Find<SitePreferencesEntity>().FirstOrDefault();
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
			SitePreferencesEntity entity = UnitOfWork.Find<SitePreferencesEntity>().FirstOrDefault();
			if (entity == null)
				entity = new SitePreferencesEntity();

			entity.Version = ApplicationSettings.Version.ToString();
			entity.Xml = preferences.GetXml();
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
				//context.VerboseLogging = true;
				//context.Logger = new TraceLogger();
				//context.Cache = new Mindscape.LightSpeed.Caching.CacheBroker(new DefaultCache());

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

			using (IDbConnection connection = context.DataProviderObjectFactory.CreateConnection())
			{
				connection.ConnectionString = connectionString;
				connection.Open();
			}
		}

		public IEnumerable<Page> AllPages()
		{
			var source = Pages;
			return Mapper.Map<IEnumerable<Page>>(source);
		}

		public Page GetPageById(int id)
		{
			var source = Pages.FirstOrDefault(p => p.Id == id);
			return Mapper.Map<Page>(source);
		}

		public IEnumerable<Page> FindPagesByCreatedBy(string username)
		{
			var source = Pages.Where(p => p.CreatedBy == username);
			return Mapper.Map<IEnumerable<Page>>(source);
		}

		public IEnumerable<Page> FindPagesByModifiedBy(string username)
		{
			var source = Pages.Where(p => p.ModifiedBy == username);
			return Mapper.Map<IEnumerable<Page>>(source);
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			var source = Pages.Where(p => p.Tags.ToLower().Contains(tag.ToLower()));
			return Mapper.Map<IEnumerable<Page>>(source);
		}

		public IEnumerable<string> AllTags()
		{
			return new List<string>(Pages.Select(p => p.Tags));
		}

		public Page GetPageByTitle(string title)
		{
			var source = Pages.FirstOrDefault(p => p.Title == title);
			return Mapper.Map<Page>(source);
		}

		public PageContent GetPageContentById(Guid id)
		{
			var source = PageContents.FirstOrDefault(p => p.Id == id);
			return Mapper.Map<PageContent>(source);
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			var source = PageContents.FirstOrDefault(p => p.Page.Id == id && p.VersionNumber == versionNumber);
			return Mapper.Map<PageContent>(source);
		}

		public PageContent GetPageContentByEditedBy(string username)
		{
			var source = PageContents.FirstOrDefault(p => p.EditedBy == username);
			return Mapper.Map<PageContent>(source);
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			var source = PageContents.Where(p => p.Page.Id == pageId);
			return Mapper.Map<IEnumerable<PageContent>>(source);
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			var source = PageContents.ToList();
			return Mapper.Map<IEnumerable<PageContent>>(source);
		}

		public User GetAdminById(Guid id)
		{
			var source = Users.FirstOrDefault(x => x.Id == id && x.IsAdmin);
			return Mapper.Map<User>(source);
		}

		public User GetUserByActivationKey(string key)
		{
			var source = Users.FirstOrDefault(x => x.ActivationKey == key && x.IsActivated == false);
			return Mapper.Map<User>(source);
		}

		public User GetEditorById(Guid id)
		{
			var source = Users.FirstOrDefault(x => x.Id == id && x.IsEditor);
			return Mapper.Map<User>(source);
		}

		public User GetUserByEmail(string email, bool isActivated = true)
		{
			var source = Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated);
			return Mapper.Map<User>(source);
		}

		public User GetUserById(Guid id, bool isActivated = true)
		{
			var source = Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated);
			return Mapper.Map<User>(source);
		}

		public User GetUserByPasswordResetKey(string key)
		{
			var source = Users.FirstOrDefault(x => x.PasswordResetKey == key);
			return Mapper.Map<User>(source);
		}

		public User GetUserByUsername(string username)
		{
			var source = Users.FirstOrDefault(x => x.Username == username);
			return Mapper.Map<User>(source);
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			var source = Users.FirstOrDefault(x => x.Username == username || x.Email == email);
			return Mapper.Map<User>(source);
		}

		public IEnumerable<User> FindAllEditors()
		{
			var source = Users.Where(x => x.IsEditor);
			return Mapper.Map<IEnumerable<User>>(source);
		}

		public IEnumerable<User> FindAllAdmins()
		{
			var source = Users.Where(x => x.IsAdmin);
			return Mapper.Map<IEnumerable<User>>(source);
		}

		public PageContent GetPageContentByVersionId(Guid versionId)
		{
			var source = PageContents.FirstOrDefault(p => p.Id == versionId);
			return Mapper.Map<PageContent>(source);
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			var source = PageContents.Where(p => p.EditedBy == username);
			return Mapper.Map<IEnumerable<PageContent>>(source);
		}

		public void Dispose()
		{
			UnitOfWork.SaveChanges();
			UnitOfWork.Dispose();
		}

		public void SaveOrUpdatePage(Page page)
		{
			PageEntity entity = UnitOfWork.FindById<PageEntity>(page.Id);
			if (entity == null)
			{
				entity = Mapper.Map<PageEntity>(page);
				UnitOfWork.Add(entity);
			}
			else
			{
				entity = Mapper.Map<Page, PageEntity>(page);
				UnitOfWork.Import<PageEntity>(entity);
			}

			UnitOfWork.SaveChanges();
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			PageEntity pageEntity = Mapper.Map<PageEntity>(page);
			pageEntity.Id = 0;
			UnitOfWork.Add(pageEntity);

			PageContentEntity pageContent = new PageContentEntity()
			{
				Id = Guid.NewGuid(),
				Page = pageEntity,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = 1,
			};

			UnitOfWork.Add(pageContent);
			UnitOfWork.SaveChanges();

			return Mapper.Map<PageContentEntity, PageContent>(pageContent);
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			PageEntity pageEntity = UnitOfWork.FindById<PageEntity>(page.Id);
			if (pageEntity != null)
			{
				PageContentEntity pageContent = new PageContentEntity()
				{
					Id = Guid.NewGuid(),
					Page = pageEntity,
					Text = text,
					EditedBy = editedBy,
					EditedOn = editedOn,
					VersionNumber = version,
				};

				UnitOfWork.Add(pageContent);
				UnitOfWork.SaveChanges();
				return Mapper.Map<PageContentEntity, PageContent>(pageContent);
			}

			Log.Error("Unable to update page content for page id {0} (not found)", page.Id);
			return null;
		}

		public void SaveOrUpdateUser(User user)
		{
			UserEntity entity = UnitOfWork.FindById<UserEntity>(user.Id);
			if (entity == null)
			{
				entity = Mapper.Map<UserEntity>(user);
				UnitOfWork.Add(entity);
			}
			else
			{
				entity = Mapper.Map<User, UserEntity>(user);
				UnitOfWork.Import<UserEntity>(entity);
			}

			UnitOfWork.SaveChanges();
		}

		public void UpdatePageContent(PageContent content)
		{
			PageContentEntity entity = UnitOfWork.FindById<PageContentEntity>(content.Id);
			if (entity != null)
			{
				entity = Mapper.Map<PageContent, PageContentEntity>(content);
				UnitOfWork.Import<PageContentEntity>(entity);
			}

			UnitOfWork.SaveChanges();
		}
	}
}
