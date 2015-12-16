using System.Data;
using System.Linq;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Tests.Integration.Repository.LightSpeed
{
	[TestFixture]
	[Category("Integration")]
	public class LightSpeedInstallerRepositoryTests : InstallerRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return TestConstants.CONNECTION_STRING; }
		}

		protected override string InvalidConnectionString
		{
			get { return "server=(local);uid=none;pwd=none;database=doesntexist;Connect Timeout=5"; }
		}

		protected override IInstallerRepository GetRepository(string connectionString)
		{
			return new LightSpeedInstallerRepository(DataProvider.SqlServer2008, new SqlServerSchema(),  connectionString);
		}

		protected override void Clearup()
		{
			TestHelpers.SqlServerSetup.ClearDatabase();
		}

		protected override void CheckDatabaseProcessIsRunning()
		{
			if (TestHelpers.IsSqlServerRunning() == false)
				Assert.Fail("A local Sql Server (sqlservr.exe) is not running");
		}

		protected override bool AllTablesAreEmpty()
		{
			var repository = new LightSpeedRepository(DataProvider.SqlServer2008, ConnectionString);

			return repository.AllPages().Count() == 0 &&
				   repository.AllPageContents().Count() == 0 &&
				   repository.FindAllAdmins().Count() == 0 &&
				   repository.FindAllEditors().Count() == 0 &&
				   repository.GetSiteSettings() != null;
		}
	}
}
