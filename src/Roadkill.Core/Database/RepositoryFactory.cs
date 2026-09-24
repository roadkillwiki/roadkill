using System;
using System.Collections.Generic;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Database.Repositories.Dapper;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// Creates the repositories for the configured database: Dapper for SQL Server and Postgres, the official driver for MongoDB.
	/// </summary>
	public class RepositoryFactory : IRepositoryFactory
	{
		public ISettingsRepository GetSettingsRepository(string databaseProviderName, string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
				return null;

			if (databaseProviderName == SupportedDatabases.MongoDB)
				return new MongoDBSettingsRepository(connectionString);

			return new DapperSettingsRepository(CreateConnectionFactory(databaseProviderName, connectionString));
		}

		public IUserRepository GetUserRepository(string databaseProviderName, string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
				return null;

			if (databaseProviderName == SupportedDatabases.MongoDB)
				return new MongoDBUserRepository(connectionString);

			return new DapperUserRepository(CreateConnectionFactory(databaseProviderName, connectionString));
		}

		public IPageRepository GetPageRepository(string databaseProviderName, string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
				return null;

			if (databaseProviderName == SupportedDatabases.MongoDB)
				return new MongoDBPageRepository(connectionString);

			return new DapperPageRepository(CreateConnectionFactory(databaseProviderName, connectionString));
		}

		public IInstallerRepository GetInstallerRepository(string databaseProviderName, string connectionString)
		{
			if (databaseProviderName == SupportedDatabases.MongoDB)
				return new MongoDbInstallerRepository(connectionString);

			return new DapperInstallerRepository(CreateConnectionFactory(databaseProviderName, connectionString), CreateSchema(databaseProviderName));
		}

		public IEnumerable<RepositoryInfo> ListAll()
		{
			return new List<RepositoryInfo>()
			{
				SupportedDatabases.MongoDB,
				SupportedDatabases.Postgres,
				SupportedDatabases.SqlServer2008
			};
		}

		/// <summary>
		/// Creates a <see cref="IDbConnectionFactory"/> for the SQL based database providers.
		/// </summary>
		public static IDbConnectionFactory CreateConnectionFactory(string databaseProviderName, string connectionString)
		{
			if (databaseProviderName == SupportedDatabases.Postgres)
				return new PostgresConnectionFactory(connectionString);

			if (string.IsNullOrEmpty(databaseProviderName) || databaseProviderName == SupportedDatabases.SqlServer2008)
				return new SqlConnectionFactory(connectionString);

			throw new DatabaseException(null, "The database provider '{0}' is not supported.", databaseProviderName);
		}

		private static SchemaBase CreateSchema(string databaseProviderName)
		{
			if (databaseProviderName == SupportedDatabases.Postgres)
				return new PostgresSchema();

			return new SqlServerSchema();
		}
	}
}
