using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Mindscape.LightSpeed;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Core.Cache
{
	public class SiteCache : IPluginCache
	{
		private ObjectCache _cache; 
		private ApplicationSettings _applicationSettings;

		public SiteCache(ApplicationSettings settings, ObjectCache cache)
		{
			_applicationSettings = settings;
			_cache = cache;
		}

		public void AddMenu(string html)
		{
			_cache.Add(CacheKeys.MenuKey(), html, new CacheItemPolicy());
		}

		public void AddLoggedInMenu(string html)
		{
			_cache.Add(CacheKeys.LoggedInMenuKey(), html, new CacheItemPolicy());
		}

		public void AddAdminMenu(string html)
		{
			_cache.Add(CacheKeys.AdminMenuKey(), html, new CacheItemPolicy());
		}

		public void UpdatePluginSettings(TextPlugin plugin)
		{
			_cache.Remove(CacheKeys.PluginSettingsKey(plugin));
			_cache.Add(CacheKeys.PluginSettingsKey(plugin), plugin.Settings, new CacheItemPolicy());
		}

		public void RemovePluginSettings(TextPlugin plugin)
		{
			_cache.Remove(CacheKeys.PluginSettingsKey(plugin));
		}

		public void RemoveMenuCacheItems()
		{
			_cache.Remove(CacheKeys.MenuKey());
			_cache.Remove(CacheKeys.LoggedInMenuKey());
			_cache.Remove(CacheKeys.AdminMenuKey());
		}

		public void RemoveAll()
		{
			foreach (var key in GetAllKeys())
			{
				_cache.Remove(key);
			}
		}

		public string GetMenu()
		{
			return _cache.Get(CacheKeys.MenuKey()) as string;
		}

		public string GetLoggedInMenu()
		{
			return _cache.Get(CacheKeys.LoggedInMenuKey()) as string;
		}

		public string GetAdminMenu()
		{
			return _cache.Get(CacheKeys.AdminMenuKey()) as string;
		}

		public PluginSettings GetPluginSettings(TextPlugin plugin)
		{
			return _cache.Get(CacheKeys.PluginSettingsKey(plugin)) as PluginSettings;
		}

		public IEnumerable<string> GetAllKeys()
		{
			return _cache.Where(x => x.Key.StartsWith(CacheKeys.SITE_CACHE_PREFIX))
					.OrderBy(x => x.Key)
					.Select(x => x.Key);
		}
	}
}