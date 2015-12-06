using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class RepositoryFactoryMock : IRepositoryFactory
	{
		public IRepository Repository { get; set; }

		public RepositoryFactoryMock()
		{
			Repository = new RepositoryMock();
		}

		public IRepository GetRepository(string databaseProviderName, string connectionString)
		{
			return Repository;
		}

		public IRepositoryInstaller GetRepositoryInstaller(string databaseProviderName, string connectionString)
		{
			return new RepositoryInstallerMock();
		}

		public IEnumerable<RepositoryInfo> ListAll()
		{
			return new List<RepositoryInfo>()
			{
				new RepositoryInfo("id1", "desc"),
				new RepositoryInfo("id2", "desc")
			};
		}
	}
}
