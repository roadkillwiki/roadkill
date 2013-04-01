using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Logging;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Description("Tests for both database and .config file settings.")]
	[Category("Integration")]
	public class ConfigFileTests
	{
		private ApplicationSettings _settings;

		[SetUp]
		public void Setup()
		{
			_settings = new ApplicationSettings();
		}

		[Test]
		public void RoadkillSection_Properties_Have_Correct_Key_Mappings_And_Values()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			ConfigFileManager configManager = new ConfigFileManager(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.AdminRoleName, Is.EqualTo("Admin-test"), "AdminRoleName");
			Assert.That(appSettings.AttachmentsFolder, Is.EqualTo("/Attachments-test"), "AttachmentsFolder");
			Assert.That(appSettings.UseObjectCache, Is.True, "UseObjectCache");
			Assert.That(appSettings.UseBrowserCache, Is.True, "UseBrowserCache");
			Assert.That(appSettings.ConnectionStringName, Is.EqualTo("Roadkill-test"), "ConnectionStringName");
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.Sqlite), "DatabaseType");
			Assert.That(appSettings.EditorRoleName, Is.EqualTo("Editor-test"), "EditorRoleName");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.True, "IgnoreSearchIndexErrors");
			Assert.That(appSettings.Installed, Is.True, "Installed");
			Assert.That(appSettings.IsPublicSite, Is.False, "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo("ldapstring-test"), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo("ldappassword-test"), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo("ldapusername-test"), "LdapUsername");
			Assert.That(appSettings.LoggingType, Is.EqualTo(LogType.All), "LoggingType");
			Assert.That(appSettings.LogErrorsOnly, Is.False, "LogErrorsOnly");
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
			ConfigFileManager configManager = new ConfigFileManager(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.SqlServer2005), "DatabaseType");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.False, "IgnoreSearchIndexErrors");
			Assert.That(appSettings.IsPublicSite, Is.True, "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo(""), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo(""), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo(""), "LdapUsername");
			Assert.That(appSettings.LoggingType, Is.EqualTo(LogType.XmlFile), "LoggingType");
			Assert.That(appSettings.LogErrorsOnly, Is.True, "LoggingType");
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
			ConfigFileManager configManager = new ConfigFileManager(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.ConnectionString, Is.EqualTo("connectionstring-test"), "ConnectionStringName");
		}

		[Test]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void RoadkillSection_Missing_Values_Throw_Exception()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-missing-values.config");

			// Act
			ConfigFileManager configManager = new ConfigFileManager(configFilePath);
			
			// Assert
		}

		[Test]
		public void RoadkillSection_Legacy_CacheValues_Are_Ignored()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-legacy-values.config");

			// Act
			ConfigFileManager configManager = new ConfigFileManager(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.UseObjectCache, Is.True, "UseObjectCache [legacy test for cacheEnabled]");
			Assert.That(appSettings.UseBrowserCache, Is.False, "UseBrowserCache [legacy test for cacheText]");
		}

		[Test]
		public void RoadkillSection_Legacy_DatabaseType_Is_Used()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-legacy-values.config");

			// Act
			ConfigFileManager configManager = new ConfigFileManager(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.Sqlite), "DataStoreType [legacy test for databaseType]");
		}

		[Test]
		public void SettingsManager_Should_Save_Settings()
		{
			// Arrange
			SiteSettings siteSettings = new SiteSettings()
			{
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

			RepositoryMock repository = new RepositoryMock();

			DependencyContainer iocSetup = new DependencyContainer(_settings, repository, new UserContext(null)); // context isn't used
			iocSetup.RegisterTypes();
			SettingsManager settingsManager = new SettingsManager(_settings, repository);

			// Act
			settingsManager.SaveSiteSettings(validConfigSettings, true);

			// Assert
			SiteSettings actualSettings = settingsManager.GetSiteSettings();

			Assert.That(actualSettings.AllowedFileTypes.Contains("jpg"), "AllowedFileTypes jpg");
			Assert.That(actualSettings.AllowedFileTypes.Contains("gif"), "AllowedFileTypes gif");
			Assert.That(actualSettings.AllowedFileTypes.Contains("png"), "AllowedFileTypes png");
			Assert.That(actualSettings.AllowUserSignup, Is.True, "AllowUserSignup");
			Assert.That(actualSettings.IsRecaptchaEnabled, Is.True, "IsRecaptchaEnabled");
			Assert.That(actualSettings.MarkupType, Is.EqualTo("markuptype"), "MarkupType");
			Assert.That(actualSettings.RecaptchaPrivateKey, Is.EqualTo("privatekey"), "RecaptchaPrivateKey");
			Assert.That(actualSettings.RecaptchaPublicKey, Is.EqualTo("publickey"), "RecaptchaPublicKey");
			Assert.That(actualSettings.SiteName, Is.EqualTo("sitename"), "SiteName");
			Assert.That(actualSettings.SiteUrl, Is.EqualTo("siteurl"), "SiteUrl");
			Assert.That(actualSettings.Theme, Is.EqualTo("theme"), "Theme");

			// How can ~/ for Attachments be tested?
			// AppDataPath etc.?
		}
		
		[Test]
		public void UseWindowsAuth_Should_Load_ActiveDirectory_UserManager()
		{
			// Arrange
			Mock<IRepository> mockRepository = new Mock<IRepository>();
			Mock<IUserContext> mockContext = new Mock<IUserContext>();

			ApplicationSettings settings = new ApplicationSettings();
			settings.UseWindowsAuthentication = true;
			settings.LdapConnectionString = "LDAP://dc=roadkill.org";
			settings.AdminRoleName = "editors";
			settings.EditorRoleName = "editors";

			// Act
			DependencyContainer iocSetup = new DependencyContainer(settings, mockRepository.Object, mockContext.Object);
			iocSetup.RegisterTypes();

			// Assert
			Assert.That(DependencyContainer.GetInstance<UserManagerBase>(), Is.TypeOf(typeof(ActiveDirectoryUserManager)));
		}
		
		[Test]
		public void Should_Use_DefaultUserManager_By_Default()
		{
			// Arrange
			Mock<IRepository> mockRepository = new Mock<IRepository>();
			Mock<IUserContext> mockContext = new Mock<IUserContext>();
			ApplicationSettings settings = new ApplicationSettings();

			// Act
			DependencyContainer iocSetup = new DependencyContainer(settings, mockRepository.Object, mockContext.Object);
			iocSetup.RegisterTypes();

			// Assert
			Assert.That(DependencyContainer.GetInstance<UserManagerBase>(), Is.TypeOf(typeof(FormsAuthenticationUserManager)));
		}

		[Test]
		[Ignore]
		public void MongoDB_databaseType_Should_Load_Repository()
		{
			// Arrange
			Mock<IRepository> mockRepository = new Mock<IRepository>();
			Mock<IUserContext> mockContext = new Mock<IUserContext>();
			ApplicationSettings settings = new ApplicationSettings();
			settings.DataStoreType = DataStoreType.MongoDB;

			// Act
			DependencyContainer iocSetup = new DependencyContainer(settings, mockRepository.Object, mockContext.Object);
			iocSetup.RegisterTypes();

			// Assert
			Assert.That(DependencyContainer.GetInstance<UserManagerBase>(), Is.TypeOf(typeof(FormsAuthenticationUserManager)));
		}

		private string GetConfigPath(string filename)
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration", "TestConfigs", filename);
		}
	}
}