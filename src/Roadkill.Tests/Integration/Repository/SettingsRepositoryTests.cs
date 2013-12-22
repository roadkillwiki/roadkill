using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Tests.Unit.StubsAndMocks;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Unit")]
	public abstract class SettingsRepositoryTests : RepositoryTests
	{
		private SiteCache _siteCache;
		protected abstract string InvalidConnectionString { get; }

		[SetUp]
		public void Setup()
		{
			_siteCache = new SiteCache(ApplicationSettings, CacheMock.RoadkillCache);
		}

		[Test]
		public void Install_Should_Clear_All_Entities_And_Create_Site_Settings()
		{
			// Arrange

			// Act
			Repository.Install(ApplicationSettings.DataStoreType, ApplicationSettings.ConnectionString, false);

			// Assert
			Assert.That(Repository.AllPages().Count(), Is.EqualTo(0));
			Assert.That(Repository.AllPageContents().Count(), Is.EqualTo(0));
			Assert.That(Repository.FindAllAdmins().Count(), Is.EqualTo(0));
			Assert.That(Repository.FindAllEditors().Count(), Is.EqualTo(0));
			Assert.That(Repository.GetSiteSettings(), Is.Not.Null);
		}
		
		[Test]
		public void SaveSiteSettings_And_GetSiteSettings()
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
		public void SavePluginSettings_And_GetTextPluginSettings()
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
		public void TestConnection_With_Valid_Connection_String()
		{
			// Arrange


			// Act
			Repository.TestConnection(ApplicationSettings.DataStoreType, ApplicationSettings.ConnectionString);

			// Assert (no exception)
		}

		[Test]
		public void TestConnection_With_Invalid_Connection_String()
		{
			// [expectedexception] can't handle exception heirachies

			// Arrange

			try
			{
				// Act
				// (MongoConnectionException is also thrown here)
				Repository.TestConnection(ApplicationSettings.DataStoreType, InvalidConnectionString);
			}
			catch (DbException)
			{
				// Assert
				Assert.Pass();
			}
			catch (ArgumentException)
			{
				Assert.Pass();
			}
			catch (Exception)
			{
				Assert.Fail();
			}
		}
	}
}
