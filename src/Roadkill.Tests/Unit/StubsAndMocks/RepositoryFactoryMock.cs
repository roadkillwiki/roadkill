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
		public RepositoryMock Repository { get; set; }
		public InstallerRepositoryMock InstallerRepository { get; set; }

		public RepositoryFactoryMock()
		{
			Repository = new RepositoryMock();
			InstallerRepository = new InstallerRepositoryMock();
		}

		public IRepository GetRepository(string databaseProviderName, string connectionString)
		{
			return Repository;
		}

		public IInstallerRepository GetRepositoryInstaller(string databaseProviderName, string connectionString)
		{
			return InstallerRepository;
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
