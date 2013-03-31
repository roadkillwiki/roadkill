using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Cache
{
	public class PageSummaryCache
	{
		internal static MemoryCache _cache = MemoryCache.Default;
		private static readonly int _latestVersionNumber = 0;
		private ApplicationSettings _settings;

		public PageSummaryCache(ApplicationSettings settings)
		{
			_settings = settings;
		}

		public void Add(int id, PageSummary item)
		{
			Add(id, _latestVersionNumber, item);
		}

		public void Add(int id, int version, PageSummary item)
		{
			if (!_settings.UseObjectCache)
				return;

			string key = string.Format("{0}.{1}", id, version);
			_cache.Add(key, item, new CacheItemPolicy());

			Log.Information("PageSummaryCache: Added key {0} to cache [Id={1}, Version{2}]", key, id, version);
		}

		public void UpdateHomePage(PageSummary item)
		{
			if (!_settings.UseObjectCache)
				return;

			string key = "latesthomepage";
			_cache.Remove(key);
			_cache.Add(key, item, new CacheItemPolicy());
		}

		public PageSummary GetHomePage()
		{
			if (!_settings.UseObjectCache)
				return null;

			string key = "latesthomepage";
			Log.Information("PageSummaryCache: Get latest homepage");

			return _cache.Get(key) as PageSummary;
		}

		public PageSummary Get(int id)
		{
			return Get(id, _latestVersionNumber);
		}

		public PageSummary Get(int id, int version)
		{
			if (!_settings.UseObjectCache)
				return null;

			string key = string.Format("{0}.{1}", id, version);
			Log.Information("PageSummaryCache: Get key {0} in cache [Id={1}, Version{2}]", key, id, version);

			return _cache.Get(key) as PageSummary;
		}

		public void RemoveHomePage()
		{
			if (!_settings.UseObjectCache)
				return;

			string key = "latesthomepage";
			_cache.Remove(key);

			Log.Information("PageSummaryCache: Removed homepage from cache", key);
		}

		public void Remove(int id)
		{
			Remove(id, _latestVersionNumber);
		}

		public void Remove(int id, int version)
		{
			if (!_settings.UseObjectCache)
				return;

			string key = string.Format("{0}.{1}", id, version);
			_cache.Remove(key);

			Log.Information("PageSummaryCache: Removed key '{0}' from cache", key);
		}

		public void RemoveAll()
		{
			if (!_settings.UseObjectCache)
				return;

			Log.Information("PageSummaryCache: RemoveAll from cache");

			foreach (var item in _cache)
			{
				_cache.Remove(item.Key);
			}
		}

		public IEnumerable<string> GetAllKeys()
		{
			return _cache.Select(x => x.Key);
		}
	}
}
