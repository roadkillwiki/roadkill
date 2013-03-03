using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Linq;
using System.Data;
using NHibernateConfig = NHibernate.Cfg.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Configuration;
using StructureMap;

namespace Roadkill.Core
{
	/// <summary>
	/// A fluent NHibernate-based repository.
	/// </summary>
	public class NHibernateRepository : IRepository
	{
		/// <summary>
		/// Gets a LINQ-to-NHibernate <see cref="Queryable{Page}"/> object to perform queries with.
		/// </summary>
		public IQueryable<Page> Pages
		{
			get
			{
				return Queryable<Page>();
			}
		}

		/// <summary>
		/// Gets a LINQ-to-NHibernate <see cref="Queryable{PageContent}"/> object to perform queries with.
		/// </summary>
		public IQueryable<PageContent> PageContents
		{
			get
			{
				return Queryable<PageContent>();
			}
		}

		/// <summary>
		/// Gets a LINQ-to-NHibernate <see cref="Queryable{User}"/> object to perform queries with.
		/// </summary>
		public IQueryable<User> Users
		{
			get
			{
				return Queryable<User>();
			}
		}

		/// <summary>
		/// The current NHibernate <see cref="ISessionFactory"/>. This is created once, the first the NHibernateRepository is used.
		/// </summary>
		public virtual ISessionFactory SessionFactory
		{
			get
			{
				ISessionFactory factory = ObjectFactory.GetInstance<ISessionFactory>();
				if (factory == null)
					throw new DatabaseException("The ISessionFactory for NHibernateRepository is null - has Startup() been called?", null);

				return factory;
			}
		}

		public virtual ISession Session
		{
			get
			{
				return ObjectFactory.GetInstance<ISession>();
			}
		}

		public virtual NHibernateConfig NHibernateConfig
		{
			get
			{
				return ObjectFactory.GetInstance<NHibernateConfig>();
			}
		}

		/// <summary>
		/// The current Fluent NHibernate <see cref="FluentConfiguration"/> object that represents the current NHibernate configuration.
		/// </summary>
		public virtual FluentConfiguration Configuration { get; protected set; }

		/// <summary>
		/// Provides a LINQ-to-NHibernate <see cref="IQueryable{T}"/> object to query with.
		/// Session disposal is the responsibility of the caller.
		/// </summary>
		/// <typeparam name="T">The domain type to query against.</typeparam>
		/// <returns><see cref="IQueryable{T}"/> for LINQ-to-NHibernate LINQ queries.</returns>
		public virtual IQueryable<T> Queryable<T>() where T : DataStoreEntity
		{
			IQueryable<T> queryable = Session.Query<T>();
			queryable = queryable.Cacheable<T>();

			return queryable;
		}

		/// <summary>
		/// Deletes the object from the database.
		/// </summary>
		/// <param name="obj">The object to delete.</param>
		public virtual void Delete<T>(T obj) where T : DataStoreEntity
		{
			using (Session.BeginTransaction())
			{
				Session.Delete(obj);
				Session.Transaction.Commit();
			}
		}

		/// <summary>
		/// Deletes alls objects from the database.
		/// </summary>
		public virtual void DeleteAll<T>() where T : DataStoreEntity
		{
			string className = typeof(T).FullName;
			using (Session.BeginTransaction()) // 2.1 uses transactions by default
			{
				// TODO: use ClassExtractor for a more intelligent way
				Session.CreateQuery(string.Format("DELETE {0} o", className)).ExecuteUpdate();
				Session.Transaction.Commit();
			}
		}

		/// <summary>
		/// Inserts or updates the object depending on whether it exists in the database.
		/// </summary>
		/// <param name="obj">The object to insert/update.</param>
		public virtual void SaveOrUpdate<T>(T obj) where T : DataStoreEntity
		{
			using (Session.BeginTransaction())
			{
				Session.SaveOrUpdate(obj);
				Session.Transaction.Commit();
			}
		}

		/// <summary>
		/// Runs a query that does not select any rows.
		/// </summary>
		/// <param name="sql">The sql query to run</param>
		/// <returns>The number of rows affected.</returns>
		public virtual int ExecuteNonQuery(string sql)
		{
			ISQLQuery query = Session.CreateSQLQuery(sql);
			return query.ExecuteUpdate();
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			PageContent latest;
			if (NHibernateConfig.GetProperty("dataStoreType") == DataStoreType.SqlServerCe.Name)
			{
				// Work around for an NHibernate 3.3.1 SQL CE bug with the HQL query in CurrentContent() - this is two SQL queries per page instead of one.
				latest = Session.QueryOver<PageContent>().Where(p => p.Page.Id == pageId).OrderBy(p => p.VersionNumber).Desc.Take(1).SingleOrDefault();
				latest.Page = Session.Get<Page>(latest.Page.Id);
			}
			else
			{
				// Fetches the parent page object via SQL as well as the PageContent, avoiding lazy loading.
				IQuery query = Session
						.CreateQuery("FROM PageContent fetch all properties WHERE Page.Id=:Id AND VersionNumber=(SELECT max(VersionNumber) FROM PageContent WHERE Page.Id=:Id)");

				query.SetCacheable(true);
				query.SetInt32("Id", pageId);
				query.SetMaxResults(1);
				latest = query.UniqueResult<PageContent>();
			}

			return latest;
		}

