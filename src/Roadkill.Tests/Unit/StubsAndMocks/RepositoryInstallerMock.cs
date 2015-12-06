using Roadkill.Core;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class RepositoryInstallerMock : IRepositoryInstaller
	{
		public bool IsConnectionValid { get; set; }

		public void Install()
		{
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