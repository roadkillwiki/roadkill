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
	[Category("Unit")]
	public abstract class RepositoryTests
	{
		protected Roadkill.Core.Database.IRepository Repository;
		protected ApplicationSettings ApplicationSettings;

		protected abstract string ConnectionString { get; }
		protected virtual DataStoreType DataStoreType { get { return null; } }

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			// Copy the SQLite interop file for x64
			string binFolder = AppDomain.CurrentDomain.BaseDirectory;
			string sqlInteropFileSource = Path.Combine(Settings.SITE_PATH, "App_Data", "Internal", "SQLiteBinaries", "x86", "SQLite.Interop.dll");
			string sqlInteropFileDest = Path.Combine(binFolder, "SQLite.Interop.dll");

			if (!File.Exists(sqlInteropFileDest))
			{
				//File.Delete(sqlInteropFileDest);

				if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
				{
					sqlInteropFileSource = Path.Combine(Settings.SITE_PATH, "App_Data", "Internal", "SQLiteBinaries", "x64", "SQLite.Interop.dll");
				}

				System.IO.File.Copy(sqlInteropFileSource, sqlInteropFileDest, true);
			}
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationSettings = new ApplicationSettings() { ConnectionString = ConnectionString, DataStoreType = DataStoreType };
			Repository = GetRepository();
			Repository.Startup(ApplicationSettings.DataStoreType, ApplicationSettings.ConnectionString, false);
			Repository.Install(ApplicationSettings.DataStoreType, ApplicationSettings.ConnectionString, false);
		}

		protected abstract IRepository GetRepository();
	}
}
