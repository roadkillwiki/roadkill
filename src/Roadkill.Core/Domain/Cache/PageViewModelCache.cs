using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.ViewModels;
using RoadkillLog = Roadkill.Core.Logging.Log;

namespace Roadkill.Core.Cache
{
	public class PageViewModelCache
	{
		/// <summary>
		/// The version number used for cache keys to indicate it's the latest version - currently 0.
		/// </summary>
		internal static readonly int LATEST_VERSION_NUMBER = 0;

		private ObjectCache _cache; 
		private ApplicationSettings _applicationSettings;

		public PageViewModelCache(ApplicationSettings settings, ObjectCache cache)
		{
			_applicationSettings = settings;
			_cache = cache;
		}

		public void Add(int id, PageViewModel item)
		{
			Add(id, LATEST_VERSION_NUMBER, item);
		}

		public void Add(int id, int version, PageViewModel item)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			if (!item.IsCacheable)
				return;

			string key = CacheKeys.PageViewModelKey(id, version);
			_cache.Add(key, item, new CacheItemPolicy());

			Log("Added key {0} to cache [Id={1}, Version{2}]", key, id, version);
		}

		public void UpdateHomePage(PageViewModel item)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			_cache.Remove(CacheKeys.HomepageKey());
			_cache.Add(CacheKeys.HomepageKey(), item, new CacheItemPolicy());
		}

		public PageViewModel GetHomePage()
		{
			if (!_applicationSettings.UseObjectCache)
				return null;

			Log("Get latest homepage");

			return _cache.Get(CacheKeys.HomepageKey()) as PageViewModel;
		}

		public PageViewModel Get(int id)
		{
			return Get(id, LATEST_VERSION_NUMBER);
		}

		public PageViewModel Get(int id, int version)
		{
			if (!_applicationSettings.UseObjectCache)
				return null;

			string key = CacheKeys.PageViewModelKey(id, version);
			Log("Get key {0} in cache [Id={1}, Version{2}]", key, id, version);

			return _cache.Get(key) as PageViewModel;
		}

		public void RemoveHomePage()
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			_cache.Remove(CacheKeys.HomepageKey());

			Log("Removed homepage from cache", CacheKeys.HomepageKey());
		}

		public void Remove(int id)
		{
			Remove(id, LATEST_VERSION_NUMBER);
		}

		public void Remove(int id, int version)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			string key = CacheKeys.PageViewModelKey(id, version);
			_cache.Remove(key);

			Log("Removed key '{0}' from cache", key);
		}

		public void RemoveAll()
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log("RemoveAll from cache");

			// No need to lock the cache as Remove doesn't throw an exception if the key doesn't exist
			IEnumerable<string> keys = GetAllKeys();
			foreach (string key in keys)
			{
				_cache.Remove(key);
			}
		}

		public IEnumerable<string> GetAllKeys()
		{
			return _cache.Where(x => x.Key.StartsWith(CacheKeys.PAGEVIEWMODEL_CACHE_PREFIX))
				.OrderBy(x => x.Key)
				.Select(x => x.Key);
		}

		private void Log(string format, params object[] args)
		{
			RoadkillLog.Information("PageViewModelCache: " + string.Format(format, args));
		}
	}
}
