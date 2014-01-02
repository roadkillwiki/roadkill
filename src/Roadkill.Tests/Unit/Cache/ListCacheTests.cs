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
	public class ListCacheTests
	{
		[Test]
		public void Should_Add_Item()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = true };

			List<string> tagCacheItems = new List<string>() { "1", "2" };
			ListCache listCache = new ListCache(settings, cache);

			// Act
			listCache.Add("all.tags", tagCacheItems);

			// Assert
			Assert.That(cache.CacheItems.Count, Is.EqualTo(1));
		}

		[Test]
		public void Should_Get_Item()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = true };

			List<string> tagCacheItems = new List<string>() { "1", "2" };
			AddToCache(cache, "all.tags", tagCacheItems);
			
			ListCache listCache = new ListCache(settings, cache);

			// Act
			var tags = listCache.Get<string>("all.tags");

			// Assert
			Assert.That(tags, Is.EqualTo(tagCacheItems));
		}

		[Test]
		public void Should_GetAllKeys()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = true };

			List<string> tagCacheItems1 = new List<string>() { "1", "2" };
			List<string> tagCacheItems2 = new List<string>() { "a", "b" };
			ListCache listCache = new ListCache(settings, cache);

			// Act
			listCache.Add("all.tags1", tagCacheItems1);
			listCache.Add("all.tags2", tagCacheItems2);

			// Assert
			List<string> keys = listCache.GetAllKeys().ToList();
			Assert.That(keys, Contains.Item(CacheKeys.ListCacheKey("all.tags1")));
			Assert.That(keys, Contains.Item(CacheKeys.ListCacheKey("all.tags2")));
		}

		[Test]
		public void Should_Remove_Item()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = true };

			List<string> tagCacheItems = new List<string>() { "1", "2" };
			AddToCache(cache, "all.tags", tagCacheItems);

			ListCache listCache = new ListCache(settings, cache);

			// Act
			listCache.Remove("all.tags");

			// Assert
			var tags = cache.CacheItems.FirstOrDefault();
			Assert.That(tags, Is.Null);
		}

		[Test]
		public void Should_RemoveAll_Items()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = true };

			List<string> tagCacheItems1 = new List<string>() { "1", "2" };
			List<string> tagCacheItems2 = new List<string>() { "1", "2" };
			AddToCache(cache, "all.tags1", tagCacheItems1);
			AddToCache(cache, "all.tags2", tagCacheItems2);

			ListCache listCache = new ListCache(settings, cache);

			// Act
			listCache.RemoveAll();

			// Assert
			Assert.That(cache.CacheItems.Count, Is.EqualTo(0));
		}

		[Test]
		public void Should_Not_Add_If_Cache_Disabled()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = false };
			
			List<string> tagCacheItems = new List<string>() { "1", "2" };			
			ListCache listCache = new ListCache(settings, cache);

			// Act
			listCache.Add("all.tags", tagCacheItems);

			// Assert
			var tags = listCache.Get<string>("all.tags");
			Assert.That(tags, Is.Null);

			IEnumerable<string> keys = listCache.GetAllKeys();
			Assert.That(keys.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Should_Not_Remove_If_Cache_Disabled()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = false };

			List<string> tagCacheItems = new List<string>() { "1", "2" };
			AddToCache(cache, "all.tags", tagCacheItems);

			ListCache listCache = new ListCache(settings, cache);
			
			// Act
			listCache.Remove("all.tags");

			// Assert
			var tags = cache.CacheItems.FirstOrDefault();
			Assert.That(tags, Is.Not.Null);
		}

		[Test]
		public void Should_Not_RemoveAll_If_Cache_Disabled()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = false };

			List<string> tagCacheItems1 = new List<string>() { "1", "2" };
			List<string> tagCacheItems2 = new List<string>() { "1", "2" };
			AddToCache(cache, "all.tags1", tagCacheItems1);
			AddToCache(cache, "all.tags2", tagCacheItems2);

			ListCache listCache = new ListCache(settings, cache);

			// Act
			listCache.RemoveAll();

			// Assert
			var tags1 = cache.CacheItems.FirstOrDefault(x => x.Key.Contains("all.tags1"));
			var tags2 = cache.CacheItems.FirstOrDefault(x => x.Key.Contains("all.tags2"));

			Assert.That(tags1, Is.Not.Null);
			Assert.That(tags2, Is.Not.Null);
		}

		[Test]
		public void RemoveAll_Should_Remove_ListCache_Keys_Only()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			cache.Add("site.blah", "xyz", new CacheItemPolicy());

			ApplicationSettings settings = new ApplicationSettings() { UseObjectCache = true };

			List<string> tagCacheItems1 = new List<string>() { "1", "2" };
			List<string> tagCacheItems2 = new List<string>() { "1", "2" };
			AddToCache(cache, "all.tags1", tagCacheItems1);
			AddToCache(cache, "all.tags2", tagCacheItems2);

			ListCache listCache = new ListCache(settings, cache);

			// Act
			listCache.RemoveAll();

			// Assert
			Assert.That(cache.Count(), Is.EqualTo(1));
		}

		private void AddToCache(CacheMock cache, string key, object value)
		{
			cache.Add(CacheKeys.ListCacheKey(key), value, new CacheItemPolicy());
		}
	}
}
