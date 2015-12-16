using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Services
{
	[TestFixture]
	[Category("Unit")]
	public class InstallationServiceTests
	{
		private MocksAndStubsContainer _container;
		private RepositoryFactoryMock _repositoryFactory;
		private RepositoryMock _repository;
		private InstallerRepositoryMock _installerRepository;

		private InstallationService _installationService;
		private UserServiceMock _userService;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
			_repositoryFactory = _container.RepositoryFactory;
            _repository = _repositoryFactory.Repository;
			_installerRepository = _repositoryFactory.InstallerRepository;

			_userService = new UserServiceMock();
			_installationService = new InstallationService(_repositoryFactory, "SQLServer2008", "bar", _userService);
		}

		[Test]
		public void addadminuser_should_add_new_admin_user_via_userservice()
		{
			// Arrange

			// Act
			_installationService.AddAdminUser("email", "password");

			// Assert
			User newUser = _userService.Users.FirstOrDefault();
			Assert.That(newUser, Is.Not.Null);
			Assert.That(newUser.Email, Is.EqualTo("email"));
			Assert.True(newUser.IsAdmin);
			Assert.That(newUser.Password, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void clearusertable_should_remove_all_users_via_repository()
		{
			// Arrange
			_repository.Users.Add(new User());
			_repository.Users.Add(new User());
			_repository.Users.Add(new User());

			// Act
			_installationService.ClearUserTable();

			// Assert
			Assert.That(_repository.Users.Count, Is.EqualTo(0));
        }

		[Test]
		public void createtables_should_install_via_repository()
		{
			// Arrange

			// Act
			_installationService.CreateTables();

			// Assert
			Assert.True(_installerRepository.Installed);
		}

		[Test]
		public void createtables_should_rethrow_database_exception_with_context_of_error()
		{
			// Arrange
			_installerRepository.ThrowInstallException = true;

			// Act + Assert
			Assert.Throws<DatabaseException>(() => _installationService.CreateTables());
		}

		[Test]
		public void getsupporteddatabases_should_return_repository_infoobjects_from_factory()
		{
			// Arrange
			int expectedCount = _repositoryFactory.ListAll().Count();

			// Act
			IEnumerable<RepositoryInfo> databases = _installationService.GetSupportedDatabases();

			// Assert
			Assert.That(databases.Count(), Is.EqualTo(expectedCount));
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
			_installationService.SaveSiteSettings(expectedSettings);
			SiteSettings actualSettings = _repository.GetSiteSettings();

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
		public void savesitesettings_should_rethrow_database_exception_with_context_of_error()
		{
			// Arrange
			_repository.ThrowSaveSiteSettingsException = true;

			// Act + Assert
			Assert.Throws<DatabaseException>(() => _installationService.SaveSiteSettings(new SettingsViewModel()));
		}
	}
}
