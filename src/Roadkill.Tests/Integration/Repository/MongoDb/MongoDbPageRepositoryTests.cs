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
			get { return TestConstants.MONGODB_CONNECTION_STRING; }
		}

		protected override IPageRepository GetRepository()
		{
			return new MongoDBPageRepository(ConnectionString);
		}

		protected override void Clearup()
		{
			new MongoDBPageRepository(ConnectionString).Wipe();
		}
	}
}
