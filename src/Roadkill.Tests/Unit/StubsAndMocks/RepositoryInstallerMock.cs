using System;
using Roadkill.Core;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class RepositoryInstallerMock : IRepositoryInstaller
	{
		public bool IsConnectionValid { get; set; }
		public bool Installed { get; set; }
		public bool ThrowInstallException { get; set; }

		public void Install()
		{
			if (ThrowInstallException)
				throw new DatabaseException("Something happened", null);

			Installed = true;
		}

		public void TestConnection()
		{
			if (!IsConnectionValid)
			{
				throw new DatabaseException("RepositoryInstallerMock", null);
			}
		}
	}
}