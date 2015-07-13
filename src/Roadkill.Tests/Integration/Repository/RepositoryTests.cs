using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Integration")]
	public abstract class RepositoryTests
	{
		protected IRepository Repository;
		protected ApplicationSettings ApplicationSettings;

		protected abstract string ConnectionString { get; }
		protected virtual DataStoreType DataStoreType { get { return null; } }

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			SqlServerSetup.RecreateLocalDbData();
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationSettings = new ApplicationSettings()
			{
				ConnectionString = ConnectionString, 
				DataStoreType = DataStoreType,
				LoggingTypes =  "none"
			};

			Repository = GetRepository();
			Repository.Startup(ApplicationSettings.DataStoreType, ApplicationSettings.ConnectionString, false);
			Repository.Install(ApplicationSettings.DataStoreType, ApplicationSettings.ConnectionString, false);
		}

		protected abstract IRepository GetRepository();
	}
}
