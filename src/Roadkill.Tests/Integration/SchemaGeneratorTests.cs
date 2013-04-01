using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Tests.Integration
{
	[TestFixture]
	[Category("Integration")]
	[Description("These don't test the db schemas post-install, just if the installer works")]
	public class SchemaGeneratorTests
	{
		[Test]
		[Ignore]
		public void Create()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.SqlServerCe;
			string connectionString = @"Data Source=|DataDirectory|\roadkill-acceptancetests.sdf;";
			//IoCSetup setup = new IoCSetup();
			//setup.Run();

			//SqlServerCESchema schema = new SqlServerCESchema();
			//schema.Create();

			// Act
			//Console.Write(generator.Create());

			// Assert
		}

		[Test]
		[Ignore]
		public void Drop()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.Sqlite;
			string connectionString = @"Data Source=roadkill-integrationtests.sqlite;";
			//SchemaGenerator generator = new SchemaGenerator(dataStoreType, connectionString);

			// Act
			//Console.Write(generator.Drop());

			// Assert
		}

		[Test]
		[Ignore]
		public void Version16_Upgrade()
		{
			
		}

		[Test]
		[Ignore]
		public void Version16_To_Version1_Downgrade()
		{
			
		}
	}
}
