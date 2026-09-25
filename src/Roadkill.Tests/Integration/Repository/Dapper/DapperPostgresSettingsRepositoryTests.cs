using System;
using NUnit.Framework;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Database.Repositories.Dapper;

namespace Roadkill.Tests.Integration.Repository.Dapper
{
	[TestFixture]
	[Category("Integration")]
	public class DapperPostgresSettingsRepositoryTests : SettingsRepositoryTests
	{
		protected override string ConnectionString => TestConstants.POSTGRES_CONNECTION_STRING;

		protected override string InvalidConnectionString
		{
			get
			{
				return TestConstants.POSTGRES_CONNECTION_STRING.Replace("Database=", "DatabaseInator=");
			}
		}

		protected override ISettingsRepository GetRepository()
		{
			var factory = new PostgresConnectionFactory(ConnectionString);
			return new DapperSettingsRepository(factory);
		}

		protected override void Clearup()
		{
			TestHelpers.PostgresSetup.RecreateTables();
			TestHelpers.PostgresSetup.ClearDatabase();
		}
	}
}
