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
using Roadkill.Core.Database.RavenDB;

namespace Roadkill.Tests.Integration.Repository.RavenDB
{
	[TestFixture]
	[Category("Unit")]
	[Explicit("Requires RavenDB installed on the machine running the tests")]
	[Description("For an easy install of MongoDB on Windows : http://chocolatey.org/packages?q=mongodb")]
	public class RavenDBUserRepositoryTests : UserRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return @"Url=http://ravendb.localhost:8080;Database=Roadkill"; }
		}

		protected override IRepository GetRepository()
		{
			return new RavenDBRepository(ApplicationSettings);
		}
	}
}
