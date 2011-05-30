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
		public ISessionFactory SessionFactory { get; private set; }

		/// <summary>
		/// The current Fluent NHibernate <see cref="FluentConfiguration"/> object that represents the current NHibernate configuration.
		/// </summary>
		public FluentConfiguration Configuration { get; private set; }

		/// <summary>
		/// Gets the current <see cref="NHibernateRepository"/> for the application.
		/// </summary>
		public static NHibernateRepository Current
		{
			get
			{
				return Nested.Current;
			}
		}

		/// <summary>
		/// Singleton for Current
		/// </summary>
		class Nested
		{
			internal static readonly NHibernateRepository Current = new NHibernateRepository();

			static Nested()
			{
			}	
		}

		/// <summary>
		/// Initializes and configures NHibernate using the connection string with Fluent NHibernate.
		/// </summary>
		/// <param name="driver">The database used.</param>
		/// <param name="connection">The connection string to configure with.</param>
		/// <param name="createSchema">if set to <c>true</c> the database schema is created automatically.</param>
		/// <param name="enableL2Cache">if set to <c>true</c> NHibernate L2 caching is enabled for all domain objects.</param>
		public void Configure(DatabaseType databaseType,string connection, bool createSchema, bool enableL2Cache)
		{
			Configuration = Fluently.Configure();

			// This is the long way of doing the configuration, the alternative would be to us 
			// something like PersistenceConfiguration<T, TBuilder> but that ends up doing what these classes are doing already.
			switch (databaseType)
			{
				case DatabaseType.DB2:
					{
						DB2Configuration db2 = DB2Configuration.Standard.ConnectionString(connection);
						if (enableL2Cache)
							db2 = db2.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache());

						Configuration.Database(db2);
					}
					break;

				case DatabaseType.Firebird:
					{
						FirebirdConfiguration fireBird = new FirebirdConfiguration();
						fireBird.ConnectionString(connection);
						if (enableL2Cache)
							fireBird = fireBird.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache());

						Configuration.Database(fireBird);
					}
					break;
				
				case DatabaseType.MySQL:
					{
						MySQLConfiguration mySql = MySQLConfiguration.Standard.ConnectionString(connection);
						if (enableL2Cache)
							mySql = mySql.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache());

						Configuration.Database(mySql);
					}
					break;

				case DatabaseType.Postgres:
					{
						PostgreSQLConfiguration postgres = PostgreSQLConfiguration.Standard.ConnectionString(connection);
						if (enableL2Cache)
							postgres = postgres.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache());

						Configuration.Database(postgres);
					}
					break;

				case DatabaseType.Sqlite:
					{
						FluentNHibernate.Cfg.Db.
						SQLiteConfiguration sqlLite = SQLiteConfiguration.Standard.ConnectionString(connection);
						if (enableL2Cache)
							sqlLite = sqlLite.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache());

						Configuration.Database(sqlLite);
					}
					break;

				case DatabaseType.SqlServer2008:
					{
						MsSqlConfiguration msSql = MsSqlConfiguration.MsSql2008.ConnectionString(connection);
						if (enableL2Cache)
							msSql = msSql.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache());

						Configuration.Database(msSql);
					}
					break;
			
				case DatabaseType.SqlServer2005:
				default:
					{
						MsSqlConfiguration msSql = MsSqlConfiguration.MsSql2005.ConnectionString(connection);
						if (enableL2Cache)
							msSql = msSql.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache());

						Configuration.Database(msSql);
					}
					break;
			}


			Configuration.Mappings(m => m.FluentMappings.AddFromAssemblyOf<Page>());

			if (createSchema)
				Configuration.ExposeConfiguration(config => new SchemaExport(config).Execute(false, true, false));

			try
			{
				Configuration.BuildConfiguration();
				SessionFactory = Configuration.BuildSessionFactory();
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		/// <summary>
		/// Provides a LINQ-to-NHibernate <see cref="IQueryable`T"/> object to query with.
		/// Session disposal is the responsibility of the caller.
		/// </summary>
		/// <typeparam name="T">The domain type to query against.</typeparam>
		/// <returns><see cref="IQueryable`T"/> for LINQ-to-NHibernate LINQ queries.</returns>
		public IQueryable<T> Queryable<T>()
		{
			IQueryable<T> queryable = SessionFactory.OpenSession().Query<T>();
			queryable = queryable.Cacheable<T>();

			return queryable;
		}

		/// <summary>
		/// Deletes the object from the database.
		/// </summary>
		/// <param name="obj">The object to delete.</param>
		public void Delete<T>(T obj) where T : class
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
		public void DeleteAll<T>() where T : class
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
		public void SaveOrUpdate<T>(T obj) where T : class
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
		public int ExecuteNonQuery(string sql)
		{
			using (ISession session = SessionFactory.OpenSession())
			{
				ISQLQuery query = session.CreateSQLQuery(sql);
				return query.ExecuteUpdate();
			}
		}
	}
}
