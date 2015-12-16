using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Repositories;

namespace Roadkill.Tests.Integration.Repository.MongoDb
{
	[TestFixture]
	[Category("Integration")]
	public class MongoDbInstallerRepositoryTests : InstallerRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return @"mongodb://localhost:27017/local"; }
		}

		protected override string InvalidConnectionString
		{
			get { return "mongodb://invalidformat"; }
		}

		protected override IInstallerRepository GetRepository(string connectionString)
		{
			return new MongoDbInstallerRepository(connectionString);
		}

		protected override void Clearup()
		{
			new MongoDBRepository(ConnectionString).Wipe();
		}

		protected override void CheckDatabaseProcessIsRunning()
		{
			if (TestHelpers.IsMongoDBRunning() == false)
				Assert.Fail("A local MongoDB (mongod.exe) server is not running");
		}

		protected override bool AllTablesAreEmpty()
		{
			var repository = new MongoDBRepository(ConnectionString);

			return repository.AllPages().Count() == 0 &&
				   repository.AllPageContents().Count() == 0 &&
				   repository.FindAllAdmins().Count() == 0 &&
				   repository.FindAllEditors().Count() == 0 &&
				   repository.GetSiteSettings() != null;
		}
	}
}
