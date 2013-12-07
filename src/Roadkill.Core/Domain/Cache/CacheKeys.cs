using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Cache
{
	public class CacheKeys
	{
		/// <summary>'latesthomepage'</summary>
		public static readonly string HOMEPAGE = "latesthomepage";

		/// <summary>'{id}.{version}'</summary>
		public static readonly string PAGEVIEWMODEL_FORMAT = "{id}.{version}";

		/// <summary>"allpages.with.content"</summary>
		public static readonly string ALLPAGES_CONTENT = "allpages.with.content";

		/// <summary>"allpages"</summary>
		public static readonly string ALLPAGES = "allpages";

		/// <summary>"allpages.createdby.{username}"</summary>
		public static readonly string ALLPAGES_CREATEDBY = "allpages.createdby.{username}";

		/// <summary>"alltags"</summary>
		public static readonly string ALLTAGS = "alltags";

		/// <summary>"pagesbytag.{tag}"</summary>
		public static readonly string PAGES_BY_TAG = "pagesbytag.{tag}";

		/// <summary>"menu"</summary>
		public static readonly string MENU = "menu";

		/// <summary>"loggedinmenu"</summary>
		public static readonly string LOGGEDINMENU = "loggedinmenu";

		/// <summary>"adminmenu"</summary>
		public static readonly string ADMINMENU = "adminmenu";

		/// <summary>"pluginsettings.{type}.{id}"</summary>
		public static readonly string PLUGIN_SETTINGS = "pluginsettings.{type}.{id}";

		/// <summary>"pageviewmodel.."</summary>
		public static readonly string PAGEVIEWMODEL_CACHE_PREFIX = "page.";

		/// <summary>"list."</summary>
		public static readonly string LIST_CACHE_PREFIX = "list.";

		/// <summary>"site."</summary>
		public static readonly string SITE_CACHE_PREFIX = "site.";

		public static string PageViewModelKey(int id, int version)
		{
			string key = LIST_CACHE_PREFIX + PAGEVIEWMODEL_FORMAT;
			key = key.Replace("{id}", id.ToString());
			key = key.Replace("{version}", version.ToString());

			return key;
		}

		public static string AllPagesCreatedByKey(string username)
		{
			string key = LIST_CACHE_PREFIX + ALLPAGES_CREATEDBY;
			key = key.Replace("{username}", username);

			return key;
		}

		public static string PagesByTagKey(string tag)
		{
			string key = LIST_CACHE_PREFIX + PAGES_BY_TAG;
			key = key.Replace("{tag}", tag);

			return key;
		}

		public static string PluginSettingsKey(TextPlugin plugin)
		{
			string key = SITE_CACHE_PREFIX + PLUGIN_SETTINGS;
			key = key.Replace("{type}", plugin.GetType().Name);
			key = key.Replace("{id}", plugin.Id);

			return key;
		}

		public static string MenuKey()
		{
			return SITE_CACHE_PREFIX + MENU;
		}

		public static string LoggedInMenuKey()
		{
			return SITE_CACHE_PREFIX + LOGGEDINMENU;
		}

		public static string AdminMenuKey()
		{
			return SITE_CACHE_PREFIX + ADMINMENU;
		}
	}
}
