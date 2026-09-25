using System;

namespace Roadkill.Core.Database
{
	public class SupportedDatabases
	{
		public static readonly RepositoryInfo MongoDB = new RepositoryInfo("MongoDB", "MongoDB - A MongoDB server, using the official MongoDB driver.");
		public static readonly RepositoryInfo Postgres = new RepositoryInfo("Postgres", "Postgres - A Postgres 9 or later database.");
		public static readonly RepositoryInfo SqlServer = new RepositoryInfo("SqlServer", "SQL Server - a SQL Server 2022 or later database.");

		/// <summary>
		/// Returns the id of the supported database for a configured database name, accepting the Roadkill 2.x names
		/// (e.g. "SqlServer2008", "SqlServer2012", "SqlAzure" are SQL Server). An empty name is SQL Server.
		/// </summary>
		public static string Normalize(string databaseName)
		{
			if (string.IsNullOrEmpty(databaseName)
				|| databaseName.StartsWith("SqlServer", StringComparison.OrdinalIgnoreCase)
				|| databaseName.StartsWith("SqlAzure", StringComparison.OrdinalIgnoreCase))
				return SqlServer.Id;

			if (databaseName.Equals(Postgres.Id, StringComparison.OrdinalIgnoreCase))
				return Postgres.Id;

			if (databaseName.Equals(MongoDB.Id, StringComparison.OrdinalIgnoreCase))
				return MongoDB.Id;

			return databaseName;
		}
	}
}
