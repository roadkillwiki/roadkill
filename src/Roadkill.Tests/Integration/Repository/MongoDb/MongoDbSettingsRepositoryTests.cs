using NUnit.Framework;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Repositories;

namespace Roadkill.Tests.Integration.Repository.MongoDb
{
	[TestFixture]
	[Category("Integration")]
	[Explicit("Requires MongoDB installed on the machine running the tests")]
	[Description("For an easy install of MongoDB on Windows : http://chocolatey.org/packages?q=mongodb")]
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
			return new MongoDBRepository(ConnectionString);
		}

		protected override void Clearup()
		{
			new MongoDBRepository(ConnectionString).Wipe();
		}
	}
}
