using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;

namespace Roadkill.Tests.Integration.Repository.LightSpeed
{
	[TestFixture]
	[Category("Integration")]
	public class LightSpeedPageRepositoryTests : PageRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return TestConstants.CONNECTION_STRING; }
		}

		protected override IPageRepository GetRepository()
		{
			return new LightSpeedRepository(DataProvider.SqlServer2008, ConnectionString);
		}

		protected override void Clearup()
		{
			TestHelpers.SqlServerSetup.RecreateTables();
		}
	}
}
