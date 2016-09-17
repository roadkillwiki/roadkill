using System;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories.Dapper;

namespace Roadkill.Tests.Integration.Repository.Dapper
{
	[TestFixture]
	[Category("Integration")]
	public class DapperUserRepositoryTests : UserRepositoryTests
	{
		protected override string ConnectionString => TestConstants.POSTGRES_CONNECTION_STRING;

		protected override IUserRepository GetRepository()
		{
			var factory = new PostgresConnectionFactory(ConnectionString);
			return new DapperUserRepository(factory);
		}

		protected override void Clearup()
		{
			//TestHelpers.SqlServerSetup.RecreateTables();
			//TestHelpers.SqlServerSetup.ClearDatabase();

			TestHelpers.PostgresSetup.RecreateTables();
			TestHelpers.PostgresSetup.ClearDatabase();
		}
	}
}
