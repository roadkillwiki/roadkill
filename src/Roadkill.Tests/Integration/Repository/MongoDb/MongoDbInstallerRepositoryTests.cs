using System.Linq;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
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
			new MongoDbInstallerRepository(ConnectionString).Wipe();
		}

		protected override void CheckDatabaseProcessIsRunning()
		{
			if (TestHelpers.IsMongoDBRunning() == false)
				Assert.Fail("A local MongoDB (mongod.exe) server is not running");
		}

		protected override bool AllTablesAreEmpty()
		{
			var settingsRrepository = new MongoDBSettingsRepository(ConnectionString);
			var userRepository = new MongoDBUserRepository(ConnectionString);
			var pageRepository = new MongoDBPageRepository(ConnectionString);


			return pageRepository.AllPages().Count() == 0 &&
				   pageRepository.AllPageContents().Count() == 0 &&
				   userRepository.FindAllAdmins().Count() == 0 &&
				   userRepository.FindAllEditors().Count() == 0 &&
				   settingsRrepository.GetSiteSettings() != null;
		}
	}
}
