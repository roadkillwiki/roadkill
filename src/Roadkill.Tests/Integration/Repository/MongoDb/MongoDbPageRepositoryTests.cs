using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.MongoDB;

namespace Roadkill.Tests.Integration.Repository.MongoDb
{
	[TestFixture]
	[Category("Integration")]
	[Explicit("Requires MongoDB installed on the machine running the tests")]
	[Description("For an easy install of MongoDB on Windows : http://chocolatey.org/packages?q=mongodb")]
	public class MongoDbPageRepositoryTests : PageRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return @"mongodb://localhost:27017/local"; }
		}

		protected override IRepository GetRepository()
		{
			return new MongoDBRepository(ConnectionString);
		}

		protected override void Clearup()
		{
			new MongoDBRepository(ConnectionString).Wipe();
		}
	}
}
