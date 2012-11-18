using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using NUnit.Framework;
using System.IO;
using Roadkill.Core.Domain;
using Roadkill.Core.Configuration;
using System.Configuration;

namespace Roadkill.Tests.Integration
{
	public class SqlTestsBase
	{
		protected SqlUserManager _sqlUserManager;

		/// <summary>
		/// Attempts to copy the correct SQL binaries to the bin folder for the architecture the app pool is running under.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			RoadkillApplication.SetupIoC();

			//
			// Copy the SQLite files over
			//
			string rootFolder = AppDomain.CurrentDomain.BaseDirectory;
			string sqliteFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x86", "System.Data.SQLite.dll");
			string sqliteFileDest = Path.Combine(rootFolder, "System.Data.SQLite.dll");
			string sqliteLinqFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x86", "System.Data.SQLite.Linq.dll");
			string sqliteFileLinqDest = Path.Combine(rootFolder, "System.Data.SQLite.Linq.dll");

			if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
			{
				sqliteFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x64", "System.Data.SQLite.dll");
				sqliteLinqFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x64", "System.Data.SQLite.Linq.dll");
			}

			System.IO.File.Copy(sqliteFileSource, sqliteFileDest, true);
			System.IO.File.Copy(sqliteLinqFileSource, sqliteFileLinqDest, true);
		}

		/// <summary>
		/// This method ensures that Roadkill isn't using the Http Request items to store its context
		/// (IsWeb = false), and is all in-memory. It also recreated the SQL lite database each time.
		/// </summary>
		[SetUp]
		public void Initialize()
		{
			RoadkillApplication.SetupIoC();

			IConfigurationContainer config = new RoadkillSettings();
			config.ApplicationSettings = new ApplicationSettings();
			config.ApplicationSettings.Load(null); // from app.config

			SettingsSummary summary = new SettingsSummary();
			summary.ConnectionString = config.ApplicationSettings.ConnectionString;

			SettingsManager settingsManager = new SettingsManager(config, new NHibernateRepository(config));
			settingsManager.CreateTables(summary);
			settingsManager.SaveSiteConfiguration(new SettingsSummary() { AllowedExtensions = "jpg, gif", MarkupType = "Creole" }, true);
		}
	}
}
