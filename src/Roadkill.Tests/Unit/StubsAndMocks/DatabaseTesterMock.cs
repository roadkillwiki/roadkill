using Roadkill.Core;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class DatabaseTesterMock : IDatabaseTester
	{
		public bool IsConnectionValid { get; set; }

		public void TestConnection(string databaseProvider, string connectionString)
		{
			if (!IsConnectionValid)
			{
				throw new DatabaseException("InstallerRepositoryMock", null);
			}
		}
	}
}