using System.Collections.Generic;
using Roadkill.Core.Database.Repositories;

namespace Roadkill.Core.Database
{
	public interface IRepositoryFactory
	{
		ISettingsRepository GetSettingsRepository(string databaseProviderName, string connectionString);
		IUserRepository GetUserRepository(string databaseProviderName, string connectionString);
		IPageRepository GetPageRepository(string databaseProviderName, string connectionString);

		IEnumerable<RepositoryInfo> ListAll();
	}
}