using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Tests.Integration
{
	[TestFixture]
	[Category("Integration")]
	[Description("These don't test the db schemas post-install, just the installer works")]
	public class SchemaInstallerTests
	{
		[Test]
		public void Version1_Create()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.SqlServer2008;
			string connection = @"server=.\SQLEXPRESS;uid=sa;pwd=Passw0rd;database=Roadkill;";

			// Act
			SchemaInstaller.Create(dataStoreType, connection);
			SchemaInstaller.Create(dataStoreType, connection); // run twice to ensure it works with the db already existing

			// Assert (will fail with an exception automatically)
		}

		[Test]
		public void Version1_Drop()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.SqlServer2008;
			string connection = @"server=.\SQLEXPRESS;uid=sa;pwd=Passw0rd;database=Roadkill;";

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
			string connection = @"server=.\SQLEXPRESS;uid=sa;pwd=Passw0rd;database=Roadkill;";

			// Act
			SchemaInstaller.Create(dataStoreType, connection);
			SchemaInstaller.Upgrade(dataStoreType, connection);

			// Assert (will fail with an exception automatically)
		}

		[Test]
		public void Version16_To_Version1_Downgrade()
		{
			// Arrange
			DataStoreType dataStoreType = DataStoreType.SqlServer2008;
			string connection = @"server=.\SQLEXPRESS;uid=sa;pwd=Passw0rd;database=Roadkill;";

			// Act
			SchemaInstaller.Create(dataStoreType, connection);
			SchemaInstaller.Upgrade(dataStoreType, connection);
			SchemaInstaller.Downgrade(dataStoreType, connection);

			// Assert (will fail with an exception automatically)
		}
	}
}
