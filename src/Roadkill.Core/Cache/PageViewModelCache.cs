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
	/// <summary>
	/// Contains a cache of all pages in Roadkill.
	/// </summary>
	public class PageViewModelCache
	{
		/// <summary>
		/// The version number used for cache keys to indicate it's the latest version - currently 0.
		/// </summary>
		internal static readonly int LATEST_VERSION_NUMBER = 0;

		private ObjectCache _cache; 
		private ApplicationSettings _applicationSettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="PageViewModelCache"/> class.
		/// </summary>
		/// <param name="settings">The application settings.</param>
		/// <param name="cache">The underlying OjectCache - a MemoryCache by default.</param>
		public PageViewModelCache(ApplicationSettings settings, ObjectCache cache)
		{
			_applicationSettings = settings;
			_cache = cache;
		}

		/// <summary>
		/// Adds an item to the cache.
		/// </summary>
		/// <param name="id">The page's Id.</param>
		/// <param name="item">The page.</param>
		public void Add(int id, PageViewModel item)
		{
			Add(id, LATEST_VERSION_NUMBER, item);
		}

		// <summary>
		/// Adds an item to the cache.
		/// </summary>
		/// <param name="id">The page's Id.</param>
		/// <param name="version">The pages content's version.</param>
		/// <param name="item">The page.</param>
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

		/// <summary>
		/// Updates the home page item in the cache.
		/// </summary>
		/// <param name="item">The updated homepage item.</param>
		public void UpdateHomePage(PageViewModel item)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			_cache.Remove(CacheKeys.HomepageKey());
			_cache.Add(CacheKeys.HomepageKey(), item, new CacheItemPolicy());
		}

		/// <summary>
		/// Retrieves the home page item from the cache, or null if it doesn't exist.
		/// </summary>
		/// <returns>The cached <see cref="PageViewModel"/> for the homepage; or null if it doesn't exist.</returns>
		public PageViewModel GetHomePage()
		{
			if (!_applicationSettings.UseObjectCache)
				return null;

			Log("Get latest homepage");

			return _cache.Get(CacheKeys.HomepageKey()) as PageViewModel;
		}

		/// <summary>
		/// Retrieves the page item from the cache, or null if it doesn't exist.
		/// </summary>
		/// <param name="id">The id of the page</param>
		/// <returns>The cached <see cref="PageViewModel"/>; or null if it doesn't exist.</returns>
		public PageViewModel Get(int id)
		{
			return Get(id, LATEST_VERSION_NUMBER);
		}

		/// <summary>
		/// Retrieves the page item for a specific version of the page from the cache, or null if it doesn't exist.
		/// </summary>
		/// <param name="id">The id of the page </param>
		/// <param name="version">The version of the page.</param>
		/// <returns>The cached <see cref="PageViewModel"/>; or null if it doesn't exist.</returns>
		public PageViewModel Get(int id, int version)
		{
			if (!_applicationSettings.UseObjectCache)
				return null;

			string key = CacheKeys.PageViewModelKey(id, version);
			Log("Get key {0} in cache [Id={1}, Version{2}]", key, id, version);

			return _cache.Get(key) as PageViewModel;
		}

		/// <summary>
		/// Removes the home page item from the cache if it exists.
		/// </summary>
		public void RemoveHomePage()
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			_cache.Remove(CacheKeys.HomepageKey());

			Log("Removed homepage from cache", CacheKeys.HomepageKey());
		}

		/// <summary>
		/// Removes the page item from the cache if it exists.
		/// </summary>
		/// <param name="id">The id of the page.</param>
		public void Remove(int id)
		{
			Remove(id, LATEST_VERSION_NUMBER);
		}

		/// <summary>
		/// Removes the page item from the cache if it exists.
		/// </summary>
		/// <param name="id">The id of the page.</param>
		/// <param name="version">The page content version.</param>
		public void Remove(int id, int version)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			string key = CacheKeys.PageViewModelKey(id, version);
			_cache.Remove(key);

			Log("Removed key '{0}' from cache", key);
		}

		/// <summary>
		/// Clears the underlying cache of the page items.
		/// </summary>
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

		/// <summary>
		/// Returns all keys from the underlying cache the <see cref="ListCache"/> manages
		/// </summary>
		/// <returns>A string list of the keys.</returns>
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
