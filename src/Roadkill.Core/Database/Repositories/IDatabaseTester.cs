using Mindscape.LightSpeed;

namespace Roadkill.Core.Database
{
	public interface IDatabaseTester
	{
		/// <summary>
		/// Tests a connection string for the database. This method should throw a <see cref="DatabaseException"/> if a
		/// database exception occurred.
		/// </summary>
		/// <exception cref="DatabaseException">Can't connect to the server, or an invalid connection string</exception>
		void TestConnection(string databaseProvider, string connectionString);
	}
}