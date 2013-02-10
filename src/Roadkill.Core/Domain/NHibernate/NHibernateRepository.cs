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

namespace Roadkill.Core
{
	/// <summary>
	/// A fluent NHibernate-based repository.
	/// </summary>
	public class NHibernateRepository : IRepository
	{
		private IConfigurationContainer _configuration;

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
		public virtual ISessionFactory SessionFactory { get; protected set; }

		/// <summary>
		/// The current Fluent NHibernate <see cref="FluentConfiguration"/> object that represents the current NHibernate configuration.
		/// </summary>
		public virtual FluentConfiguration Configuration { get; protected set; }

		public NHibernateRepository(IConfigurationContainer configuration)
		{
			_configuration = configuration;

			if (configuration.ApplicationSettings.Installed)
			{
				Configure(configuration.ApplicationSettings.DataStoreType,
						  configuration.ApplicationSettings.ConnectionString,
						  false,
						  configuration.ApplicationSettings.CacheEnabled);
			}
		}

		/// <summary>
		/// Initializes and configures NHibernate using the connection string with Fluent NHibernate.
		/// </summary>
		/// <param name="datastoreType">The database used.</param>
		/// <param name="connection">The connection string to configure with.</param>
		/// <param name="createSchema">if set to <c>true</c> the database schema is created automatically.</param>
		/// <param name="enableL2Cache">if set to <c>true</c> NHibernate L2 caching is enabled for all domain objects.</param>
		/// <remarks>
		/// Microsoft SQL Server CE: http://www.microsoft.com/downloads/en/details.aspx?FamilyID=033cfb76-5382-44fb-bc7e-b3c8174832e2
		/// </remarks>
		public virtual void Configure(DataStoreType dataStoreType, string connection, bool createSchema, bool enableL2Cache)
		{
			NHibernateConfig config = new NHibernateConfig();
			Configuration = Fluently.Configure(config);
			Configuration.Mappings(m => m.FluentMappings.AddFromAssemblyOf<Page>());

			if (createSchema)
				Configuration.ExposeConfiguration(c => new SchemaExport(c).Execute(false, true, false));

			// Only configure the Databasetype if it's not already in the config file
			if (!config.Properties.ContainsKey("connection.driver_class"))
			{
				SetDatabase(dataStoreType, connection);
			}

			if (!config.Properties.ContainsKey("connection.connection_string_name"))
			{
				config.SetProperty("connection.connection_string_name", _configuration.ApplicationSettings.ConnectionStringName);
			}

			// Only configure the caching if it's not already in the config file
			if (!config.Properties.ContainsKey("cache.use_second_level_cache"))
			{
				if (enableL2Cache)
				{
					Configuration.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache().UseSecondLevelCache());
				}
			}

			try
			{
				SessionFactory = Configuration.BuildSessionFactory();
			}
			catch (Exception e)
			{
				throw e;
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

		/// <summary>
		/// Provides a LINQ-to-NHibernate <see cref="IQueryable{T}"/> object to query with.
		/// Session disposal is the responsibility of the caller.
		/// </summary>
		/// <typeparam name="T">The domain type to query against.</typeparam>
		/// <returns><see cref="IQueryable{T}"/> for LINQ-to-NHibernate LINQ queries.</returns>
		public virtual IQueryable<T> Queryable<T>() where T : DataStoreEntity
		{
			IQueryable<T> queryable = SessionFactory.OpenSession().Query<T>();
			queryable = queryable.Cacheable<T>();

			return queryable;
		}

		/// <summary>
		/// Deletes the object from the database.
		/// </summary>
		/// <param name="obj">The object to delete.</param>
		public virtual void Delete<T>(T obj) where T : DataStoreEntity
		{
			ISession session = SessionFactory.OpenSession();
			using (session.BeginTransaction())
			{
				session.Delete(obj);
				session.Transaction.Commit();
			}
		}

		/// <summary>
		/// Deletes alls objects from the database.
		/// </summary>
		public virtual void DeleteAll<T>() where T : DataStoreEntity
		{
			string className = typeof(T).FullName;
			ISession session = SessionFactory.OpenSession();
			using (session.BeginTransaction()) // 2.1 uses transactions by default
			{
				// TODO: use ClassExtractor for a more intelligent way
				session.CreateQuery(string.Format("DELETE {0} o", className)).ExecuteUpdate();
				session.Transaction.Commit();
			}
		}

		/// <summary>
		/// Inserts or updates the object depending on whether it exists in the database.
		/// </summary>
		/// <param name="obj">The object to insert/update.</param>
		public virtual void SaveOrUpdate<T>(T obj) where T : DataStoreEntity
		{
			ISession session = SessionFactory.OpenSession();
			using (session.BeginTransaction())
			{
				session.SaveOrUpdate(obj);
				session.Transaction.Commit();
			}
		}

		/// <summary>
		/// Runs a query that does not select any rows.
		/// </summary>
		/// <param name="sql">The sql query to run</param>
		/// <returns>The number of rows affected.</returns>
		public virtual int ExecuteNonQuery(string sql)
		{
			using (ISession session = SessionFactory.OpenSession())
			{
				ISQLQuery query = session.CreateSQLQuery(sql);
				return query.ExecuteUpdate();
			}
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			PageContent latest;
			if (_configuration.ApplicationSettings.DataStoreType == DataStoreType.SqlServerCe)
			{
				// Work around for an NHibernate 3.3.1 SQL CE bug with the HQL query in CurrentContent() - this is two SQL queries per page instead of one.
				using (ISession session = SessionFactory.OpenSession())
				{
					latest = session.QueryOver<PageContent>().Where(p => p.Page.Id == pageId).OrderBy(p => p.VersionNumber).Desc.Take(1).SingleOrDefault();
					latest.Page = session.Get<Page>(latest.Page.Id);
				}
			}
			else
			{
				// Fetches the parent page object via SQL as well as the PageContent, avoiding lazy loading.
				IQuery query = SessionFactory.OpenSession()
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

		public Page FindPageByTitle(string title)
		{
			try
			{
				if (string.IsNullOrEmpty(title))
					return null;

				Page page = Pages.FirstOrDefault(p => p.Title.ToLower() == title.ToLower());

				if (page == null)
					return null;
				else
					return page;
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred finding the page with title '{0}' in the database", title);
			}
		}
	}
}
