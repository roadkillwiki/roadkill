using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;

namespace Roadkill.Tests.Integration.Repository.LightSpeed
{
	[TestFixture]
	[Category("Unit")]
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

		protected override IRepository GetRepository()
		{
			return new MongoDBRepository(ApplicationSettings);
		}
	}
}
