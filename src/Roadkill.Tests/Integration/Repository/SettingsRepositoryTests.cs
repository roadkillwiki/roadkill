using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Tests.Unit.StubsAndMocks;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Integration")]
	public abstract class SettingsRepositoryTests
	{
		private SiteCache _siteCache;
		protected abstract string InvalidConnectionString { get; }

		protected ISettingsRepository Repository;
		protected abstract string ConnectionString { get; }
		protected abstract ISettingsRepository GetRepository();
		protected abstract void Clearup();

		[SetUp]
		public void Setup()
		{
			// Setup the repository
			Repository = GetRepository();
			Clearup();

			_siteCache = new SiteCache(CacheMock.RoadkillCache);
		}

		[TearDown]
		public void TearDown()
		{
			Repository.Dispose();
		}

		[Test]
		public void install_should_clear_all_entities_and_create_site_settings()
		{
			//// Arrange

			//// Act
			//Repository.Install(ApplicationSettings.DatabaseName, ApplicationSettings.ConnectionString, false);

			//// Assert
			//Assert.That(Repository.AllPages().Count(), Is.EqualTo(0));
			//Assert.That(Repository.AllPageContents().Count(), Is.EqualTo(0));
			//Assert.That(Repository.FindAllAdmins().Count(), Is.EqualTo(0));
			//Assert.That(Repository.FindAllEditors().Count(), Is.EqualTo(0));
			//Assert.That(Repository.GetSiteSettings(), Is.Not.Null);
			Assert.Fail("TODO");
		}
		
		[Test]
		public void savesitesettings_and_getsitesettings()
		{
			// Arrange
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
			Repository.SaveSiteSettings(expectedSettings);
			SiteSettings actualSettings = Repository.GetSiteSettings();

			// Assert
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

		[Test]
		public void savepluginsettings_and_gettextpluginsettings()
		{
			// Arrange
			PluginSettings expectedSettings = new PluginSettings("mockplugin", "1.0");
			expectedSettings.SetValue("somekey1", "thevalue1");
			expectedSettings.SetValue("somekey2", "thevalue2");

			TextPluginStub plugin = new TextPluginStub(Repository, _siteCache);
			plugin.Settings.SetValue("somekey1", "thevalue1");
			plugin.Settings.SetValue("somekey2", "thevalue2");

			// Act
			Repository.SaveTextPluginSettings(plugin);
			PluginSettings actualSettings = Repository.GetTextPluginSettings(plugin.DatabaseId);

			// Assert
			Assert.That(actualSettings.GetValue("somekey1"), Is.EqualTo("thevalue1"));
			Assert.That(actualSettings.GetValue("somekey2"), Is.EqualTo("thevalue2"));
		}

		[Test]
		public void testconnection_with_valid_connection_string()
		{
			Assert.Fail("TODO");
			//// Arrange


			//// Act
			//Repository.TestConnection(ApplicationSettings.DatabaseName, ApplicationSettings.ConnectionString);

			//// Assert (no exception)
		}

		[Test]
		public void testconnection_with_invalid_connection_string()
		{
			Assert.Fail("TODO");
			//// [expectedexception] can't handle exception heirachies

			//// Arrange

			//try
			//{
			//	// Act
			//	// (MongoConnectionException is also thrown here)
			//	Repository.TestConnection(ApplicationSettings.DatabaseName, InvalidConnectionString);
			//}
			//catch (DbException)
			//{
			//	// Assert
			//	Assert.Pass();
			//}
			//catch (ArgumentException)
			//{
			//	Assert.Pass();
			//}
			//catch (Exception)
			//{
			//	Assert.Fail();
			//}
		}
	}
}
