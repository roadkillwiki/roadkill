using System.Data;

namespace Roadkill.Core.Database.Repositories.Dapper
{
	public interface IDbConnectionFactory
	{
		IDbConnection CreateConnection();
		string GetAutoIdentitySqlSuffix();
	}
}