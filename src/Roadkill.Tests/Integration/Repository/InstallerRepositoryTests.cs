using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Integration")]
	public abstract class InstallerRepositoryTests
	{
		protected abstract string InvalidConnectionString { get; }
		protected abstract string ConnectionString { get; }

		protected abstract IInstallerRepository GetRepository(string connectionString);
		protected abstract void Clearup();
		protected abstract void CheckDatabaseProcessIsRunning();
		protected abstract bool AllTablesAreEmpty();

		[SetUp]
		public void Setup()
		{
			// Setup the repository
			Clearup();
		}

		[Test]
		public void install_should_create_and_clear_all_tables()
		{
			// Arrange
			var repository = GetRepository(ConnectionString);

			// Act
			repository.Install();

			// Assert
			Assert.True(AllTablesAreEmpty());
		}

		[Test]
		public void install_should_throw_databaseexception_with_invalid_connection_string()
		{
			// Arrange 
			var repository = GetRepository(InvalidConnectionString);

			// Act Assert
			Assert.Throws<DatabaseException>(() => repository.Install());
		}

		[Test]
		public void testconnection_should_succeed_with_valid_connection_string()
		{
			// Arrange 
			var repository = GetRepository(ConnectionString);

			// Act + Assert (no exception)
			repository.TestConnection();
		}

		[Test]
		public void testconnection_should_throw_databaseexception_with_invalid_connection_string()
		{
			// Arrange 
			var repository = GetRepository(InvalidConnectionString);

			// Act Assert
			Assert.Throws<DatabaseException>(() => repository.TestConnection());
		}
	}
}
