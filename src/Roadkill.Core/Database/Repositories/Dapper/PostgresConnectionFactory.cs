using System.Data;
using Npgsql;

namespace Roadkill.Core.Database.Repositories.Dapper
{
	public class PostgresConnectionFactory : IDbConnectionFactory
	{
		private readonly string _connectionString;

		public PostgresConnectionFactory(string connectionString)
		{
			_connectionString = connectionString;
		}

		public IDbConnection CreateConnection()
		{
			return new NpgsqlConnection(_connectionString);
		}

		public string GetAutoIdentitySqlSuffix()
		{
			return "RETURNING id";
		}
	}
}