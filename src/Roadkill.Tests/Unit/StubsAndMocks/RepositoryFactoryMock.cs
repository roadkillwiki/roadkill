using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class RepositoryFactoryMock : IRepositoryFactory
	{
		public SettingsRepositoryMock SettingsRepository { get; set; }
		public UserRepositoryMock UserRepository { get; set; }
		public PageRepositoryMock PageRepository { get; set; }

		public InstallerRepositoryMock InstallerRepository { get; set; }

		public RepositoryFactoryMock()
		{
			PageRepository = new PageRepositoryMock();
			InstallerRepository = new InstallerRepositoryMock();
		}

		public ISettingsRepository GetSettingsRepository(string databaseProviderName, string connectionString)
		{
			return SettingsRepository;
		}

		public IUserRepository GetUserRepository(string databaseProviderName, string connectionString)
		{
			return UserRepository;
		}

		public IPageRepository GetPageRepository(string databaseProviderName, string connectionString)
		{
			return PageRepository;
		}

		public IInstallerRepository GetInstallerRepository(string databaseProviderName, string connectionString)
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
