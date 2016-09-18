using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Integration")]
	public abstract class InstallerRepositoryTests
	{
		protected abstract string InvalidConnectionString { get; }
		protected abstract string ConnectionString { get; }

		protected abstract IInstallerRepository GetRepository(string connectionString);
		protected abstract void Clearup();
		protected virtual void CheckDatabaseProcessIsRunning() { }
		protected abstract bool HasAdminUser();
		protected abstract bool HasEmptyTables();
		protected abstract SiteSettings GetSiteSettings();

		[SetUp]
		public void Setup()
		{
			// Setup the repository
			Clearup();
		}

		[Test]
		public void AddAdminUser_should_add_user()
		{
			// Arrange
			var repository = GetRepository(ConnectionString);
			string username = "admin";
			string email = "email@example.com";
			string password = "password";

			// Act
			repository.AddAdminUser(email, username, password);

			// Assert
			Assert.True(HasAdminUser());
		}

		[Test]
		public void CreateSchema_should_create_and_clear_all_tables()
		{
			// Arrange
			var repository = GetRepository(ConnectionString);

			// Act
			repository.CreateSchema();

			// Assert
			Assert.True(HasEmptyTables());
		}

		[Test]
		public void AddAdminUser_And_CreateSchema_should_throw_databaseexception_with_invalid_connection_string()
		{
			// Arrange 
			var repository = GetRepository(InvalidConnectionString);

			// Act Assert
			Assert.Throws<DatabaseException>(() => repository.AddAdminUser("", "", ""));
			Assert.Throws<DatabaseException>(() => repository.CreateSchema());
		}

		[Test]
		public void savesitesettings_and_getsitesettings()
		{
			// Arrange 
			var repository = GetRepository(ConnectionString);
			SiteSettings expectedSettings = new SiteSettings()
			{
				AllowedFileTypes = "exe, virus, trojan",
				AllowUserSignup = true,
				IsRecaptchaEnabled = true,
				MarkupType = "Test",
				RecaptchaPrivateKey = "RecaptchaPrivateKey",
				RecaptchaPublicKey = "RecaptchaPublicKey",
				SiteName = "NewSiteName",
				SiteUrl = "http://sitename",
				Theme = "newtheme"
			};

			// Act
			repository.SaveSettings(expectedSettings);

			// Assert
			SiteSettings actualSettings = GetSiteSettings();

			Assert.That(actualSettings.AllowedFileTypes, Is.EqualTo(expectedSettings.AllowedFileTypes));
			Assert.That(actualSettings.AllowUserSignup, Is.EqualTo(expectedSettings.AllowUserSignup));
			Assert.That(actualSettings.IsRecaptchaEnabled, Is.EqualTo(expectedSettings.IsRecaptchaEnabled));
			Assert.That(actualSettings.MarkupType, Is.EqualTo(expectedSettings.MarkupType));
			Assert.That(actualSettings.RecaptchaPrivateKey, Is.EqualTo(expectedSettings.RecaptchaPrivateKey));
			Assert.That(actualSettings.RecaptchaPublicKey, Is.EqualTo(expectedSettings.RecaptchaPublicKey));
			Assert.That(actualSettings.SiteName, Is.EqualTo(expectedSettings.SiteName));
			Assert.That(actualSettings.SiteUrl, Is.EqualTo(expectedSettings.SiteUrl));
			Assert.That(actualSettings.Theme, Is.EqualTo(expectedSettings.Theme));
		}
	}
}
