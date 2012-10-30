using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Converters;
using Roadkill.Tests.Integration;

namespace Roadkill.Tests.CoreTests
{
	[TestFixture]
	[Description("These tests are for both the SettingsManager and the Settings class, and the web.config Section.")]
	public class SettingsTests : SqlTestsBase
	{
		// These tests clash with each the RoadkillSection tests if not in seperate (STA) threads.

		[Test]
		[RequiresSTA] 
		public void Settings_Should_Contain_Same_Values_As_WebConfig()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestConfigs", "test.config");
	
			// Act
			RoadkillSection.LoadCustomConfigFile(configFilePath);
			RoadkillSettings.ConnectionString = ""; // blank it as the Setup() method sets it

			// Assert
			Assert.That(RoadkillSettings.AdminRoleName, Is.EqualTo("Admin-test"), "AdminRoleName");
			Assert.That(RoadkillSettings.AttachmentsFolder, Is.EqualTo("/Attachments-test"), "AttachmentsFolder");
			Assert.That(RoadkillSettings.CachedEnabled, Is.True, "CacheEnabled");
			Assert.That(RoadkillSettings.CacheText, Is.True, "CacheText");
			Assert.That(RoadkillSettings.ConnectionString, Is.EqualTo("connectionstring-test"), "ConnectionStringName");
			Assert.That(RoadkillSettings.DatabaseType, Is.EqualTo(DatabaseType.Sqlite), "DatabaseType");
			Assert.That(RoadkillSettings.EditorRoleName, Is.EqualTo("Editor-test"), "EditorRoleName");
			Assert.That(RoadkillSettings.IgnoreSearchIndexErrors, Is.True, "IgnoreSearchIndexErrors");
			Assert.That(RoadkillSettings.Installed, Is.True, "Installed");
			Assert.That(RoadkillSettings.IsPublicSite, Is.False, "IsPublicSite");
			Assert.That(RoadkillSettings.LdapConnectionString, Is.EqualTo("ldapstring-test"), "LdapConnectionString");
			Assert.That(RoadkillSettings.LdapPassword, Is.EqualTo("ldappassword-test"), "LdapPassword");
			Assert.That(RoadkillSettings.LdapUsername, Is.EqualTo("ldapusername-test"), "LdapUsername");
			Assert.That(RoadkillSettings.ResizeImages, Is.True, "ResizeImages");
			Assert.That(RoadkillSettings.UserManagerType, Is.EqualTo("SqlUserManager-test"), "SqlUserManager");
			Assert.That(RoadkillSettings.UseWindowsAuthentication, Is.False, "UseWindowsAuthentication");
		}

		[Test]
		[RequiresSTA]
		public void Settings_Should_Contain_Same_Values_As_Database_ConfigurationClass()
		{
			// Arrange
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
			SettingsManager.SaveSiteConfiguration(validConfigSettings, true);

			// Assert
			Assert.That(RoadkillSettings.AllowedFileTypes.Contains("jpg"), "AllowedFileTypes jpg");
			Assert.That(RoadkillSettings.AllowedFileTypes.Contains("gif"), "AllowedFileTypes gif");
			Assert.That(RoadkillSettings.AllowedFileTypes.Contains("png"), "AllowedFileTypes png");
			Assert.That(RoadkillSettings.AllowUserSignup, Is.True, "AllowUserSignup");
			Assert.That(RoadkillSettings.IsRecaptchaEnabled, Is.True, "IsRecaptchaEnabled");
			Assert.That(RoadkillSettings.MarkupType, Is.EqualTo("markuptype"), "MarkupType");
			Assert.That(RoadkillSettings.RecaptchaPrivateKey, Is.EqualTo("privatekey"), "RecaptchaPrivateKey");
			Assert.That(RoadkillSettings.RecaptchaPublicKey, Is.EqualTo("publickey"), "RecaptchaPublicKey");
			Assert.That(RoadkillSettings.SiteName, Is.EqualTo("sitename"), "SiteName");
			Assert.That(RoadkillSettings.SiteUrl, Is.EqualTo("siteurl"), "SiteUrl");
			Assert.That(RoadkillSettings.Theme, Is.EqualTo("theme"), "Theme");

			// How can ~/ for Attachments be tested?
			// AppDataPath etc.?
		}

		[Test]
		[RequiresSTA]
		public void Connection_Setting_Should_Find_Connection_Value()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestConfigs", "test.config");

			// Act
			RoadkillSettings.ConnectionString = null; // blank it so the new config is reloaded
			RoadkillSection.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(RoadkillSettings.ConnectionString, Is.EqualTo("connectionstring-test"), "ConnectionStringName");
		}
	}
}