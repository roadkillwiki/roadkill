using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
{
	[TestFixture]
	[Category("Unit")]
	public class SettingsViewModelTests
	{
		[Test]
		public void Constructor_Should_Convert_ApplicationSettings_And_SiteSettings_To_Properties()
		{
			// Arrange
			ApplicationSettings appSettings = new ApplicationSettings()
			{
				AdminRoleName = "admin role name",
				AttachmentsFolder = @"c:\AttachmentsFolder",
				UseObjectCache = true,
				UseBrowserCache = true,
				ConnectionString = "connection string",
				DatabaseName = "SqlServer2008",
				EditorRoleName = "editor role name",
				LdapConnectionString = "ldap connection string",
				LdapUsername = "ldap username",
				LdapPassword = "ldap password",
				UseWindowsAuthentication = true
			};

			SiteSettings siteSettings = new SiteSettings()
			{
				AllowedFileTypes = "jpg,png,gif",
				AllowUserSignup = true,
				IsRecaptchaEnabled = true,
				MarkupType = "markuptype",
				RecaptchaPrivateKey = "privatekey",
				RecaptchaPublicKey = "publickey",
				SiteName = "sitename",
				SiteUrl = "siteurl",
				Theme = "theme",
				OverwriteExistingFiles = true,
				HeadContent = "head content",
				MenuMarkup = "menu markup"
			};

			
			// Act
			SettingsViewModel model = new SettingsViewModel(appSettings, siteSettings);

			// Assert
			Assert.That(model.AdminRoleName, Is.EqualTo(appSettings.AdminRoleName));
			Assert.That(model.AttachmentsFolder, Is.EqualTo(appSettings.AttachmentsFolder));
			Assert.That(model.UseObjectCache, Is.EqualTo(appSettings.UseObjectCache));
			Assert.That(model.UseBrowserCache, Is.EqualTo(appSettings.UseBrowserCache));
			Assert.That(model.ConnectionString, Is.EqualTo(appSettings.ConnectionString));
			Assert.That(model.DatabaseName, Is.EqualTo(appSettings.DatabaseName));
			Assert.That(model.EditorRoleName, Is.EqualTo(appSettings.EditorRoleName));
			Assert.That(model.LdapConnectionString, Is.EqualTo(appSettings.LdapConnectionString));
			Assert.That(model.LdapUsername, Is.EqualTo(appSettings.LdapUsername));
			Assert.That(model.LdapPassword, Is.EqualTo(appSettings.LdapPassword));
			Assert.That(model.UseWindowsAuth, Is.EqualTo(appSettings.UseWindowsAuthentication));

			Assert.That(model.AllowedFileTypes, Is.EqualTo(siteSettings.AllowedFileTypes));
			Assert.That(model.AllowUserSignup, Is.EqualTo(siteSettings.AllowUserSignup));
			Assert.That(model.IsRecaptchaEnabled, Is.EqualTo(siteSettings.IsRecaptchaEnabled));
			Assert.That(model.MarkupType, Is.EqualTo(siteSettings.MarkupType));
			Assert.That(model.RecaptchaPrivateKey, Is.EqualTo(siteSettings.RecaptchaPrivateKey));
			Assert.That(model.RecaptchaPublicKey, Is.EqualTo(siteSettings.RecaptchaPublicKey));
			Assert.That(model.SiteName, Is.EqualTo(siteSettings.SiteName));
			Assert.That(model.SiteUrl, Is.EqualTo(siteSettings.SiteUrl));
			Assert.That(model.Theme, Is.EqualTo(siteSettings.Theme));
			Assert.That(model.OverwriteExistingFiles, Is.EqualTo(siteSettings.OverwriteExistingFiles));
			Assert.That(model.HeadContent, Is.EqualTo(siteSettings.HeadContent));
			Assert.That(model.MenuMarkup, Is.EqualTo(siteSettings.MenuMarkup));
		}

		[Test]
		public void Constructor_Should_Remove_Spaces_From_SiteSettings_Allow_File_Types()
		{
			// Arrange
			ApplicationSettings appSettings = new ApplicationSettings();
			SiteSettings siteSettings = new SiteSettings()
			{
				AllowedFileTypes = "jpg, png,      gif"
			};


			// Act
			SettingsViewModel model = new SettingsViewModel(appSettings, siteSettings);

			// Assert
			Assert.That(model.AllowedFileTypes, Is.EqualTo("jpg,png,gif"));
		}

		[Test]
		public void FillFromApplicationSettings_Should_Convert_ApplicationSettings_To_Properties()
		{
			// Arrange
			ApplicationSettings appSettings = new ApplicationSettings()
			{
				AdminRoleName = "admin role name",
				AttachmentsFolder = @"c:\AttachmentsFolder",
				UseObjectCache = true,
				UseBrowserCache = true,
				ConnectionString = "connection string",
				DatabaseName = "SqlServer2008",
				EditorRoleName = "editor role name",
				LdapConnectionString = "ldap connection string",
				LdapUsername = "ldap username",
				LdapPassword = "ldap password",
				UseWindowsAuthentication = true,
				IsPublicSite = false,
				IgnoreSearchIndexErrors = false
			};

			// Act
			SettingsViewModel model = new SettingsViewModel();
			model.FillFromApplicationSettings(appSettings);

			// Assert
			Assert.That(model.AdminRoleName, Is.EqualTo(appSettings.AdminRoleName));
			Assert.That(model.AttachmentsFolder, Is.EqualTo(appSettings.AttachmentsFolder));
			Assert.That(model.UseObjectCache, Is.EqualTo(appSettings.UseObjectCache));
			Assert.That(model.UseBrowserCache, Is.EqualTo(appSettings.UseBrowserCache));
			Assert.That(model.ConnectionString, Is.EqualTo(appSettings.ConnectionString));
			Assert.That(model.DatabaseName, Is.EqualTo(appSettings.DatabaseName));
			Assert.That(model.EditorRoleName, Is.EqualTo(appSettings.EditorRoleName));
			Assert.That(model.LdapConnectionString, Is.EqualTo(appSettings.LdapConnectionString));
			Assert.That(model.LdapUsername, Is.EqualTo(appSettings.LdapUsername));
			Assert.That(model.LdapPassword, Is.EqualTo(appSettings.LdapPassword));
			Assert.That(model.UseWindowsAuth, Is.EqualTo(appSettings.UseWindowsAuthentication));
			Assert.That(model.IsPublicSite, Is.EqualTo(appSettings.IsPublicSite));
			Assert.That(model.IgnoreSearchIndexErrors, Is.EqualTo(appSettings.IgnoreSearchIndexErrors));
		}

		[Test]
		public void SetSupportedDatabases_Should_Convert_RepositoryInfo_Objects_SelectList()
		{
			// Arrange
			var respositoryFactory = new RepositoryFactoryMock();
			List<RepositoryInfo> repositoryInfos = respositoryFactory.ListAll().ToList();
            SettingsViewModel model = new SettingsViewModel();

			// Act
			model.SetSupportedDatabases(repositoryInfos);

			// Assert
			Assert.That(model.DatabaseTypesAsSelectList.Count, Is.EqualTo(repositoryInfos.Count()));

			var firstItem = repositoryInfos[0];

			Assert.That(model.DatabaseTypesAsSelectList[0].Value, Is.EqualTo(firstItem.Id));
			Assert.That(model.DatabaseTypesAsSelectList[0].Text, Is.EqualTo(firstItem.Description));
		}

		[Test]
		public void MarkupTypesAvailable_Should_Contain_Known_Markups()
		{
			// Arrange
			SettingsViewModel model = new SettingsViewModel();

			// Act + Assert
			Assert.That(model.MarkupTypesAvailable, Contains.Item("Creole"));
			Assert.That(model.MarkupTypesAvailable, Contains.Item("Markdown"));
			Assert.That(model.MarkupTypesAvailable, Contains.Item("MediaWiki"));
		}

		[Test]
		public void Version_Should_Equal_ApplicationSettingsProductVersion()
		{
			// Arrange
			SettingsViewModel model = new SettingsViewModel();

			// Act + Assert
			Assert.That(model.Version, Is.EqualTo(ApplicationSettings.ProductVersion));
		}

		[Test]
		public void ThemesAvailable_Should_Scan_Themes_Directory()
		{
			// Arrange
			string themeDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
			if (Directory.Exists(themeDir))
				Directory.Delete(themeDir, true);

			Directory.CreateDirectory(Path.Combine(themeDir, "Theme1"));
			Directory.CreateDirectory(Path.Combine(themeDir, "Theme2"));
			Directory.CreateDirectory(Path.Combine(themeDir, "Theme3"));
			
			SettingsViewModel model = new SettingsViewModel();			

			// Act
			List<string> themesAvailable = model.ThemesAvailable.ToList();

			// Assert
			Assert.That(themesAvailable, Contains.Item("Theme1"));
			Assert.That(themesAvailable, Contains.Item("Theme2"));
			Assert.That(themesAvailable, Contains.Item("Theme3"));
		}
	}
}
