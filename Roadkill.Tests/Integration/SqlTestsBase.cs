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
	[Category("Integration")]
	public class SqlTestsBase
	{
		protected SqlUserManager _sqlUserManager;

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

			SettingsSummary summary = new SettingsSummary(config);
			summary.ConnectionString = config.ApplicationSettings.ConnectionString;

			SettingsManager settingsManager = new SettingsManager(config, new NHibernateRepository(config));
			settingsManager.CreateTables(summary);
			settingsManager.SaveSiteConfiguration(new SettingsSummary(config) { AllowedExtensions = "jpg, gif", MarkupType = "Creole" }, true);
		}
	}
}
