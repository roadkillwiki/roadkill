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

		public void Add<T>(string key, IEnumerable<T> items)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			if (!key.StartsWith(CacheKeys.LIST_CACHE_PREFIX))
				key = CacheKeys.LIST_CACHE_PREFIX + key;

			Log.Information("ListCache: Added {0} to cache", key);
			_cache.Add(key, items.ToList(), new CacheItemPolicy());
		}

		public List<T> Get<T>(string key)
		{
			Log.Information("ListCache: Retrieved {0} from cache", key);
			return _cache.Get(CacheKeys.LIST_CACHE_PREFIX + key) as List<T>;
		}

		public void Remove(string key)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("ListCache: Removed {0} from cache", key);
			_cache.Remove(CacheKeys.LIST_CACHE_PREFIX + key);
		}

		public void RemoveAll()
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("ListCache: RemoveAll from cache");

			// No need to lock the cache as Remove doesn't throw an exception if the key doesn't exist
			IEnumerable<string> keys = GetAllKeys();
			foreach (string key in keys)
			{
				_cache.Remove(key);
			}
		}

		public IEnumerable<string> GetAllKeys()
		{
			return _cache.Where(x => x.Key.StartsWith(CacheKeys.LIST_CACHE_PREFIX))
					.OrderBy(x => x.Key)
					.Select(x => x.Key);
		}
	}
}