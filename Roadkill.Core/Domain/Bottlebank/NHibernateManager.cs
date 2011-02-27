using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using FluentNHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using FluentNHibernate.Cfg.Db;
using System.Reflection;

namespace BottleBank
{
	public class NHibernateManager
	{
		public ISessionFactory SessionFactory { get; private set; }
		public FluentConfiguration Configuration { get; private set; }

		public static NHibernateManager Current
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
			internal static readonly NHibernateManager Current = new NHibernateManager();
		}

		/// <summary>
		/// Configures NHibernate for the application.
		/// </summary>
		/// <param name="connection">The connection string.</param>
		/// <param name="types">The FluentNHibernate <see cref="ClassMap`T"/> based types to map.</param>
		internal void Configure<T>(string connection)
		{
			Configure<T>(connection, false);
		}

		/// <summary>
		/// Configures NHibernate for the application.
		/// </summary>
		/// <param name="connection">The connection string.</param>
		/// <param name="createSchema">Whether to wipe the existing database schema and recreate it based on the NHibernate's automatic type to SQL mappings.</param>
		/// <param name="types">The FluentNHibernate <see cref="ClassMap`T"/> based types to map.</param>
		internal void Configure<T>(string connection, bool createSchema)
		{
			Configuration = Fluently.Configure();
			Configuration.Database(MsSqlConfiguration.MsSql2008.ConnectionString(connection));

			Configuration.Mappings(m => m.FluentMappings.AddFromAssemblyOf<T>());

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
	}
}
