using System;
using System.Collections.Generic;
using System.Linq;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Schema;
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

		private InstallationService _installationService;
		private InstallerRepositoryMock _installerRepository;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_installerRepository = _container.InstallerRepository;
			_installationService = _container.InstallationService;
		}

		[Test]
		public void install_should_create_schema_add_new_admin_user_and_save_settings()
		{
			// Arrange
			var expectedModel = new SettingsViewModel()
			{
				ConnectionString = "connection string",
				DatabaseName = "MongoDb",

				AllowedFileTypes = "AllowedFileTypes",
				Theme = "Mytheme",
				SiteName = "Mysitename",
				SiteUrl = "SiteUrl",
				RecaptchaPrivateKey = "RecaptchaPrivateKey",
				RecaptchaPublicKey = "RecaptchaPublicKey",
				MarkupType = "MarkupType",
				IsRecaptchaEnabled = true,
				OverwriteExistingFiles = true,
				HeadContent = "some head content",
				MenuMarkup = "some menu markup",
			};

			// Act
			_installationService.Install(expectedModel);

			// Assert
			Assert.That(_installerRepository.AddAdminUserCalled, Is.True);
			Assert.That(_installerRepository.SaveSettingsCalled, Is.True);
			Assert.That(_installerRepository.CreateSchemaCalled, Is.True);

			Assert.That(_installerRepository.ConnectionString, Is.EqualTo(expectedModel.ConnectionString));
			Assert.That(_installerRepository.DatabaseName, Is.EqualTo(expectedModel.DatabaseName));

			SiteSettings siteSettings = _installerRepository.SiteSettings;
			Assert.That(siteSettings.AllowedFileTypes, Is.EqualTo(expectedModel.AllowedFileTypes));
			Assert.That(siteSettings.Theme, Is.EqualTo(expectedModel.Theme));
			Assert.That(siteSettings.SiteName, Is.EqualTo(expectedModel.SiteName));
			Assert.That(siteSettings.SiteUrl, Is.EqualTo(expectedModel.SiteUrl));
			Assert.That(siteSettings.RecaptchaPrivateKey, Is.EqualTo(expectedModel.RecaptchaPrivateKey));
			Assert.That(siteSettings.RecaptchaPublicKey, Is.EqualTo(expectedModel.RecaptchaPublicKey));
			Assert.That(siteSettings.MarkupType, Is.EqualTo(expectedModel.MarkupType));
			Assert.That(siteSettings.IsRecaptchaEnabled, Is.EqualTo(expectedModel.IsRecaptchaEnabled));
			Assert.That(siteSettings.OverwriteExistingFiles, Is.EqualTo(expectedModel.OverwriteExistingFiles));
			Assert.That(siteSettings.HeadContent, Is.EqualTo(expectedModel.HeadContent));
			Assert.That(siteSettings.MenuMarkup, Is.EqualTo(expectedModel.MenuMarkup));
		}


		[Test]
		public void install_should_not_add_adminuser_when_windows_auth_is_true()
		{
			// Arrange
			var model = new SettingsViewModel();
			model.UseWindowsAuth = true;

			// Act
			_installationService.Install(model);

			// Assert
			Assert.That(_installerRepository.AddAdminUserCalled, Is.False);
		}

		[Test]
		public void getsupporteddatabases_should_return_repository_infoobjects_from_factory()
		{
			// Arrange
			int expectedCount = 4;

			// Act
			IEnumerable<RepositoryInfo> databases = _installationService.GetSupportedDatabases();

			// Assert
			Assert.That(databases.Count(), Is.EqualTo(expectedCount));
		}

		[Test]
		[TestCase("PostGres", "my-postgres-connection-string", DataProvider.PostgreSql9, typeof(PostgresSchema))]
		[TestCase("Mysql", "myql-connection-string", DataProvider.MySql5, typeof(MySqlSchema))]
		[TestCase("sqlserver", "my-sqlserver-connection-string", DataProvider.SqlServer2008, typeof(SqlServerSchema))]
		[TestCase("anything", "connection-string", DataProvider.SqlServer2008, typeof(SqlServerSchema))]
		public void GetRepository_should_return_correct_lightspeedrepository(string provider, string connectionString, DataProvider expectedProvider, Type expectedSchemaType)
		{
			// Arrange + Act
			IInstallerRepository installerRepository = _installationService.GetRepository(provider, connectionString);

			// Assert
			LightSpeedInstallerRepository lightSpeedInstallerRepository = installerRepository as LightSpeedInstallerRepository;
			Assert.That(lightSpeedInstallerRepository, Is.Not.Null);
			Assert.That(lightSpeedInstallerRepository.ConnectionString, Is.EqualTo(connectionString));
			Assert.That(lightSpeedInstallerRepository.DataProvider, Is.EqualTo(expectedProvider));
			Assert.That(lightSpeedInstallerRepository.Schema, Is.TypeOf(expectedSchemaType));
		}

		[Test]
		public void GetRepository_should_default_to_sqlserver_lightspeedrepository()
		{
			// Arrange
			string provider = "anything";
			string connectionString = "connection-string";
			Type expectedSchemaType = typeof(SqlServerSchema);

			// Act
			IInstallerRepository installerRepository = _installationService.GetRepository(provider, connectionString);

			// Assert
			LightSpeedInstallerRepository lightSpeedInstallerRepository = installerRepository as LightSpeedInstallerRepository;
			Assert.That(lightSpeedInstallerRepository, Is.Not.Null);
			Assert.That(lightSpeedInstallerRepository.ConnectionString, Is.EqualTo(connectionString));
			Assert.That(lightSpeedInstallerRepository.DataProvider, Is.EqualTo(DataProvider.SqlServer2008));
			Assert.That(lightSpeedInstallerRepository.Schema, Is.TypeOf(expectedSchemaType));
		}

		[Test]
		public void GetRepository_should_return_mongodb_repository()
		{
			// Arrange
			string provider = "MONGODB";
			string connectionString = "mongodb-connection-string";

			// Act
			IInstallerRepository installerRepository = _installationService.GetRepository(provider, connectionString);

			// Assert
			MongoDbInstallerRepository mongoDbInstallerRepository = installerRepository as MongoDbInstallerRepository;
			Assert.That(mongoDbInstallerRepository, Is.Not.Null);
			Assert.That(mongoDbInstallerRepository.ConnectionString, Is.EqualTo(connectionString));
		}
	}
}
