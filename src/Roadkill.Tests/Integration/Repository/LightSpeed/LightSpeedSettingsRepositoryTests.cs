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
	public class LightSpeedSettingsRepositoryTests : SettingsRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return SqlExpressSetup.ConnectionString; }
		}

		protected override DataStoreType DataStoreType
		{
			get { return DataStoreType.SqlServer2012; }
		}

		protected override string InvalidConnectionString
		{
			get { return "server=(local);uid=none;pwd=none;database=doesntexist;Connect Timeout=5"; }
		}

		protected override IRepository GetRepository()
		{
			return new LightSpeedRepository(ApplicationSettings);
		}
	}
}
