using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Tests.Unit.StubsAndMocks;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit.Cache
{
	[TestFixture]
	[Category("Unit")]
	public class SiteCacheTests
	{
		[Test]
		public void AddMenu_Should_Cache_Html()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);

			// Act
			siteCache.AddMenu("some html");

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
			IEnumerable<string> keys = cache.Select(x => x.Key);
			Assert.That(keys, Contains.Item(CacheKeys.MenuKey()));
		}

		[Test]
		public void AddAdminMenu_Should_Cache_Html()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);

			// Act
			siteCache.AddAdminMenu("some html");

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
			IEnumerable<string> keys = cache.Select(x => x.Key);
			Assert.That(keys, Contains.Item(CacheKeys.AdminMenuKey()));
		}

		[Test]
		public void AddLoggedInMenu_Should_Cache_Html()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);

			// Act
			siteCache.AddLoggedInMenu("some html");

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
			IEnumerable<string> keys = cache.Select(x => x.Key);
			Assert.That(keys, Contains.Item(CacheKeys.LoggedInMenuKey()));
		}

		[Test]
		public void GetMenu_Should_Return_Correct_Html()
		{
			// Arrange
			string expectedHtml = "some html";

			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);
			siteCache.AddMenu(expectedHtml);

			// Act
			string actualHtml = siteCache.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void GetAdminMenu_Should_Return_Correct_Html()
		{
			// Arrange
			string expectedHtml = "some html";
			
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);
			siteCache.AddAdminMenu(expectedHtml);

			// Act
			string actualHtml = siteCache.GetAdminMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void GetLoggedInMenu_Should_Return_Correct_Html()
		{
			// Arrange
			string expectedHtml = "some html";
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);
			siteCache.AddLoggedInMenu(expectedHtml);

			// Act
			string actualHtml = siteCache.GetLoggedInMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void RemoveMenuCacheItems_Should_Clear_Cache_Items()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = true };

			SiteCache siteCache = new SiteCache(settings, cache);
			siteCache.AddMenu("menu html");
			siteCache.AddLoggedInMenu("logged in menu html");
			siteCache.AddAdminMenu("admin menu html");

			// Act
			siteCache.RemoveMenuCacheItems();

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(0));
		}

		[Test]
		public void UpdatePluginSettings_Should_Add_Plugin_Settings_ToCache()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = siteCache;
			plugin.Repository = new RepositoryMock();
			plugin.Settings.SetValue("foo", "bar");

			// Act
			siteCache.UpdatePluginSettings(plugin);

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
		}

		[Test]
		public void GetPluginSettings_Should_Return_Plugin_Settings()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);

			TextPluginStub plugin = new TextPluginStub("id1", "", "");
			plugin.PluginCache = siteCache;
			plugin.Repository = new RepositoryMock();
			plugin.Settings.SetValue("foo", "bar");

			TextPluginStub plugin2 = new TextPluginStub("id2", "", "");
			plugin2.PluginCache = siteCache;
			plugin2.Repository = new RepositoryMock();
			plugin2.Settings.SetValue("foo", "bar2");

			// Act
			PluginSettings pluginSettings = siteCache.GetPluginSettings(plugin);

			// Assert
			Assert.That(pluginSettings.Values.Count(), Is.EqualTo(1));
			Assert.That(pluginSettings.GetValue("foo"), Is.EqualTo("bar"));
		}

		[Test]
		public void RemovePluginSettings_Should_Remove_Plugin_Settings()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = siteCache;
			plugin.Repository = new RepositoryMock();
			plugin.Settings.SetValue("foo", "bar");

			// Act
			siteCache.RemovePluginSettings(plugin);

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(0));
		}

		[Test]
		public void RemoveAll_Should_Remove_SiteCache_Keys_Only()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			cache.Add("list.blah", "xyz", new CacheItemPolicy());
			ApplicationSettings settings = new ApplicationSettings();
			SiteCache siteCache = new SiteCache(settings, cache);

			siteCache.AddMenu("menu html");
			siteCache.AddLoggedInMenu("logged in menu html");
			siteCache.AddAdminMenu("admin menu html");

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = siteCache;
			plugin.Repository = new RepositoryMock();
			plugin.Settings.SetValue("foo", "bar");

			// Act
			siteCache.RemoveAll();

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
		}
	}
}
