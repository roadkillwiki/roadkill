using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Domain;
using Roadkill.Tests.Integration;

namespace Roadkill.Tests.CoreTests
{
	[TestFixture]
	[Description("Tests for both database and .config file settings.")]
	public class RoadkillSettingsTests
	{
		private IConfigurationContainer _config;

		[SetUp]
		public void SearchSetup()
		{
			_config = new RoadkillSettings();
		}

		[Test]
		public void RoadkillSection_Properties_Have_Correct_Key_Mappings_And_Values()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestConfigs", "test.config");

			// Act
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(appSettings.AdminRoleName, Is.EqualTo("Admin-test"), "AdminRoleName");
			Assert.That(appSettings.AttachmentsFolder, Is.EqualTo("/Attachments-test"), "AttachmentsFolder");
			Assert.That(appSettings.CachedEnabled, Is.True, "CacheEnabled");
			Assert.That(appSettings.CacheText, Is.True, "CacheText");
			Assert.That(appSettings.ConnectionStringName, Is.EqualTo("Roadkill-test"), "ConnectionStringName");
			Assert.That(appSettings.DatabaseType, Is.EqualTo(DatabaseType.Sqlite), "DatabaseType");
			Assert.That(appSettings.EditorRoleName, Is.EqualTo("Editor-test"), "EditorRoleName");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.True, "IgnoreSearchIndexErrors");
			Assert.That(appSettings.Installed, Is.True, "Installed");
			Assert.That(appSettings.IsPublicSite, Is.False, "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo("ldapstring-test"), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo("ldappassword-test"), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo("ldapusername-test"), "LdapUsername");
			Assert.That(appSettings.ResizeImages, Is.True, "ResizeImages");
			Assert.That(appSettings.UserManagerType, Is.EqualTo("SqlUserManager-test"), "SqlUserManager");
			Assert.That(appSettings.UseWindowsAuthentication, Is.False, "UseWindowsAuthentication");
		}

		[Test]
		[RequiresSTA]
		public void RoadkillSection_Optional_Settings_With_Missing_Values_Have_Default_Values()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestConfigs", "test-optional-values.config");

			// Act
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(appSettings.DatabaseType, Is.EqualTo(DatabaseType.SqlServer2005), "DatabaseType");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.False, "IgnoreSearchIndexErrors");
			Assert.That(appSettings.IsPublicSite, Is.True, "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo(""), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo(""), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo(""), "LdapUsername");
			Assert.That(appSettings.ResizeImages, Is.True, "ResizeImages");
			Assert.That(appSettings.UserManagerType, Is.EqualTo(""), "SqlUserManager");
		}

		[Test]
		public void Connection_Setting_Should_Find_Connection_Value()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestConfigs", "test.config");

			// Act
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(appSettings.ConnectionString, Is.EqualTo("connectionstring-test"), "ConnectionStringName");
		}

		[Test]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void RoadkillSection_Missing_Values_Throw_Exception()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestConfigs", "test-missing-values.config");

			// Act
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.LoadCustomConfigFile(configFilePath);

			// Assert (call the Current method to load the config)
			var x = appSettings.ConnectionStringName;
		}

		[Test]
		public void SettingsManager_Should_Save_Settings()
		{
			// Arrange
			SettingsManager settingsManager = new SettingsManager(_config, new NHibernateRepository(_config));

			SettingsSummary validConfigSettings = new SettingsSummary()
			{
				AllowedExtensions = "jpg, png, gif",
				AllowUserSignup = true,
				EnableRecaptcha = true,
				MarkupType = "markuptype",
				RecaptchaPrivateKey = "privatekey",
				RecaptchaPublicKey = "publickey",
				SiteName = "sitename",
				SiteUrl = "siteurl",
				Theme = "theme",
			};

			// Act
			settingsManager.SaveSiteConfiguration(validConfigSettings, true);

			// Assert
			Assert.That(RoadkillSettings.Current.SitePreferences.AllowedFileTypes.Contains("jpg"), "AllowedFileTypes jpg");
			Assert.That(RoadkillSettings.Current.SitePreferences.AllowedFileTypes.Contains("gif"), "AllowedFileTypes gif");
			Assert.That(RoadkillSettings.Current.SitePreferences.AllowedFileTypes.Contains("png"), "AllowedFileTypes png");
			Assert.That(RoadkillSettings.Current.SitePreferences.AllowUserSignup, Is.True, "AllowUserSignup");
			Assert.That(RoadkillSettings.Current.SitePreferences.IsRecaptchaEnabled, Is.True, "IsRecaptchaEnabled");
			Assert.That(RoadkillSettings.Current.SitePreferences.MarkupType, Is.EqualTo("markuptype"), "MarkupType");
			Assert.That(RoadkillSettings.Current.SitePreferences.RecaptchaPrivateKey, Is.EqualTo("privatekey"), "RecaptchaPrivateKey");
			Assert.That(RoadkillSettings.Current.SitePreferences.RecaptchaPublicKey, Is.EqualTo("publickey"), "RecaptchaPublicKey");
			Assert.That(RoadkillSettings.Current.SitePreferences.SiteName, Is.EqualTo("sitename"), "SiteName");
			Assert.That(RoadkillSettings.Current.SitePreferences.SiteUrl, Is.EqualTo("siteurl"), "SiteUrl");
			Assert.That(RoadkillSettings.Current.SitePreferences.Theme, Is.EqualTo("theme"), "Theme");

			// How can ~/ for Attachments be tested?
			// AppDataPath etc.?
		}
	}
}