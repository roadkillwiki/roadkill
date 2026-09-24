using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.MongoDB;

namespace Roadkill.Tests.Integration.Repository.MongoDb
{
	[TestFixture]
	[Category("Integration")]
	public class MongoDbPageRepositoryTests : PageRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return @"mongodb://localhost:27017/local"; }
		}

		protected override IPageRepository GetRepository()
		{
			return new MongoDBPageRepository(ConnectionString);
		}

		protected override void Clearup()
		{
			new MongoDBPageRepository(ConnectionString).Wipe();
		}

		protected override void CheckDatabaseProcessIsRunning()
		{
			if (TestHelpers.IsMongoDBRunning() == false)
				Assert.Ignore("MongoDB is not tested (set ROADKILL_MONGODB_TESTS=true with a local MongoDB server to run these tests)");
		}
	}
}
