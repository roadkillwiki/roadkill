namespace Roadkill.Core.Database
{
	public class SupportedDatabases
	{
		public static readonly RepositoryInfo MongoDB = new RepositoryInfo("MongoDB", "MongoDB - A MongoDB server, using the official MongoDB driver.");
		public static readonly RepositoryInfo MySQL = new RepositoryInfo("MySQL", "MySQL");
		public static readonly RepositoryInfo Postgres = new RepositoryInfo("Postgres", "Postgres - A Postgres 9 or later database.");
		public static readonly RepositoryInfo SqlServer2008 = new RepositoryInfo("SqlServer2008", "Sql Server - a SqlServer 2008 or later database.");
	}
}