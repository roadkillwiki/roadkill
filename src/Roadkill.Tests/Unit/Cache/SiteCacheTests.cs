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
			Assert.That(keys, Contains.Item(CacheKeys.MENU));
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
			Assert.That(keys, Contains.Item(CacheKeys.ADMINMENU));
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
			Assert.That(keys, Contains.Item(CacheKeys.LOGGEDINMENU));
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
	}
}
