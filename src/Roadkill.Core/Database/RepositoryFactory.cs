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
		// Hack to make sure the factory doesn't return invalid Repositories, while installing.
		private readonly bool _pendingInstallation;

		public LightSpeedContext Context { get; set; }
		internal Func<LightSpeedContext, IUnitOfWork> UnitOfWorkFunc { get; set; }

		public RepositoryFactory()
		{
		}

		public RepositoryFactory(string databaseProviderName, string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				_pendingInstallation = true;
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

		public void EnableVerboseLogging()
		{
			Context.VerboseLogging = true;
			Context.Logger = new DatabaseLogger();
		}

		public ISettingsRepository GetSettingsRepository(string databaseProviderName, string connectionString)
		{
			if (_pendingInstallation)
				return null;

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
			if (_pendingInstallation)
				return null;

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
			if (_pendingInstallation)
				return null;

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