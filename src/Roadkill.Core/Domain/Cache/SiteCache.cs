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
			_cache.Remove(CacheKeys.MENU);
			_cache.Remove(CacheKeys.LOGGEDINMENU);
			_cache.Remove(CacheKeys.ADMINMENU);
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

		public PluginSettings GetPluginSettings(TextPlugin plugin)
		{
			return _cache.Get(CacheKeys.PluginSettingsKey(plugin)) as PluginSettings;
		}
	}
}