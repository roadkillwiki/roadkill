using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class SettingsServiceTests
	{
		private RepositoryMock _repository;
		private ApplicationSettings _settings;
		private SettingsService _settingsService;

		[SetUp]
		public void Setup()
		{
			_settings = new ApplicationSettings();
			_settings.Installed = true;

			_repository = new RepositoryMock();
			_settingsService = new SettingsService(_settings, _repository);
		}

		[Test]
		public void ClearPageTables_Should_Remove_All_Pages_And_Content()
		{
			// Arrange
			_repository.AddNewPage(new Page(), "test1", "test1", DateTime.UtcNow);
			_repository.AddNewPage(new Page(), "test2", "test2", DateTime.UtcNow);

			// Act
			_settingsService.ClearPageTables();

			// Assert
			Assert.That(_repository.AllPages().Count(), Is.EqualTo(0));
			Assert.That(_repository.AllPageContents().Count(), Is.EqualTo(0));
		}

		[Test]
		public void ClearUserTable_Should_Remove_All_Users()
		{
			// Arrange
			_repository.Users.Add(new User() { IsAdmin = true });
			_repository.Users.Add(new User() { IsAdmin = true });
			_repository.Users.Add(new User() { IsEditor = true });
			_repository.Users.Add(new User() { IsEditor = true });

			// Act
			_settingsService.ClearUserTable();

			// Assert
			Assert.That(_repository.FindAllAdmins().Count(), Is.EqualTo(0)); // need an allusers method
			Assert.That(_repository.FindAllEditors().Count(), Is.EqualTo(0));
		}

		[Test]
		public void CreateTables_Calls_Repository_Install()
		{
			// Arrange
			SettingsViewModel model = new SettingsViewModel();
			model.DataStoreTypeName = "SQLite";
			model.ConnectionString = "Data Source=somefile.sqlite;";
			model.UseObjectCache = true;

			// Act
			_settingsService.CreateTables(model);


			// Assert
			Assert.That(_repository.InstalledConnectionString, Is.EqualTo(model.ConnectionString));
			Assert.That(_repository.InstalledDataStoreType, Is.EqualTo(DataStoreType.Sqlite));
			Assert.That(_repository.InstalledEnableCache, Is.EqualTo(model.UseObjectCache));
		}

		[Test]
		public void GetSiteSettings_Should_Return_Correct_Settings()
		{
			// Arrange
			SiteSettings expectedSettings = new SiteSettings();
			expectedSettings.Theme = "Mytheme";
			expectedSettings.SiteName = "Mysitename";
			expectedSettings.SiteUrl = "SiteUrl";
			expectedSettings.RecaptchaPrivateKey = "RecaptchaPrivateKey";
			expectedSettings.RecaptchaPublicKey = "RecaptchaPublicKey";
			expectedSettings.MarkupType = "MarkupType";
			expectedSettings.IsRecaptchaEnabled = true;
			expectedSettings.AllowedFileTypes = "AllowedFileTypes";
			expectedSettings.OverwriteExistingFiles = true;
			expectedSettings.HeadContent = "some head content";
			expectedSettings.MenuMarkup = "some menu markup";
			_repository.SiteSettings = expectedSettings;

			// Act
			SiteSettings actualSettings = _settingsService.GetSiteSettings();

			// Assert
			Assert.That(actualSettings.Theme, Is.EqualTo(expectedSettings.Theme));
			Assert.That(actualSettings.SiteName, Is.EqualTo(expectedSettings.SiteName));
			Assert.That(actualSettings.SiteUrl, Is.EqualTo(expectedSettings.SiteUrl));
			Assert.That(actualSettings.RecaptchaPrivateKey, Is.EqualTo(expectedSettings.RecaptchaPrivateKey));
			Assert.That(actualSettings.RecaptchaPublicKey, Is.EqualTo(expectedSettings.RecaptchaPublicKey));
			Assert.That(actualSettings.MarkupType, Is.EqualTo(expectedSettings.MarkupType));
			Assert.That(actualSettings.IsRecaptchaEnabled, Is.EqualTo(expectedSettings.IsRecaptchaEnabled));
			Assert.That(actualSettings.AllowedFileTypes, Is.EqualTo(expectedSettings.AllowedFileTypes));
			Assert.That(actualSettings.OverwriteExistingFiles, Is.EqualTo(expectedSettings.OverwriteExistingFiles));
			Assert.That(actualSettings.HeadContent, Is.EqualTo(expectedSettings.HeadContent));
			Assert.That(actualSettings.MenuMarkup, Is.EqualTo(expectedSettings.MenuMarkup));
		}

		[Test]
		public void SaveSiteSettings_Should_Save_All_Values()
		{
			// Arrange
			SettingsViewModel expectedSettings = new SettingsViewModel();
			expectedSettings.AllowedFileTypes = "AllowedFileTypes";
			expectedSettings.Theme = "Mytheme";
			expectedSettings.SiteName = "Mysitename";
			expectedSettings.SiteUrl = "SiteUrl";
			expectedSettings.RecaptchaPrivateKey = "RecaptchaPrivateKey";
			expectedSettings.RecaptchaPublicKey = "RecaptchaPublicKey";
			expectedSettings.MarkupType = "MarkupType";
			expectedSettings.IsRecaptchaEnabled = true;
			expectedSettings.OverwriteExistingFiles = true;
			expectedSettings.HeadContent = "some head content";
			expectedSettings.MenuMarkup = "some menu markup";

			// Act
			_settingsService.SaveSiteSettings(expectedSettings);
			SiteSettings actualSettings = _settingsService.GetSiteSettings();

			// Assert
			Assert.That(actualSettings.AllowedFileTypes, Is.EqualTo(expectedSettings.AllowedFileTypes));
			Assert.That(actualSettings.Theme, Is.EqualTo(expectedSettings.Theme));
			Assert.That(actualSettings.SiteName, Is.EqualTo(expectedSettings.SiteName));
			Assert.That(actualSettings.SiteUrl, Is.EqualTo(expectedSettings.SiteUrl));
			Assert.That(actualSettings.RecaptchaPrivateKey, Is.EqualTo(expectedSettings.RecaptchaPrivateKey));
			Assert.That(actualSettings.RecaptchaPublicKey, Is.EqualTo(expectedSettings.RecaptchaPublicKey));
			Assert.That(actualSettings.MarkupType, Is.EqualTo(expectedSettings.MarkupType));
			Assert.That(actualSettings.IsRecaptchaEnabled, Is.EqualTo(expectedSettings.IsRecaptchaEnabled));
			Assert.That(actualSettings.OverwriteExistingFiles, Is.EqualTo(expectedSettings.OverwriteExistingFiles));
			Assert.That(actualSettings.HeadContent, Is.EqualTo(expectedSettings.HeadContent));
			Assert.That(actualSettings.MenuMarkup, Is.EqualTo(expectedSettings.MenuMarkup));
		}
	}
}
