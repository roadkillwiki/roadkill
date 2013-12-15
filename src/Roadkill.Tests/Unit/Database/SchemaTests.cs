using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Schema;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Database.Schema
{
	[TestFixture]
	[Category("Unit")]
	[Description("Checks the various SchemaBase implementations get their embedded resource SQL scripts.")]
	public class SchemaTests
	{
		/// <summary>
		/// TestCase needs a constant, so here they are
		/// </summary>
		public class DbTypes
		{
			public const string Postgres = "Postgres";
			public const string MySql = "MySql";
			public const string SqlServerCe = "SqlServerCe";
			public const string SqlServer2012 = "SqlServer2012";
			public const string Sqlite = "Sqlite";
		}

		[Test]
		[TestCase(DbTypes.Postgres)]
		[TestCase(DbTypes.MySql)]
		[TestCase(DbTypes.Sqlite)]
		[TestCase(DbTypes.SqlServerCe)]
		[TestCase(DbTypes.SqlServer2012)]
		public void Schema_Create_Should_Contain_SQL(string dbType)
		{
			// Arrange
			SchemaBase schema = DataStoreType.ByName(dbType).Schema;
			DbCommandStub dbCommand = new DbCommandStub();

			// Act
			schema.Create(dbCommand);

			// Assert
			Assert.That(dbCommand.CommandText, Is.StringContaining("CREATE TABLE"));
		}

		[Test]
		[TestCase(DbTypes.Postgres)]
		[TestCase(DbTypes.MySql)]
		[TestCase(DbTypes.Sqlite)]
		[TestCase(DbTypes.SqlServerCe)]
		[TestCase(DbTypes.SqlServer2012)]
		public void Schema_Drop_Should_Contain_SQL(string dbType)
		{
			// Arrange
			SchemaBase schema = DataStoreType.ByName(dbType).Schema;
			DbCommandStub dbCommand = new DbCommandStub();

			// Act
			schema.Drop(dbCommand);

			// Assert
			Assert.That(dbCommand.CommandText, Is.StringContaining("DROP"));
		}

		[Test]
		[TestCase(DbTypes.Postgres)]
		[TestCase(DbTypes.MySql)]
		[TestCase(DbTypes.Sqlite)]
		[TestCase(DbTypes.SqlServerCe)]
		[TestCase(DbTypes.SqlServer2012)]
		public void Schema_Upgrade_Should_Contain_SQL(string dbType)
		{
			// Arrange
			SchemaBase schema = DataStoreType.ByName(dbType).Schema;
			DbCommandStub dbCommand = new DbCommandStub();
			
			// Act
			schema.Upgrade(dbCommand);

			// Assert
			Assert.That(dbCommand.CommandText, Is.StringContaining("CREATE"));
		}
	}
}
