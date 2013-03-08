using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.Sqlite;
using FluentMigrator.Runner.Processors.SqlServer;

namespace Roadkill.Core.Database.Schema
{
	public class SchemaInstaller
	{
		public static void Create(DataStoreType databaseType, string connectionString)
		{
			MigrationRunner runner = CreateRunner(databaseType, connectionString);
			runner.MigrateUp();
		}

		public static void Upgrade(DataStoreType databaseType, string connectionString)
		{
			MigrationRunner runner = CreateRunner(databaseType, connectionString);
			runner.MigrateUp(16);
		}

		public static void Downgrade(DataStoreType databaseType, string connectionString)
		{
			MigrationRunner runner = CreateRunner(databaseType, connectionString);
			runner.MigrateDown(1);
		}

		public static void Drop(DataStoreType databaseType, string connectionString)
		{
			MigrationRunner runner = CreateRunner(databaseType, connectionString);
			runner.MigrateDown(0);
		}

		private static MigrationRunner CreateRunner(DataStoreType databaseType, string connectionString)
		{
			MigrationProcessorFactory factory = new SqlServer2008ProcessorFactory();

			if (databaseType == DataStoreType.SqlServerCe)
			{
				factory = new SqlServerCeProcessorFactory();
			}
			else if (databaseType == DataStoreType.SqlServer2005)
			{
				factory = new SqlServer2005ProcessorFactory();
			}
			else if (databaseType == DataStoreType.MySQL)
			{
				factory = new MySqlProcessorFactory();
			}
			else if (databaseType == DataStoreType.Sqlite)
			{
				factory = new SqliteProcessorFactory();
			}
			else if (databaseType == DataStoreType.Postgres)
			{
				factory = new PostgresProcessorFactory();
			}
			
			Announcer announcer = new TextWriterAnnouncer(s => System.Diagnostics.Debug.WriteLine(s));
			announcer.ShowSql = true;
			Assembly assembly = Assembly.GetExecutingAssembly();
			IRunnerContext migrationContext = new RunnerContext(announcer);

			var options = new ProcessorOptions { PreviewOnly = false, Timeout = 60 };
			IMigrationProcessor processor = factory.Create(connectionString, announcer, options);
			var runner = new MigrationRunner(assembly, migrationContext, processor);

			return runner;
		}
	}
}
