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
		protected override string ConnectionString => TestConstants.SQLSERVER_CONNECTION_STRING;

		protected override string InvalidConnectionString
		{
			get
			{
				return TestConstants.SQLSERVER_CONNECTION_STRING.Replace("Database=", "DatabaseInator=");
			}
		}

		protected override ISettingsRepository GetRepository()
		{
			var factory = new SqlConnectionFactory(ConnectionString);
			return new DapperSettingsRepository(factory);
		}

		protected override void Clearup()
		{
			TestHelpers.SqlServerSetup.RecreateTables();
			TestHelpers.SqlServerSetup.ClearDatabase();
		}
	}
}
