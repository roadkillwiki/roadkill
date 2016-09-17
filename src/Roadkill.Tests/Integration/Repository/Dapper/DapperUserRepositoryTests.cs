using System;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.Repositories.Dapper;

namespace Roadkill.Tests.Integration.Repository.Dapper
{
	[TestFixture]
	[Category("Integration")]
	public class DapperUserRepositoryTests : UserRepositoryTests
	{
		protected override string ConnectionString => TestConstants.CONNECTION_STRING;

		protected override IUserRepository GetRepository()
		{
			var factory = new SqlConnectionFactory(ConnectionString);
			return new DapperUserRepository(factory);
		}

		protected override void Clearup()
		{
			TestHelpers.SqlServerSetup.RecreateTables();
			TestHelpers.SqlServerSetup.ClearDatabase();
		}

		protected override void CheckDatabaseProcessIsRunning()
		{
			if (TestHelpers.IsSqlServerRunning() == false)
				Assert.Fail("A local Sql Server (sqlservr.exe) is not running");
		}
	}
}
