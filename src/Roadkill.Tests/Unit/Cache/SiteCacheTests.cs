using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Tests.Unit.StubsAndMocks;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit.Cache
{
	[TestFixture]
	[Category("Unit")]
	public class SiteCacheTests
	{
		[Test]
		public void addmenu_should_cache_html()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			SiteCache siteCache = new SiteCache(cache);

			// Act
			siteCache.AddMenu("some html");

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
			IEnumerable<string> keys = cache.Select(x => x.Key);
			Assert.That(keys, Contains.Item(CacheKeys.MenuKey()));
		}

		[Test]
		public void addadminmenu_should_cache_html()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			SiteCache siteCache = new SiteCache(cache);

			// Act
			siteCache.AddAdminMenu("some html");

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
			IEnumerable<string> keys = cache.Select(x => x.Key);
			Assert.That(keys, Contains.Item(CacheKeys.AdminMenuKey()));
		}

		[Test]
		public void addloggedinmenu_should_cache_html()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			SiteCache siteCache = new SiteCache(cache);

			// Act
			siteCache.AddLoggedInMenu("some html");

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
			IEnumerable<string> keys = cache.Select(x => x.Key);
			Assert.That(keys, Contains.Item(CacheKeys.LoggedInMenuKey()));
		}

		[Test]
		public void getmenu_should_return_correct_html()
		{
			// Arrange
			string expectedHtml = "some html";

			CacheMock cache = new CacheMock();
			SiteCache siteCache = new SiteCache(cache);
			siteCache.AddMenu(expectedHtml);

			// Act
			string actualHtml = siteCache.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void getadminmenu_should_return_correct_html()
		{
			// Arrange
			string expectedHtml = "some html";
			
			CacheMock cache = new CacheMock();
			SiteCache siteCache = new SiteCache(cache);
			siteCache.AddAdminMenu(expectedHtml);

			// Act
			string actualHtml = siteCache.GetAdminMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void getloggedinmenu_should_return_correct_html()
		{
			// Arrange
			string expectedHtml = "some html";
			CacheMock cache = new CacheMock();
			SiteCache siteCache = new SiteCache(cache);
			siteCache.AddLoggedInMenu(expectedHtml);

			// Act
			string actualHtml = siteCache.GetLoggedInMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void removemenucacheitems_should_clear_cache_items()
		{
			// Arrange
			CacheMock cache = new CacheMock();

			SiteCache siteCache = new SiteCache(cache);
			siteCache.AddMenu("menu html");
			siteCache.AddLoggedInMenu("logged in menu html");
			siteCache.AddAdminMenu("admin menu html");

			// Act
			siteCache.RemoveMenuCacheItems();

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(0));
		}

		[Test]
		public void updatepluginsettings_should_add_plugin_settings_tocache()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			SiteCache siteCache = new SiteCache(cache);

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = siteCache;
			plugin.Repository = new SettingsRepositoryMock();
			plugin.Settings.SetValue("foo", "bar");

			// Act
			siteCache.UpdatePluginSettings(plugin);

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
		}

		[Test]
		public void getpluginsettings_should_return_plugin_settings()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			SiteCache siteCache = new SiteCache(cache);

			TextPluginStub plugin = new TextPluginStub("id1", "", "");
			plugin.PluginCache = siteCache;
			plugin.Repository = new SettingsRepositoryMock();
			plugin.Settings.SetValue("foo", "bar");

			TextPluginStub plugin2 = new TextPluginStub("id2", "", "");
			plugin2.PluginCache = siteCache;
			plugin2.Repository = new SettingsRepositoryMock();
			plugin2.Settings.SetValue("foo", "bar2");

			// Act
			PluginSettings pluginSettings = siteCache.GetPluginSettings(plugin);

			// Assert
			Assert.That(pluginSettings.Values.Count(), Is.EqualTo(1));
			Assert.That(pluginSettings.GetValue("foo"), Is.EqualTo("bar"));
		}

		[Test]
		public void removepluginsettings_should_remove_plugin_settings()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			SiteCache siteCache = new SiteCache(cache);

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = siteCache;
			plugin.Repository = new SettingsRepositoryMock();
			plugin.Settings.SetValue("foo", "bar");

			// Act
			siteCache.RemovePluginSettings(plugin);

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(0));
		}

		[Test]
		public void removeall_should_remove_sitecache_keys_only()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			cache.Add("list.blah", "xyz", new CacheItemPolicy());
			SiteCache siteCache = new SiteCache(cache);

			siteCache.AddMenu("menu html");
			siteCache.AddLoggedInMenu("logged in menu html");
			siteCache.AddAdminMenu("admin menu html");

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = siteCache;
			plugin.Repository = new SettingsRepositoryMock();
			plugin.Settings.SetValue("foo", "bar");

			// Act
			siteCache.RemoveAll();

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
		}
	}
}
