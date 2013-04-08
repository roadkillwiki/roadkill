using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class SettingsManagerTests
	{
		private RepositoryMock _repository;
		private ApplicationSettings _settings;
		private SettingsManager _settingsManager;

		[SetUp]
		public void Setup()
		{
			_settings = new ApplicationSettings();
			_settings.Installed = true;

			_repository = new RepositoryMock();
			_settingsManager = new SettingsManager(_settings, _repository);
		}

		[Test]
		public void ClearPageTables_Should_Remove_All_Pages_And_Content()
		{
			// Arrange
			_repository.AddNewPage(new Page(), "test1", "test1", DateTime.UtcNow);
			_repository.AddNewPage(new Page(), "test2", "test2", DateTime.UtcNow);

			// Act
			_settingsManager.ClearPageTables();

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
			_settingsManager.ClearUserTable();

			// Assert
			Assert.That(_repository.FindAllAdmins().Count(), Is.EqualTo(0)); // need an allusers method
			Assert.That(_repository.FindAllEditors().Count(), Is.EqualTo(0));
		}

		[Test]
		public void CreateTables_Calls_Repository_Install()
		{
			// Arrange
			SettingsSummary summary = new SettingsSummary();
			summary.DataStoreTypeName = "SQLite";
			summary.ConnectionString = "Data Source=somefile.sqlite;";
			summary.UseObjectCache = true;


			// Act
			_settingsManager.CreateTables(summary);


			// Assert
			Assert.That(_repository.InstalledConnectionString, Is.EqualTo(summary.ConnectionString));
			Assert.That(_repository.InstalledDataStoreType, Is.EqualTo(DataStoreType.Sqlite));
			Assert.That(_repository.InstalledEnableCache, Is.EqualTo(summary.UseObjectCache));
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
			_repository.SiteSettings = expectedSettings;

			// Act
			SiteSettings actualSettings = _settingsManager.GetSiteSettings();

			// Assert
			Assert.That(actualSettings.Theme, Is.EqualTo(expectedSettings.Theme));
			Assert.That(actualSettings.SiteName, Is.EqualTo(expectedSettings.SiteName));
			Assert.That(actualSettings.SiteUrl, Is.EqualTo(expectedSettings.SiteUrl));
			Assert.That(actualSettings.RecaptchaPrivateKey, Is.EqualTo(expectedSettings.RecaptchaPrivateKey));
			Assert.That(actualSettings.RecaptchaPublicKey, Is.EqualTo(expectedSettings.RecaptchaPublicKey));
			Assert.That(actualSettings.MarkupType, Is.EqualTo(expectedSettings.MarkupType));
			Assert.That(actualSettings.IsRecaptchaEnabled, Is.EqualTo(expectedSettings.IsRecaptchaEnabled));
			Assert.That(actualSettings.AllowedFileTypes, Is.EqualTo(expectedSettings.AllowedFileTypes));
		}

		[Test]
		public void SaveSiteSettings_Should_Save_All_Values()
		{
			// Arrange
			SettingsSummary expectedSettings = new SettingsSummary();
			expectedSettings.Theme = "Mytheme";
			expectedSettings.SiteName = "Mysitename";
			expectedSettings.SiteUrl = "SiteUrl";
			expectedSettings.RecaptchaPrivateKey = "RecaptchaPrivateKey";
			expectedSettings.RecaptchaPublicKey = "RecaptchaPublicKey";
			expectedSettings.MarkupType = "MarkupType";
			expectedSettings.IsRecaptchaEnabled = true;
			expectedSettings.AllowedFileTypes = "AllowedFileTypes";

			// Act
			_settingsManager.SaveSiteSettings(expectedSettings);
			SiteSettings actualSettings = _settingsManager.GetSiteSettings();

			// Assert
			Assert.That(actualSettings.Theme, Is.EqualTo(expectedSettings.Theme));
			Assert.That(actualSettings.SiteName, Is.EqualTo(expectedSettings.SiteName));
			Assert.That(actualSettings.SiteUrl, Is.EqualTo(expectedSettings.SiteUrl));
			Assert.That(actualSettings.RecaptchaPrivateKey, Is.EqualTo(expectedSettings.RecaptchaPrivateKey));
			Assert.That(actualSettings.RecaptchaPublicKey, Is.EqualTo(expectedSettings.RecaptchaPublicKey));
			Assert.That(actualSettings.MarkupType, Is.EqualTo(expectedSettings.MarkupType));
			Assert.That(actualSettings.IsRecaptchaEnabled, Is.EqualTo(expectedSettings.IsRecaptchaEnabled));
			Assert.That(actualSettings.AllowedFileTypes, Is.EqualTo(expectedSettings.AllowedFileTypes));
		}
	}
}
