﻿using System;
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
	public class LightSpeedPageRepositoryTests : PageRepositoryTests
	{
		protected override string ConnectionString
		{
			get { return @"Data Source=roadkill-integrationtests.sqlite;"; }
		}

		protected override DataStoreType DataStoreType
		{
			get { return DataStoreType.Sqlite; }
		}

		protected override IRepository GetRepository()
		{
			return new LightSpeedRepository(ApplicationSettings);
		}
	}
}
