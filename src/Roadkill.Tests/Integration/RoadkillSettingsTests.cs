using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Description("Tests for both database and .config file settings.")]
	[Category("Integration")]
	public class RoadkillSettingsTests
	{
		private IConfigurationContainer _config;

		[SetUp]
		public void Setup()
		{
			_config = new RoadkillSettings();
		}

		[Test]
		public void RoadkillSection_Properties_Have_Correct_Key_Mappings_And_Values()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(appSettings.AdminRoleName, Is.EqualTo("Admin-test"), "AdminRoleName");
			Assert.That(appSettings.AttachmentsFolder, Is.EqualTo("/Attachments-test"), "AttachmentsFolder");
			Assert.That(appSettings.CacheEnabled, Is.True, "CacheEnabled");
			Assert.That(appSettings.CacheText, Is.True, "CacheText");
			Assert.That(appSettings.ConnectionStringName, Is.EqualTo("Roadkill-test"), "ConnectionStringName");
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.Sqlite), "DatabaseType");
			Assert.That(appSettings.EditorRoleName, Is.EqualTo("Editor-test"), "EditorRoleName");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.True, "IgnoreSearchIndexErrors");
			Assert.That(appSettings.Installed, Is.True, "Installed");
			Assert.That(appSettings.IsPublicSite, Is.False, "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo("ldapstring-test"), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo("ldappassword-test"), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo("ldapusername-test"), "LdapUsername");
			Assert.That(appSettings.ResizeImages, Is.True, "ResizeImages");
			Assert.That(appSettings.UseHtmlWhiteList, Is.EqualTo(false), "UseHtmlWhiteList");
			Assert.That(appSettings.UserManagerType, Is.EqualTo("DefaultUserManager-test"), "DefaultUserManager");
			Assert.That(appSettings.UseWindowsAuthentication, Is.False, "UseWindowsAuthentication");
		}

		[Test]
		public void RoadkillSection_Optional_Settings_With_Missing_Values_Have_Default_Values()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-optional-values.config");

			// Act
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.SqlServer2005), "DatabaseType");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.False, "IgnoreSearchIndexErrors");
			Assert.That(appSettings.IsPublicSite, Is.True, "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo(""), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo(""), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo(""), "LdapUsername");
			Assert.That(appSettings.ResizeImages, Is.True, "ResizeImages");
			Assert.That(appSettings.UseHtmlWhiteList, Is.EqualTo(true), "UseHtmlWhiteList");
			Assert.That(appSettings.UserManagerType, Is.EqualTo(""), "DefaultUserManager");
		}

		[Test]
		public void Connection_Setting_Should_Find_Connection_Value()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(appSettings.ConnectionString, Is.EqualTo("connectionstring-test"), "ConnectionStringName");
		}

		[Test]
		public void RoadkillSection_Missing_Values_Throw_Exception()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-missing-values.config");

			// Act
			ApplicationSettings appSettings = new ApplicationSettings();
			

			// Assert
			Assert.Throws<ConfigurationErrorsException>(() => 
			{ 
				//var x = appSettings.ConnectionStringName; 
				appSettings.LoadCustomConfigFile(configFilePath);
			});
		}

		[Test]
		public void SettingsManager_Should_Save_Settings()
		{
			// Arrange
			SitePreferences preferences = new SitePreferences()
			{
				Id = SitePreferences.ConfigurationId,
				AllowedFileTypes = "jpg, png, gif",
				AllowUserSignup = true,
				IsRecaptchaEnabled = true,
				MarkupType = "markuptype",
				RecaptchaPrivateKey = "privatekey",
				RecaptchaPublicKey = "publickey",
				SiteName = "sitename",
				SiteUrl = "siteurl",
				Theme = "theme",
			};
			SettingsSummary validConfigSettings = new SettingsSummary(_config)
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

			Mock<IRepository> mockRepository = new Mock<IRepository>();
			mockRepository.Setup(x => x.SaveOrUpdate<SitePreferences>(preferences));
			mockRepository.Setup(x => x.GetSitePreferences()).Returns(preferences);

			IoCSetup iocSetup = new IoCSetup(_config, mockRepository.Object, new RoadkillContext(null)); // context isn't used
			iocSetup.Run();
			SettingsManager settingsManager = new SettingsManager(_config, mockRepository.Object);

			// Act
			settingsManager.SaveSiteConfiguration(validConfigSettings, true);

			// Assert
			mockRepository.Verify(x => x.SaveOrUpdate<SitePreferences>(
				It.Is<SitePreferences>(s => s.Id == preferences.Id && s.MarkupType == preferences.MarkupType)
			));

			IConfigurationContainer config = RoadkillSettings.GetInstance();

			Assert.That(config.SitePreferences.AllowedFileTypes.Contains("jpg"), "AllowedFileTypes jpg");
			Assert.That(config.SitePreferences.AllowedFileTypes.Contains("gif"), "AllowedFileTypes gif");
			Assert.That(config.SitePreferences.AllowedFileTypes.Contains("png"), "AllowedFileTypes png");
			Assert.That(config.SitePreferences.AllowUserSignup, Is.True, "AllowUserSignup");
			Assert.That(config.SitePreferences.IsRecaptchaEnabled, Is.True, "IsRecaptchaEnabled");
			Assert.That(config.SitePreferences.MarkupType, Is.EqualTo("markuptype"), "MarkupType");
			Assert.That(config.SitePreferences.RecaptchaPrivateKey, Is.EqualTo("privatekey"), "RecaptchaPrivateKey");
			Assert.That(config.SitePreferences.RecaptchaPublicKey, Is.EqualTo("publickey"), "RecaptchaPublicKey");
			Assert.That(config.SitePreferences.SiteName, Is.EqualTo("sitename"), "SiteName");
			Assert.That(config.SitePreferences.SiteUrl, Is.EqualTo("siteurl"), "SiteUrl");
			Assert.That(config.SitePreferences.Theme, Is.EqualTo("theme"), "Theme");

			// How can ~/ for Attachments be tested?
			// AppDataPath etc.?
		}
		
		[Test]
		public void Custom_UserManager_Should_Load()
		{
			// Arrange
			Mock<IRepository> mockRepository = new Mock<IRepository>();
			Mock<IRoadkillContext> mockContext = new Mock<IRoadkillContext>();

			IConfigurationContainer config = new RoadkillSettings();
			config.SitePreferences = new SitePreferences();
			config.ApplicationSettings = new ApplicationSettings();
			config.ApplicationSettings.UserManagerType = typeof(UserManagerStub).AssemblyQualifiedName;
			
			// Act
			IoCSetup iocSetup = new IoCSetup(config, mockRepository.Object, mockContext.Object);
			iocSetup.Run();

			// Assert
			Assert.That(UserManager.GetInstance(), Is.TypeOf(typeof(UserManagerStub)));
		}
		
		[Test]
		public void UseWindowsAuth_Should_Load_ActiveDirectory_UserManager()
		{
			// Arrange
			Mock<IRepository> mockRepository = new Mock<IRepository>();
			Mock<IRoadkillContext> mockContext = new Mock<IRoadkillContext>();

			IConfigurationContainer config = new RoadkillSettings();
			config.SitePreferences = new SitePreferences();
			config.ApplicationSettings = new ApplicationSettings();
			config.ApplicationSettings.UseWindowsAuthentication = true;
			config.ApplicationSettings.LdapConnectionString = "LDAP://dc=roadkill.org";
			config.ApplicationSettings.AdminRoleName = "editors";
			config.ApplicationSettings.EditorRoleName = "editors";

			// Act
			IoCSetup iocSetup = new IoCSetup(config, mockRepository.Object, mockContext.Object);
			iocSetup.Run();

			// Assert
			Assert.That(UserManager.GetInstance(), Is.TypeOf(typeof(ActiveDirectoryUserManager)));
		}
		
		[Test]
		public void Should_Use_DefaultUserManager_By_Default()
		{
			// Arrange
			Mock<IRepository> mockRepository = new Mock<IRepository>();
			Mock<IRoadkillContext> mockContext = new Mock<IRoadkillContext>();

			IConfigurationContainer config = new RoadkillSettings();
			config.SitePreferences = new SitePreferences();
			config.ApplicationSettings = new ApplicationSettings();

			// Act
			IoCSetup iocSetup = new IoCSetup(config, mockRepository.Object, mockContext.Object);
			iocSetup.Run();

			// Assert
			Assert.That(UserManager.GetInstance(), Is.TypeOf(typeof(DefaultUserManager)));
		}

		private string GetConfigPath(string filename)
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration", "TestConfigs", filename);
		}
	}
}