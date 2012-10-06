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

namespace Roadkill.Core
{
	/// <summary>
	/// A repository class for all NHibernate actions.
	/// </summary>
	public class NHibernateRepository
	{
		/// <summary>
		/// The current NHibernate <see cref="ISessionFactory"/>. This is created once, the first the NHibernateRepository is used.
		/// </summary>
		public virtual ISessionFactory SessionFactory { get; protected set; }

		/// <summary>
		/// The current Fluent NHibernate <see cref="FluentConfiguration"/> object that represents the current NHibernate configuration.
		/// </summary>
		public virtual FluentConfiguration Configuration { get; protected set; }

		/// <summary>
		/// Initializes and configures NHibernate using the connection string with Fluent NHibernate.
		/// </summary>
		/// <param name="driver">The database used.</param>
		/// <param name="connection">The connection string to configure with.</param>
		/// <param name="createSchema">if set to <c>true</c> the database schema is created automatically.</param>
		/// <param name="enableL2Cache">if set to <c>true</c> NHibernate L2 caching is enabled for all domain objects.</param>
		/// <remarks>
		/// Microsoft SQL Server CE: http://www.microsoft.com/downloads/en/details.aspx?FamilyID=033cfb76-5382-44fb-bc7e-b3c8174832e2
		/// </remarks>
		public virtual void Configure(DatabaseType databaseType,string connection, bool createSchema, bool enableL2Cache)
		{
			NHibernateConfig config = new NHibernateConfig();
			Configuration = Fluently.Configure(config);
			Configuration.Mappings(m => m.FluentMappings.AddFromAssemblyOf<Page>());

			if (createSchema)
				Configuration.ExposeConfiguration(c => new SchemaExport(c).Execute(false, true, false));

			// Only configure the Databasetype if it's not already in the config file
			if (!config.Properties.ContainsKey("connection.driver_class"))
			{
				SetDatabase(databaseType, connection);
			}

			if (!config.Properties.ContainsKey("connection.connection_string_name"))
			{
				config.SetProperty("connection.connection_string_name", RoadkillSection.Current.ConnectionStringName);
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

		private void SetDatabase(DatabaseType databaseType, string connection)
		{
			switch (databaseType)
			{
				case DatabaseType.DB2:
					{
						DB2Configuration db2 = DB2Configuration.Standard.ConnectionString(connection);
						Configuration.Database(db2);
					}
					break;

				case DatabaseType.Firebird:
					{
						FirebirdConfiguration fireBird = new FirebirdConfiguration();
						fireBird.ConnectionString(connection);
						Configuration.Database(fireBird);
					}
					break;

				case DatabaseType.MySQL:
					{
						MySQLConfiguration mySql = MySQLConfiguration.Standard.ConnectionString(connection);
						Configuration.Database(mySql);
					}
					break;

				case DatabaseType.Postgres:
					{
						PostgreSQLConfiguration postgres = PostgreSQLConfiguration.Standard.ConnectionString(connection);
						Configuration.Database(postgres);
					}
					break;

				case DatabaseType.Sqlite:
					{
						SQLiteConfiguration sqlLite = SQLiteConfiguration.Standard.ConnectionString(connection);
						Configuration.Database(sqlLite);
					}
					break;

				case DatabaseType.SqlServer2008:
					{
						MsSqlConfiguration msSql = MsSqlConfiguration.MsSql2008.ConnectionString(connection);
						Configuration.Database(msSql);
					}
					break;

				case DatabaseType.SqlServerCe:
					{
						MsSqlCeConfiguration msSqlCe = MsSqlCeConfiguration.Standard.ConnectionString(connection);
						msSqlCe.Dialect("NHibernate.Dialect.MsSqlCe40Dialect, NHibernate"); // fluent uses SQL CE 3 which is wrong
						Configuration.Database(msSqlCe);
					}
					break;

				case DatabaseType.SqlServer2005:
				default:
					{
						MsSqlConfiguration msSql = MsSqlConfiguration.MsSql2005.ConnectionString(connection);
						Configuration.Database(msSql);
					}
					break;
			}
		}

		/// <summary>
		/// Provides a LINQ-to-NHibernate <see cref="IQueryable`T"/> object to query with.
		/// Session disposal is the responsibility of the caller.
		/// </summary>
		/// <typeparam name="T">The domain type to query against.</typeparam>
		/// <returns><see cref="IQueryable`T"/> for LINQ-to-NHibernate LINQ queries.</returns>
		public virtual IQueryable<T> Queryable<T>()
		{
			IQueryable<T> queryable = SessionFactory.OpenSession().Query<T>();
			queryable = queryable.Cacheable<T>();

			return queryable;
		}

		/// <summary>
		/// Deletes the object from the database.
		/// </summary>
		/// <param name="obj">The object to delete.</param>
		public virtual void Delete<T>(T obj) where T : class
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
		public virtual void DeleteAll<T>() where T : class
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
		public virtual void SaveOrUpdate<T>(T obj) where T : class
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

		private static bool _initialized;

		/// <summary>
		/// Gets the current <see cref="NHibernateRepository"/> for the application.
		/// </summary>
		public static NHibernateRepository Current
		{
			get
			{
				if (!_initialized)
					Initialize(null);

				return Nested.Current;
			}
		}

		/// <summary>
		/// Re-initializes the repository's singleton instance.
		/// </summary>
		/// <param name="repository">The repository type to re-initialize with.</param>
		public static void Initialize(NHibernateRepository repository)
		{
			Nested.Initialize(repository);
			_initialized = true;
		}

		/// <summary>
		/// Singleton for Current
		/// </summary>
		class Nested
		{
			internal static NHibernateRepository Current;

			public static void Initialize(NHibernateRepository repository)
			{
				if (repository == null)
					Current = new NHibernateRepository();
				else
					Current = repository;
			}
		}
	}
}
