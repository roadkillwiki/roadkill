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

namespace Roadkill.Core
{
	public class NHibernateRepository
	{
		public ISessionFactory SessionFactory { get; private set; }
		public FluentConfiguration Configuration { get; private set; }

		public static NHibernateRepository Current
		{
			get
			{
				return Nested.Current;
			}
		}

		class Nested
		{
			static Nested()
			{
			}
			internal static readonly NHibernateRepository Current = new NHibernateRepository();
		}

		public void Configure(string connection, bool createSchema, bool enableL2Cache)
		{
			MsSqlConfiguration msSql = MsSqlConfiguration.MsSql2008.ConnectionString(connection);

			if (enableL2Cache)
				msSql = msSql.Cache(c => c.ProviderClass<HashtableCacheProvider>().UseQueryCache());

			Configuration = Fluently.Configure();
			Configuration.Database(msSql);

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
	}
}
