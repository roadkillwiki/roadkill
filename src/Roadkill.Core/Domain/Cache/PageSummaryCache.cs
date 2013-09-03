using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Cache
{
	public class PageSummaryCache
	{
		/// <summary>
		/// The version number used for cache keys to indicate it's the latest version - currently 0.
		/// </summary>
		internal static readonly int LATEST_VERSION_NUMBER = 0;

		private ObjectCache _cache; 
		private ApplicationSettings _applicationSettings;

		public PageSummaryCache(ApplicationSettings settings, ObjectCache cache)
		{
			_applicationSettings = settings;
			_cache = cache;
		}

		public void Add(int id, PageSummary item)
		{
			Add(id, LATEST_VERSION_NUMBER, item);
		}

		public void Add(int id, int version, PageSummary item)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			if (!item.IsCacheable)
				return;

			string key = CacheKeys.PageSummaryKey(id, version);
			_cache.Add(key, item, new CacheItemPolicy());

			Log.Information("PageSummaryCache: Added key {0} to cache [Id={1}, Version{2}]", key, id, version);
		}

		public void UpdateHomePage(PageSummary item)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			_cache.Remove(CacheKeys.HOMEPAGE);
			_cache.Add(CacheKeys.HOMEPAGE, item, new CacheItemPolicy());
		}

		public PageSummary GetHomePage()
		{
			if (!_applicationSettings.UseObjectCache)
				return null;

			Log.Information("PageSummaryCache: Get latest homepage");

			return _cache.Get(CacheKeys.HOMEPAGE) as PageSummary;
		}

		public PageSummary Get(int id)
		{
			return Get(id, LATEST_VERSION_NUMBER);
		}

		public PageSummary Get(int id, int version)
		{
			if (!_applicationSettings.UseObjectCache)
				return null;

			string key = CacheKeys.PageSummaryKey(id, version);
			Log.Information("PageSummaryCache: Get key {0} in cache [Id={1}, Version{2}]", key, id, version);

			return _cache.Get(key) as PageSummary;
		}

		public void RemoveHomePage()
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			_cache.Remove(CacheKeys.HOMEPAGE);

			Log.Information("PageSummaryCache: Removed homepage from cache", CacheKeys.HOMEPAGE);
		}

		public void Remove(int id)
		{
			Remove(id, LATEST_VERSION_NUMBER);
		}

		public void Remove(int id, int version)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			string key = CacheKeys.PageSummaryKey(id, version);
			_cache.Remove(key);

			Log.Information("PageSummaryCache: Removed key '{0}' from cache", key);
		}

		public void RemoveAll()
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("PageSummaryCache: RemoveAll from cache");

			// No need to lock the cache as Remove doesn't throw an exception if the key doesn't exist
			IEnumerable<string> keys = _cache.Select(x => x.Key).ToList();
			foreach (string key in keys)
			{
				_cache.Remove(key);
			}
		}

		public IEnumerable<string> GetAllKeys()
		{
			return _cache.Select(x => x.Key);
		}
	}
}
