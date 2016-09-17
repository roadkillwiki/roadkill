using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.MongoDB;

namespace Roadkill.Tests.Integration.Repository.MongoDb
{
	[TestFixture]
	[Category("Integration")]
	public class MongoDbUserRepositoryTests : UserRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return @"mongodb://localhost:27017/local"; }
		}

		protected override IUserRepository GetRepository()
		{
			return new MongoDBUserRepository(ConnectionString);
		}

		protected override void Clearup()
		{
			new MongoDBUserRepository(ConnectionString).Wipe();
		}
	}
}
