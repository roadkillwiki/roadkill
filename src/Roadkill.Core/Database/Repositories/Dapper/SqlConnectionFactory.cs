using System.Data;
using Microsoft.Data.SqlClient;

namespace Roadkill.Core.Database.Repositories.Dapper
{
	public class SqlConnectionFactory : IDbConnectionFactory
	{
		private readonly string _connectionString;

		public SqlConnectionFactory(string connectionString)
		{
			_connectionString = connectionString;
		}

		public IDbConnection CreateConnection()
		{
			return new SqlConnection(_connectionString);
		}

		public string GetAutoIdentitySqlSuffix()
		{
			return ";select SCOPE_IDENTITY()";
		}
	}
}