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
	/// <summary>
	/// Caches all IEnumerables in Roadkill.
	/// </summary>
	public class ListCache
	{
		private ObjectCache _cache; 
		private ApplicationSettings _applicationSettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="ListCache"/> class.
		/// </summary>
		/// <param name="settings">The application settings.</param>
		/// <param name="cache">The underlying OjectCache - a MemoryCache by default.</param>
		public ListCache(ApplicationSettings settings, ObjectCache cache)
		{
			_applicationSettings = settings;
			_cache = cache;
		}

		/// <summary>
		/// Adds the list to the cache using the specified key.
		/// </summary>
		/// <typeparam name="T">The list type</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="items">The list to add.</param>
		public void Add<T>(string key, IEnumerable<T> items)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			if (!key.StartsWith(CacheKeys.LIST_CACHE_PREFIX))
				key = CacheKeys.LIST_CACHE_PREFIX + key;

			Log.Information("ListCache: Added {0} to cache", key);
			_cache.Add(key, items.ToList(), new CacheItemPolicy());
		}

		/// <summary>
		/// Gets the list from the cache using the specified cache key.
		/// </summary>
		/// <typeparam name="T">The type of the list</typeparam>
		/// <param name="key">The key.</param>
		/// <returns>The list from the cache, or null if it doesn't exist</returns>
		public List<T> Get<T>(string key)
		{
			Log.Information("ListCache: Retrieved {0} from cache", key);
			return _cache.Get(CacheKeys.LIST_CACHE_PREFIX + key) as List<T>;
		}

		/// <summary>
		/// Removes the item from the key using the specified key. If the item does not 
		/// exist no action occurs.
		/// </summary>
		/// <param name="key">The cache key</param>
		public void Remove(string key)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("ListCache: Removed {0} from cache", key);
			_cache.Remove(CacheKeys.LIST_CACHE_PREFIX + key);
		}

		/// <summary>
		/// Clears the underlying cache of the items the <see cref="ListCache"/> manages.
		/// </summary>
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

		/// <summary>
		/// Returns all keys from the underlying cache the <see cref="ListCache"/> manages
		/// </summary>
		/// <returns>A string list of the keys.</returns>
		public IEnumerable<string> GetAllKeys()
		{
			return _cache.Where(x => x.Key.StartsWith(CacheKeys.LIST_CACHE_PREFIX))
					.OrderBy(x => x.Key)
					.Select(x => x.Key);
		}
	}
}