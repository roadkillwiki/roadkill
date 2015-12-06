using System.Collections.Generic;
using Mindscape.LightSpeed;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Core.Database
{
	public class RepositoryFactory : IRepositoryFactory
	{
		public static readonly RepositoryInfo MongoDB = new RepositoryInfo("MongoDB", "A MongoDB server, using the official MongoDB driver.");
		public static readonly RepositoryInfo MySQL = new RepositoryInfo("MySQL", "A MySQL database.");
		public static readonly RepositoryInfo Postgres = new RepositoryInfo("Postgres", "A Postgres database.");
		public static readonly RepositoryInfo SqlServer2005 = new RepositoryInfo("SqlServer2005", "A SqlServer 2005 (or 2000) database.");
		public static readonly RepositoryInfo SqlServer2008 = new RepositoryInfo("SqlServer2008", "A SqlServer 2008 database.");
		public static readonly RepositoryInfo SqlServer2012 = new RepositoryInfo("SqlServer2012", "A SqlServer 2012 database.");
		public static readonly RepositoryInfo SqlServerCe = new RepositoryInfo("SqlServerCe", "A SqlServer CE 4 database.");

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

		public IRepositoryInstaller GetRepositoryInstaller(string databaseProviderName, string connectionString)
		{
			if (databaseProviderName == MongoDB)
			{
				return new MongoDBRepositoryInstaller(connectionString);
			}
			else if (databaseProviderName == MySQL)
			{
				return new LightSpeedRepositoryInstaller(DataProvider.MySql5, new MySqlSchema(), connectionString);
			}
			else if (databaseProviderName == Postgres)
			{
				return new LightSpeedRepositoryInstaller(DataProvider.PostgreSql9, new PostgresSchema(), connectionString);
			}
			else
			{
				return new LightSpeedRepositoryInstaller(DataProvider.SqlServer2008, new SqlServerSchema(), connectionString);
			}
		}

		public IEnumerable<RepositoryInfo> ListAll()
		{
			return new List<RepositoryInfo>()
			{
				new RepositoryInfo("MongoDB", "A MongoDB server, using the official MongoDB driver."),
                new RepositoryInfo("MySQL", "A MySQL database."),
				new RepositoryInfo("Postgres", "A Postgres database."),
				new RepositoryInfo("SqlServer2005", "A SqlServer 2005 (or 2000) database."),
				new RepositoryInfo("SqlServer2008", "A SqlServer 2008 database."),
				new RepositoryInfo("SqlServer2012", "A SqlServer 2012 database."),
				new RepositoryInfo("SqlServerCe", "A SqlServer CE 4 database.")
			};
		}
	}
}