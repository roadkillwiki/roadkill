using NUnit.Framework;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Repositories;

namespace Roadkill.Tests.Integration.Repository.MongoDb
{
	[TestFixture]
	[Category("Integration")]
	public class MongoDbSettingsRepositoryTests : SettingsRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return @"mongodb://localhost:27017/local"; }
		}

		protected override string InvalidConnectionString
		{
			get { return "mongodb://invalidformat"; }
		}

		protected override ISettingsRepository GetRepository()
		{
			return new MongoDBSettingsRepository(ConnectionString);
		}

		protected override void Clearup()
		{
			new MongoDBSettingsRepository(ConnectionString).Wipe();
		}

		protected override void CheckDatabaseProcessIsRunning()
		{
			if (TestHelpers.IsMongoDBRunning() == false)
				Assert.Ignore("MongoDB is not tested (set ROADKILL_MONGODB_TESTS=true with a local MongoDB server to run these tests)");
		}
	}
}
