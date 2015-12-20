using System;
using System.Collections.Generic;
using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Caching;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Database.Schema;
using Roadkill.Core.DependencyResolution;

namespace Roadkill.Core.Database
{
	public class RepositoryFactory : IRepositoryFactory
	{
		public static readonly RepositoryInfo MongoDB = new RepositoryInfo("MongoDB", "MongoDB - A MongoDB server, using the official MongoDB driver.");
		public static readonly RepositoryInfo MySQL = new RepositoryInfo("MySQL", "MySQL");
		public static readonly RepositoryInfo Postgres = new RepositoryInfo("Postgres", "Postgres - A Postgres 9 or later database.");
		public static readonly RepositoryInfo SqlServer2008 = new RepositoryInfo("SqlServer2008", "Sql Server - a SqlServer 2008 or later database.");

		public LightSpeedContext Context { get; set; }

		public RepositoryFactory()
		{
		}

		public RepositoryFactory(string databaseProviderName, string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
				throw new DatabaseException("The database connection string is empty", null);

			if (databaseProviderName == MongoDB)
				return;

			// LightspeedSetup
			DataProvider provider = DataProvider.SqlServer2008;

			if (databaseProviderName == MySQL)
			{
				provider = DataProvider.MySql5;
			}
			else if (databaseProviderName == Postgres)
			{
				provider = DataProvider.PostgreSql9;
			}
			
			Context = new LightSpeedContext();
			Context.Cache = new CacheBroker(new DefaultCache());
			Context.ConnectionString = connectionString;
			Context.DataProvider = provider;
			Context.IdentityMethod = IdentityMethod.GuidComb;
			Context.CascadeDeletes = true;
		}

		public void EnableVerboseLogging()
		{
			Context.VerboseLogging = true;
			Context.Logger = new DatabaseLogger();
		}

		public ISettingsRepository GetSettingsRepository(string databaseProviderName, string connectionString)
		{
			if (databaseProviderName == MongoDB)
			{
				return new MongoDBSettingsRepository(connectionString);
			}
			else
			{
				return new LightSpeedSettingsRepository(LocatorStartup.Locator.GetInstance<IUnitOfWork>());
			}
		}

		public IUserRepository GetUserRepository(string databaseProviderName, string connectionString)
		{
			if (databaseProviderName == MongoDB)
			{
				return new MongoDBUserRepository(connectionString);
			}
			else
			{
				return new LightSpeedUserRepository(LocatorStartup.Locator.GetInstance<IUnitOfWork>());
			}
		}

		public IPageRepository GetPageRepository(string databaseProviderName, string connectionString)
		{
			if (databaseProviderName == MongoDB)
			{
				return new MongoDBPageRepository(connectionString);
			}
			else
			{
				return new LightSpeedPageRepository(LocatorStartup.Locator.GetInstance<IUnitOfWork>());
			}
		}

		public IInstallerRepository GetInstallerRepository(string databaseProviderName, string connectionString)
		{
			if (databaseProviderName == MongoDB)
			{
				return new MongoDbInstallerRepository(connectionString);
			}
			else if (databaseProviderName == MySQL)
			{
				return new LightSpeedInstallerRepository(DataProvider.MySql5, new MySqlSchema(), connectionString);
			}
			else if (databaseProviderName == Postgres)
			{
				return new LightSpeedInstallerRepository(DataProvider.PostgreSql9, new PostgresSchema(), connectionString);
			}
			else
			{
				return new LightSpeedInstallerRepository(DataProvider.SqlServer2008, new SqlServerSchema(), connectionString);
			}
		}

		public IEnumerable<RepositoryInfo> ListAll()
		{
			return new List<RepositoryInfo>()
			{
				MongoDB,
				MySQL,
				Postgres,
				SqlServer2008
			};
		}
	}
}