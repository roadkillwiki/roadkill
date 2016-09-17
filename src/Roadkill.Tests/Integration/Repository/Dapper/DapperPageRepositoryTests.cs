using System;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories.Dapper;

namespace Roadkill.Tests.Integration.Repository.Dapper
{
	[TestFixture]
	[Category("Integration")]
	public class DapperPageRepositoryTests : PageRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return TestConstants.POSTGRES_CONNECTION_STRING; }
		}

		protected override IPageRepository GetRepository()
		{
			var factory = new PostgresConnectionFactory(ConnectionString);
			return new DapperPageRepository(factory);
		}

		protected override void Clearup()
		{
			TestHelpers.PostgresSetup.RecreateTables();
			TestHelpers.PostgresSetup.ClearDatabase();
		}
	}
}
