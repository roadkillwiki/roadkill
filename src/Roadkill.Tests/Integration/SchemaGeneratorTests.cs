using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Integration
{
	[TestFixture]
	[Category("Integration")]
	[Description("These don't test the db schemas post-install, just if the installer works")]
	public class SchemaGeneratorTests
	{
		[Test]
		public void Create()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.Postgres;
			string connectionString = @"Data Source=roadkill-integrationtests.sqlite;";
			SchemaGenerator generator = new SchemaGenerator(dataStoreType, connectionString);

			// Act
			Console.Write(generator.Create());

			// Assert
		}

		[Test]
		public void Drop()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.Sqlite;
			string connectionString = @"Data Source=roadkill-integrationtests.sqlite;";
			SchemaGenerator generator = new SchemaGenerator(dataStoreType, connectionString);

			// Act
			Console.Write(generator.Drop());

			// Assert
		}

		[Test]
		public void Version16_Upgrade()
		{
			
		}

		[Test]
		public void Version16_To_Version1_Downgrade()
		{
			
		}
	}
}
