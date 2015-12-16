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
		public void constructor_should_convert_applicationsettings_and_sitesettings_to_properties()
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
		public void constructor_should_remove_spaces_from_sitesettings_allow_file_types()
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
		public void fillfromapplicationsettings_should_convert_applicationsettings_to_properties()
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
		public void setsupporteddatabases_should_convert_repositoryinfo_objects_selectlist()
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
		public void markuptypesavailable_should_contain_known_markups()
		{
			// Arrange
			SettingsViewModel model = new SettingsViewModel();

			// Act + Assert
			Assert.That(model.MarkupTypesAvailable, Contains.Item("Creole"));
			Assert.That(model.MarkupTypesAvailable, Contains.Item("Markdown"));
			Assert.That(model.MarkupTypesAvailable, Contains.Item("MediaWiki"));
		}

		[Test]
		public void version_should_equal_applicationsettingsproductversion()
		{
			// Arrange
			SettingsViewModel model = new SettingsViewModel();

			// Act + Assert
			Assert.That(model.Version, Is.EqualTo(ApplicationSettings.ProductVersion));
		}

		[Test]
		public void themesavailable_should_scan_themes_directory()
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
