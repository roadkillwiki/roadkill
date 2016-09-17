using System.Linq;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Tests.Integration.Repository.LightSpeed
{
	[TestFixture]
	[Category("Integration")]
	public class LightSpeedInstallerRepositoryTests : InstallerRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return TestConstants.SQLSERVER_CONNECTION_STRING; }
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

		protected override bool HasEmptyTables()
		{
			IUnitOfWork unitOfWork = CreateUnitOfWork();

			var settingsRepository = new LightSpeedSettingsRepository(unitOfWork);
			var userRepository = new LightSpeedUserRepository(unitOfWork);
			var pageRepository = new LightSpeedPageRepository(unitOfWork);

			return pageRepository.AllPages().Count() == 0 &&
				   pageRepository.AllPageContents().Count() == 0 &&
				   userRepository.FindAllAdmins().Count() == 0 &&
				   userRepository.FindAllEditors().Count() == 0 &&
				   settingsRepository.GetSiteSettings() != null;
		}

		protected override bool HasAdminUser()
		{
			IUnitOfWork unitOfWork = CreateUnitOfWork();
			var userRepository = new LightSpeedUserRepository(unitOfWork);

			return userRepository.FindAllAdmins().Count() == 1;
		}

		protected override SiteSettings GetSiteSettings()
		{
			IUnitOfWork unitOfWork = CreateUnitOfWork();
			var settingsRepository = new LightSpeedSettingsRepository(unitOfWork);
			return settingsRepository.GetSiteSettings();
		}

		private IUnitOfWork CreateUnitOfWork()
		{
			var context = new LightSpeedContext();
			context.ConnectionString = ConnectionString;
			context.DataProvider = DataProvider.SqlServer2008;
			context.IdentityMethod = IdentityMethod.GuidComb;

			IUnitOfWork unitOfWork = context.CreateUnitOfWork();
			return unitOfWork;
		}
	}
}
