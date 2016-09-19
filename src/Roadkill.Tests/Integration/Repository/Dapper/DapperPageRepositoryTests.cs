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
			get { return TestConstants.SQLSERVER_CONNECTION_STRING; }
		}

		protected override IPageRepository GetRepository()
		{
			var factory = new SqlConnectionFactory(ConnectionString);
			return new DapperPageRepository(factory);
		}

		protected override void Clearup()
		{
			TestHelpers.SqlServerSetup.RecreateTables();
			TestHelpers.SqlServerSetup.ClearDatabase();
		}
	}
}
