using System;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class InstallerRepositoryMock : IInstallerRepository
	{
		public bool Installed { get; set; }
		public bool ThrowInstallException { get; set; }

		public void AddAdminUser(string email, string username, string password)
		{
			if (ThrowInstallException)
				throw new DatabaseException("Something happened", null);
		}

		public void CreateSchema()
		{
			if (ThrowInstallException)
				throw new DatabaseException("Something happened", null);
		}

		public void SaveSettings(SiteSettings siteSettings)
		{
			if (ThrowInstallException)
				throw new DatabaseException("Something happened", null);
		}

		public void Dispose()
		{
		}
	}
}