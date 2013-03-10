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
	public class SchemaInstallerTests
	{
		[Test]
		public void Version1_Create()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.Sqlite;
			string connectionString = @"Data Source=roadkill-integrationtests.sqlite;";
			SchemaInstaller installer = new SchemaInstaller(dataStoreType, connectionString);

			// Act
			installer.Create();
			bool schemaExists = installer.HasSchema();
			installer.Create();
			bool schemaExistsAgagin = installer.HasSchema();

			// Assert
		}

		[Test]
		public void Version1_Drop()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.Sqlite;
			string connection = @"Data Source=roadkill-integrationtests.sqlite;";

			// Act
			SchemaInstaller.Create(dataStoreType, connection);
			SchemaInstaller.Drop(dataStoreType, connection);

			// Assert (will fail with an exception automatically)
		}

		[Test]
		public void Version16_Upgrade()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.SqlServer2008;
			string connection = @"Data Source=roadkill-integrationtests.sqlite;";
			connection = @"server=.\SQLEXPRESS;uid=sa;pwd=Passw0rd;database=Roadkill;";

			// Act
			SchemaInstaller.Create(dataStoreType, connection);
			SchemaInstaller.Upgrade(dataStoreType, connection);

			// Assert (will fail with an exception automatically)
		}

		[Test]
		public void Version16_To_Version1_Downgrade()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.Sqlite;
			string connection = @"Data Source=roadkill-integrationtests.sqlite;";
			//connection = @"server=.\SQLEXPRESS;uid=sa;pwd=Passw0rd;database=Roadkill;";

			// Act
			SchemaInstaller.Create(dataStoreType, connection);
			SchemaInstaller.Upgrade(dataStoreType, connection);
			//SchemaInstaller.Downgrade(dataStoreType, connection);
//
			// Assert (will fail with an exception automatically)
		}
	}
}
