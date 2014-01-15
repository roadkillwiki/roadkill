using System;
using System.Configuration;
using System.IO;
using System.Web.Configuration;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Description("Tests writing and reading .config files.")]
	[Category("Integration")]
	public class FullTrustConfigReaderWriterTests
	{
		[SetUp]
		public void Setup()
		{
			// Copy the config files so they're fresh before each test
			string source = Path.Combine(Settings.ROOT_FOLDER, "src", "Roadkill.Tests", "Integration", "Configuration", "TestConfigs");
			string destination = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration", "Configuration", "TestConfigs");

			foreach (string filename in Directory.GetFiles(source))
			{
				FileInfo info = new FileInfo(filename);
				File.Copy(filename, Path.Combine(destination, info.Name), true);
			}
		}

		[Test]
		public void Load_Should_Return_RoadkillSection()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			RoadkillSection appSettings = configManager.Load();

			// Assert
			Assert.That(appSettings.AdminRoleName, Is.EqualTo("Admin-test"), "AdminRoleName"); // basic check
		}

		[Test]
		public void UpdateLanguage_Should_Save_Language_Code_To_Globalization_Section()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			configManager.UpdateLanguage("fr-FR");

			// Assert
			Configuration config = configManager.GetConfiguration();
			GlobalizationSection globalizationSection = config.GetSection("system.web/globalization") as GlobalizationSection;

			Assert.That(globalizationSection, Is.Not.Null);
			Assert.That(globalizationSection.UICulture, Is.EqualTo("fr-FR"));
		}

		[Test]
		public void UpdateCurrentVersion_Should_Save_Version_To_RoadkillSection()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			configManager.UpdateCurrentVersion("2.0");

			// Assert
			RoadkillSection section = configManager.Load();
			Assert.That(section.Version, Is.EqualTo("2.0"));
		}

		[Test]
		public void ResetInstalledState_Should_Set_Installed_To_False()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			configManager.ResetInstalledState();

			// Assert
			RoadkillSection section = configManager.Load();
			Assert.That(section.Installed, Is.False);
		}

		[Test]
		public void TestSaveWebConfig_Should_Return_Empty_String_For_Success()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			string result = configManager.TestSaveWebConfig();

			// Assert
			Assert.That(result, Is.EqualTo(""));
		}

		[Test]
		public void GetConfiguration_Should_Return_Configuration_For_Exe_File()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			Configuration config = configManager.GetConfiguration();

			// Assert
			Assert.That(config, Is.Not.Null);
			Assert.That(config.FilePath, Is.EqualTo(configFilePath));
		}

		[Test]
		public void WriteConfigForFormsAuth_Should_Add_FormsAuth_Section_And_AnonymousIdentification()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			configManager.WriteConfigForFormsAuth();

			// Assert
			Configuration config = configManager.GetConfiguration();
			AuthenticationSection authSection = config.GetSection("system.web/authentication") as AuthenticationSection;

			Assert.That(authSection, Is.Not.Null);
			Assert.That(authSection.Mode, Is.EqualTo(AuthenticationMode.Forms));
			Assert.That(authSection.Forms.LoginUrl, Is.EqualTo("~/User/Login"));

			AnonymousIdentificationSection anonSection = config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			Assert.That(anonSection.Enabled, Is.True);
		}

		[Test]
		public void WriteConfigForWindowsAuth_Should_Set_WindowsAuthMode_And_Disable_AnonymousIdentification()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			configManager.WriteConfigForWindowsAuth();

			// Assert
			Configuration config = configManager.GetConfiguration();
			AuthenticationSection authSection = config.GetSection("system.web/authentication") as AuthenticationSection;

			Assert.That(authSection, Is.Not.Null);
			Assert.That(authSection.Mode, Is.EqualTo(AuthenticationMode.Windows));
			Assert.That(authSection.Forms.LoginUrl, Is.EqualTo("login.aspx")); // login.aspx is the default for windows auth

			AnonymousIdentificationSection anonSection = config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			Assert.That(anonSection.Enabled, Is.False);
		}

		[Test]
		public void GetApplicationSettings_Should_Have_Correct_Key_Mappings_And_Values()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.AdminRoleName, Is.EqualTo("Admin-test"), "AdminRoleName");
			Assert.That(appSettings.AttachmentsRoutePath, Is.EqualTo("AttachmentsRoutePathTest"), "AttachmentsRoutePath"); 
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
			Assert.That(appSettings.LoggingTypes, Is.EqualTo("All"), "LoggingType");
			Assert.That(appSettings.LogErrorsOnly, Is.False, "LogErrorsOnly");
			Assert.That(appSettings.UseHtmlWhiteList, Is.EqualTo(false), "UseHtmlWhiteList");
			Assert.That(appSettings.UserServiceType, Is.EqualTo("DefaultUserManager-test"), "DefaultUserManager");
			Assert.That(appSettings.UseWindowsAuthentication, Is.False, "UseWindowsAuthentication");
		}

		[Test]
		public void GetApplicationSettings_Should_Use_Default_Values_When_Optional_Settings_Have_Missing_Values()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-optional-values.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.AttachmentsRoutePath, Is.EqualTo("Attachments"), "AttachmentsRoutePath");
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.SqlServer2005), "DatabaseType");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.False, "IgnoreSearchIndexErrors");
			Assert.That(appSettings.IsPublicSite, Is.True, "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo(""), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo(""), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo(""), "LdapUsername");
			Assert.That(appSettings.LoggingTypes, Is.EqualTo("None"), "LoggingType");
			Assert.That(appSettings.LogErrorsOnly, Is.True, "LoggingType");
			Assert.That(appSettings.UseHtmlWhiteList, Is.EqualTo(true), "UseHtmlWhiteList");
			Assert.That(appSettings.UserServiceType, Is.EqualTo(""), "DefaultUserManager");
		}

		[Test]
		public void GetApplicationSettings_Should_Find_Connection_Value_From_Connection_Setting()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
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
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			
			// Assert
		}

		[Test]
		public void RoadkillSection_Legacy_UserManagerType_Value_Is_Ignored()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-legacy-values.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.UserServiceType, Is.Null.Or.Empty, "UserManagerType [legacy test for userManagerType]");
		}

		[Test]
		public void RoadkillSection_Legacy_CacheValues_Are_Ignored()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-legacy-values.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
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
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.Sqlite), "DataStoreType [legacy test for databaseType]");
		}

		[Test]
		[Description("Tests the save from both the settings page and installation")]
		public void Save_Should_Persist_All_ApplicationSettings()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-empty.config");
			SettingsViewModel viewModel = new SettingsViewModel()
			{
				AdminRoleName = "admin role name",
				AttachmentsFolder = @"c:\AttachmentsFolder",
				UseObjectCache = true,
				UseBrowserCache = true,
				ConnectionString = "connection string",
				DataStoreTypeName = "MongoDB",
				EditorRoleName = "editor role name",
				LdapConnectionString = "ldap connection string",
				LdapUsername = "ldap username",
				LdapPassword = "ldap password",
				UseWindowsAuth = true,
				IsPublicSite = false,
				IgnoreSearchIndexErrors = false
			};

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			configManager.Save(viewModel);

			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.AdminRoleName, Is.EqualTo(viewModel.AdminRoleName), "AdminRoleName");
			Assert.That(appSettings.AttachmentsFolder, Is.EqualTo(viewModel.AttachmentsFolder), "AttachmentsFolder");
			Assert.That(appSettings.UseObjectCache, Is.EqualTo(viewModel.UseObjectCache), "UseObjectCache");
			Assert.That(appSettings.UseBrowserCache, Is.EqualTo(viewModel.UseBrowserCache), "UseBrowserCache");
			Assert.That(appSettings.ConnectionString, Is.EqualTo(viewModel.ConnectionString), "ConnectionStringName");
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.MongoDB), "DatabaseType");
			Assert.That(appSettings.EditorRoleName, Is.EqualTo(viewModel.EditorRoleName), "EditorRoleName");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.EqualTo(viewModel.IgnoreSearchIndexErrors), "IgnoreSearchIndexErrors");
			Assert.That(appSettings.IsPublicSite, Is.EqualTo(viewModel.IsPublicSite), "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo(viewModel.LdapConnectionString), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo(viewModel.LdapPassword), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo(viewModel.LdapUsername), "LdapUsername");
			Assert.That(appSettings.UseWindowsAuthentication, Is.EqualTo(viewModel.UseWindowsAuth), "UseWindowsAuthentication");
			Assert.That(appSettings.Installed, Is.True, "Installed");
		}

		private string GetConfigPath(string filename)
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration", "Configuration", "TestConfigs", filename);
		}
	}
}