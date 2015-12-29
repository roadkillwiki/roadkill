using System;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.Repositories;

namespace Roadkill.Tests.Integration.Repository.LightSpeed
{
	[TestFixture]
	[Category("Integration")]
	public class LightSpeedSettingsRepositoryTests : SettingsRepositoryTests
	{
		[ThreadStatic]
		private static LightSpeedContext _context;

		public LightSpeedContext Context
		{
			get
			{
				if (_context == null)
				{
					_context = new LightSpeedContext();
					_context.ConnectionString = ConnectionString;
					_context.DataProvider = DataProvider.SqlServer2008;
					_context.IdentityMethod = IdentityMethod.GuidComb;
				}

				return _context;
			}
		}

		protected override string ConnectionString
		{
			get { return TestConstants.CONNECTION_STRING; }
		}

		protected override string InvalidConnectionString
		{
			get { return "server=(local);uid=none;pwd=none;database=doesntexist;Connect Timeout=5"; }
		}

		protected override ISettingsRepository GetRepository()
		{
			return new LightSpeedSettingsRepository(Context.CreateUnitOfWork());
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
	}
}
