using System;
using NUnit.Framework;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Database.Repositories.Dapper;

namespace Roadkill.Tests.Integration.Repository.Dapper
{
	[TestFixture]
	[Category("Integration")]
	public class DapperSettingsRepositoryTests : SettingsRepositoryTests
	{
		protected override string ConnectionString => TestConstants.POSTGRES_CONNECTION_STRING;

		protected override string InvalidConnectionString
		{
			get { return "User ID=postgres;Password=mysecretpassword;Host=localhost;Port=5432;Database=doesntexist;"; }
		}

		protected override ISettingsRepository GetRepository()
		{
			var factory = new PostgresConnectionFactory(ConnectionString);
			return new DapperSettingsRepository(factory);
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
