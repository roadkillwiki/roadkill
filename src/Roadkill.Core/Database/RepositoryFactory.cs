using System.Collections.Generic;
using Mindscape.LightSpeed;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Core.Database
{
	public class RepositoryFactory : IRepositoryFactory
	{
		public static readonly RepositoryInfo MongoDB = new RepositoryInfo("MongoDB", "MongoDB - A MongoDB server, using the official MongoDB driver.");
		public static readonly RepositoryInfo MySQL = new RepositoryInfo("MySQL", "MySQL");
		public static readonly RepositoryInfo Postgres = new RepositoryInfo("Postgres", "Postgres - A Postgres 9 or later database.");
		public static readonly RepositoryInfo SqlServer2008 = new RepositoryInfo("SqlServer2008", "Sql Server - a SqlServer 2008 or later database.");

		public IRepository GetRepository(string databaseProviderName, string connectionString)
		{
			if (databaseProviderName == MongoDB)
			{
				return new MongoDBRepository(connectionString);
			}
			else if (databaseProviderName == MySQL)
			{
				return new LightSpeedRepository(DataProvider.MySql5, connectionString);
			}
			else if (databaseProviderName == Postgres)
			{
				return new LightSpeedRepository(DataProvider.PostgreSql9, connectionString);
			}
			else
			{
				return new LightSpeedRepository(DataProvider.SqlServer2008, connectionString);
			}
		}

		public IInstallerRepository GetRepositoryInstaller(string databaseProviderName, string connectionString)
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