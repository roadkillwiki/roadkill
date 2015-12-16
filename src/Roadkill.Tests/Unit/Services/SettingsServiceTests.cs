using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Services
{
	[TestFixture]
	[Category("Unit")]
	public class SettingsServiceTests
	{
		private MocksAndStubsContainer _container;

		private RepositoryMock _repository;
		private SettingsService _settingsService;
		private RepositoryFactoryMock _repositoryFactory;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_repositoryFactory = _container.RepositoryFactory;
			_repository = _container.Repository;
			_settingsService = _container.SettingsService;
		}

		[Test]
		public void getsitesettings_should_return_correct_settings()
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
		public void savesitesettings_should_save_all_values()
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

		[Test]
		public void savesitesettings_should_persist_all_values()
		{
			// Arrange
			ApplicationSettings appSettings = new ApplicationSettings();
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
			SettingsViewModel validConfigSettings = new SettingsViewModel()
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

			SettingsService settingsService = new SettingsService(_repositoryFactory, appSettings);

			// Act
			settingsService.SaveSiteSettings(validConfigSettings);

			// Assert
			SiteSettings actualSettings = settingsService.GetSiteSettings();

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
		}

		[Test]
		public void savesitesettings_should_rethrow_database_exception_with_context_of_error()
		{
			// Arrange
			_repository.ThrowSaveSiteSettingsException = true;

			// Act + Assert
			Assert.Throws<DatabaseException>(() => _settingsService.SaveSiteSettings(new SettingsViewModel()));
		}
	}
}
