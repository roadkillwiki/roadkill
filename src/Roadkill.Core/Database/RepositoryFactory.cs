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
		private readonly bool _isInvalidState;

		public LightSpeedContext Context { get; set; }
		internal Func<LightSpeedContext, IUnitOfWork> UnitOfWorkFunc { get; set; }

		public RepositoryFactory()
		{
		}

		public RepositoryFactory(string databaseProviderName, string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				_isInvalidState = true;
				return;
			}

			if (databaseProviderName == SupportedDatabases.MongoDB)
				return;

			SetupLightSpeed(databaseProviderName, connectionString);
		}

		private void SetupLightSpeed(string databaseProviderName, string connectionString)
		{
			DataProvider provider = DataProvider.SqlServer2008;

			if (databaseProviderName == SupportedDatabases.MySQL)
			{
				provider = DataProvider.MySql5;
			}
			else if (databaseProviderName == SupportedDatabases.Postgres)
			{
				provider = DataProvider.PostgreSql9;
			}

			Context = new LightSpeedContext();
			Context.Cache = new CacheBroker(new DefaultCache());
			Context.ConnectionString = connectionString;
			Context.DataProvider = provider;
			Context.IdentityMethod = IdentityMethod.GuidComb;
			Context.CascadeDeletes = true;

			UnitOfWorkFunc = context => LocatorStartup.Locator.GetInstance<IUnitOfWork>();
		}

		private void EnsureValidState()
		{
			if (_isInvalidState)
				throw new DatabaseException("The database connection string is empty", null);
		}

		public void EnableVerboseLogging()
		{
			Context.VerboseLogging = true;
			Context.Logger = new DatabaseLogger();
		}

		public ISettingsRepository GetSettingsRepository(string databaseProviderName, string connectionString)
		{
			EnsureValidState();

			if (databaseProviderName == SupportedDatabases.MongoDB)
			{
				return new MongoDBSettingsRepository(connectionString);
			}
			else
			{
				IUnitOfWork unitOfWork = UnitOfWorkFunc(Context);
				return new LightSpeedSettingsRepository(unitOfWork);
			}
		}

		public IUserRepository GetUserRepository(string databaseProviderName, string connectionString)
		{
			EnsureValidState();

			if (databaseProviderName == SupportedDatabases.MongoDB)
			{
				return new MongoDBUserRepository(connectionString);
			}
			else
			{
				IUnitOfWork unitOfWork = UnitOfWorkFunc(Context);
				return new LightSpeedUserRepository(unitOfWork);
			}
		}

		public IPageRepository GetPageRepository(string databaseProviderName, string connectionString)
		{
			EnsureValidState();

			if (databaseProviderName == SupportedDatabases.MongoDB)
			{
				return new MongoDBPageRepository(connectionString);
			}
			else
			{
				IUnitOfWork unitOfWork = UnitOfWorkFunc(Context);
				return new LightSpeedPageRepository(unitOfWork);
			}
		}

		public IInstallerRepository GetInstallerRepository(string databaseProviderName, string connectionString)
		{
			EnsureValidState();

			if (databaseProviderName == SupportedDatabases.MongoDB)
			{
				return new MongoDbInstallerRepository(connectionString);
			}
			else if (databaseProviderName == SupportedDatabases.MySQL)
			{
				return new LightSpeedInstallerRepository(DataProvider.MySql5, new MySqlSchema(), connectionString);
			}
			else if (databaseProviderName == SupportedDatabases.Postgres)
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
				SupportedDatabases.MongoDB,
				SupportedDatabases.MySQL,
				SupportedDatabases.Postgres,
				SupportedDatabases.SqlServer2008
			};
		}
	}
}