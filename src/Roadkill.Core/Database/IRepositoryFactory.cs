using System.Collections.Generic;

namespace Roadkill.Core.Database
{
	public interface IRepositoryFactory
	{
		IRepository GetRepository(string databaseProviderName, string connectionString);
		IInstallerRepository GetRepositoryInstaller(string databaseProviderName, string connectionString);
		IEnumerable<RepositoryInfo> ListAll();
	}
}