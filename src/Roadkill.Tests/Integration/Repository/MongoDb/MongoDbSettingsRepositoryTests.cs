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
			get { return TestConstants.MONGODB_CONNECTION_STRING; }
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
	}
}