		public SitePreferences GetSitePreferences()
		{
			return Queryable<SitePreferences>().FirstOrDefault(s => s.Id == SitePreferences.ConfigurationId);
		}

		public IEnumerable<Page> AllPages()
		{
			return Pages;
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
			try
			{
				if (string.IsNullOrEmpty(title))
					return null;

				return Pages.FirstOrDefault(p => p.Title == title);
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred finding the page with title '{0}' in the database", title);
			}
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
			return PageContents;
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

		// ---- SETUP ----

		public void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			InitializeSessionFactory(dataStoreType, connectionString, enableCache, false);
		}

		public void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			InitializeSessionFactory(dataStoreType, connectionString, enableCache, true);
		}

		public void InitializeSessionFactory(DataStoreType dataStoreType, string connectionString, bool enableCache, bool createSchema)
		{
			// These are valid states, not exceptions
			if (dataStoreType == null)
				return;

			if (string.IsNullOrEmpty(connectionString))
				return;

			NHibernateConfig config = new NHibernateConfig();
			Configuration = Fluently.Configure(config);
			Configuration.Mappings(m => m.FluentMappings.AddFromAssemblyOf<Page>());
			config.SetProperty("dataStoreType", dataStoreType.Name);

			if (createSchema)
				Configuration.ExposeConfiguration(c => new SchemaExport(c).Execute(false, true, false));

			// Only configure the Databasetype if it's not already in the config file
			if (!config.Properties.ContainsKey("connection.driver_class"))
			{
				SetDatabase(dataStoreType, connectionString);
			}

			if (!config.Properties.ContainsKey("connection.connection_string"))
			{
				config.SetProperty("connection.connection_string", connectionString);
			}

			// Only configure the caching if it's not already in the config file
			if (!config.Properties.ContainsKey("cache.use_second_level_cache"))
			{
				if (enableCache)
				{
					// SQL CE's second level cache breaks with QueryOver
					if (dataStoreType != DataStoreType.SqlServerCe)
					{
						Configuration.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache().UseSecondLevelCache());
					}
				}
			}

			try
			{
				ISessionFactory sessionFactory = Configuration.BuildSessionFactory();

				// Store one SessionFactory per http request or thread if no http context.
				// StructureMap does all this magic for us.
				ObjectFactory.Configure(x =>
				{
					x.For<NHibernate.Cfg.Configuration>().Singleton().Use(config);
					x.For<ISessionFactory>().Singleton().Use(sessionFactory);
					x.For<ISession>().HybridHttpOrThreadLocalScoped().Use(ctx => ctx.GetInstance<ISessionFactory>().OpenSession());
				});
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public void Test(DataStoreType dataStoreType, string connection)
		{
			NHibernateConfig config = new NHibernateConfig();
			Configuration = Fluently.Configure(config);
			Configuration.Mappings(m => m.FluentMappings.AddFromAssemblyOf<Page>());

			SetDatabase(dataStoreType, connection);
			config.SetProperty("connection.connection_string", connection);

			try
			{
				// Don't do anything with the SessionFactory as it's just to test the connection
				ISessionFactory factory = Configuration.BuildSessionFactory();
				factory.OpenSession();
			}
			catch (HibernateException e)
			{
				throw new DatabaseException("NHibernate exception occurred testing the database", e);
			}
		}

		private void SetDatabase(DataStoreType dataStoreType, string connection)
		{
			if (dataStoreType == DataStoreType.DB2)
			{
				DB2Configuration db2 = DB2Configuration.Standard.ConnectionString(connection);
				Configuration.Database(db2);
			}
			else if (dataStoreType == DataStoreType.Firebird)
			{
				FirebirdConfiguration fireBird = new FirebirdConfiguration();
				fireBird.ConnectionString(connection);
				Configuration.Database(fireBird);
			}
			else if (dataStoreType == DataStoreType.MySQL)
			{
				MySQLConfiguration mySql = MySQLConfiguration.Standard.ConnectionString(connection);
				Configuration.Database(mySql);
			}
			else if (dataStoreType == DataStoreType.Postgres)
			{
				PostgreSQLConfiguration postgres = PostgreSQLConfiguration.Standard.ConnectionString(connection);
				Configuration.Database(postgres);
			}
			else if (dataStoreType == DataStoreType.Sqlite)
			{
				SQLiteConfiguration sqlLite = SQLiteConfiguration.Standard.ConnectionString(connection);
				Configuration.Database(sqlLite);
			}
			else if (dataStoreType == DataStoreType.SqlServer2008)
			{
				MsSqlConfiguration msSql = MsSqlConfiguration.MsSql2008.ConnectionString(connection);
				Configuration.Database(msSql);
			}
			else if (dataStoreType == DataStoreType.SqlServerCe)
			{
				MsSqlCeConfiguration msSqlCe = MsSqlCeConfiguration.Standard.ConnectionString(connection);
				msSqlCe.Dialect("NHibernate.Dialect.MsSqlCe40Dialect, NHibernate"); // fluent uses SQL CE 3 which is wrong
				Configuration.Database(msSqlCe);
			}
			else
			{
				MsSqlConfiguration msSql = MsSqlConfiguration.MsSql2005.ConnectionString(connection);
				Configuration.Database(msSql);
			}
		}

		public void Dispose()
		{
			if (SessionFactory != null && Session != null)
			{
				Session.Dispose();
			}
		}
	}
}
