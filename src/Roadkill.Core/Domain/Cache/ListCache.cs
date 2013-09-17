using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Mindscape.LightSpeed;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Cache
{
	public class ListCache
	{
		private ObjectCache _cache; 
		private ApplicationSettings _applicationSettings;

		public ListCache(ApplicationSettings settings, ObjectCache cache)
		{
			_applicationSettings = settings;
			_cache = cache;
		}

		// These menu methods should probably live in their own separate cache, or refactor this cache to
		// be a generalised cache.

		public void AddMenu(string html)
		{
			_cache.Add(CacheKeys.MENU, html, new CacheItemPolicy());
		}

		public void AddLoggedInMenu(string html)
		{
			_cache.Add(CacheKeys.LOGGEDINMENU, html, new CacheItemPolicy());
		}

		public void AddAdminMenu(string html)
		{
			_cache.Add(CacheKeys.ADMINMENU, html, new CacheItemPolicy());
		}

		public void RemoveMenuCacheItems()
		{
			_cache.Remove(CacheKeys.MENU);
			_cache.Remove(CacheKeys.LOGGEDINMENU);
			_cache.Remove(CacheKeys.ADMINMENU);
		}

		public void Add<T>(string key, IEnumerable<T> items)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("ListCache: Added {0} to cache", key);
			_cache.Add(key, items.ToList(), new CacheItemPolicy());
		}

		public List<T> Get<T>(string key)
		{
			Log.Information("ListCache: Retrieved {0} from cache", key);
			return _cache.Get(key) as List<T>;
		}

		public string GetMenu()
		{
			return _cache.Get(CacheKeys.MENU) as string;
		}

		public string GetLoggedInMenu()
		{
			return _cache.Get(CacheKeys.LOGGEDINMENU) as string;
		}

		public string GetAdminMenu()
		{
			return _cache.Get(CacheKeys.ADMINMENU) as string;
		}

		public void Remove(string key)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("ListCache: Removed {0} from cache", key);
			_cache.Remove(key);
		}

		public void RemoveAll()
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("ListCache: RemoveAll from cache");

			// No need to lock the cache as Remove doesn't throw an exception if the key doesn't exist
			IEnumerable<string> keys = _cache.Select(x => x.Key).ToList();
			foreach (string key in keys)
			{
				_cache.Remove(key);
			}
		}

		public IEnumerable<string> GetAllKeys()
		{
			return _cache.Where(x => !x.Key.StartsWith(CacheKeys.PageSummaryKeyPrefix()))
					.OrderBy(x => x.Key)
					.Select(x => x.Key);
		}
	}
}