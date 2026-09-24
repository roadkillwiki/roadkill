using NUnit.Framework;
using Roadkill.Core.Database.Schema;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Database
{
	[TestFixture]
	[Category("Unit")]
	[Description("Checks the various SchemaBase implementations get their embedded resource SQL scripts.")]
	public class SchemaTests
	{
		public class SchemaLookup
		{
			public static SchemaBase FromName(string name)
			{
				switch (name)
				{
					case "Postgres":
						return new PostgresSchema();
					case "SqlServer":
					default:
						return new SqlServerSchema();
				}
			}
		}

		[Test]
		[TestCase("Postgres")]
		[TestCase("SqlServer")]
		public void Schema_Create_Should_Contain_SQL(string dbType)
		{
			// Arrange
			SchemaBase schema = SchemaLookup.FromName(dbType);
			DbCommandStub dbCommand = new DbCommandStub();

			// Act
			schema.Create(dbCommand);

			// Assert
			Assert.That(dbCommand.CommandText, Does.Contain("CREATE TABLE"));
		}

        [Test]
		[TestCase("Postgres")]
		[TestCase("SqlServer")]
		public void Schema_Drop_Should_Contain_SQL(string dbType)
		{
			// Arrange
			SchemaBase schema = SchemaLookup.FromName(dbType);
			DbCommandStub dbCommand = new DbCommandStub();

			// Act
			schema.Drop(dbCommand);

			// Assert
			Assert.That(dbCommand.CommandText, Does.Contain("DROP"));
		}
	}
}
