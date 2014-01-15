using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Cache
{
	/// <summary>
	/// Contains all cache keys used by the Roadkill MemoryCache (or custom cache implementation)
	/// </summary>
	public class CacheKeys
	{
		/// <summary>'latesthomepage'</summary>
		private static readonly string HOMEPAGE = "latesthomepage";

		/// <summary>'{id}.{version}'</summary>
		private static readonly string PAGEVIEWMODEL_FORMAT = "{id}.{version}";

		/// <summary>"allpages.with.content"</summary>
		private static readonly string ALLPAGES_WITH_CONTENT = "allpages.with.content";

		/// <summary>"allpages"</summary>
		private static readonly string ALLPAGES = "allpages";

		/// <summary>"allpages.createdby.{username}"</summary>
		private static readonly string ALLPAGES_CREATEDBY = "allpages.createdby.{username}";

		/// <summary>"alltags"</summary>
		private static readonly string ALLTAGS = "alltags";

		/// <summary>"pagesbytag.{tag}"</summary>
		private static readonly string PAGES_BY_TAG = "pagesbytag.{tag}";

		/// <summary>"menu"</summary>
		private static readonly string MENU = "menu";

		/// <summary>"loggedinmenu"</summary>
		private static readonly string LOGGEDINMENU = "loggedinmenu";

		/// <summary>"adminmenu"</summary>
		private static readonly string ADMINMENU = "adminmenu";

		/// <summary>"pluginsettings.{type}.{id}"</summary>
		private static readonly string PLUGIN_SETTINGS = "pluginsettings.{type}.{id}";

		/// <summary>"pageviewmodel.."</summary>
		internal static readonly string PAGEVIEWMODEL_CACHE_PREFIX = "page.";

		/// <summary>"list."</summary>
		internal static readonly string LIST_CACHE_PREFIX = "list.";

		/// <summary>"site."</summary>
		internal static readonly string SITE_CACHE_PREFIX = "site.";


		/// <summary>
		/// Gets the cache key for a page, using its id and version.
		/// </summary>
		/// <param name="id">The page ID.</param>
		/// <param name="version">The page version.</param>
		/// <returns>The cache key.</returns>
		public static string PageViewModelKey(int id, int version)
		{
			string key = PAGEVIEWMODEL_CACHE_PREFIX + PAGEVIEWMODEL_FORMAT;
			key = key.Replace("{id}", id.ToString());
			key = key.Replace("{version}", version.ToString());

			return key;
		}

		/// <summary>
		/// Gets the cache key for the "alls the pages created by xyz" page.
		/// </summary>
		/// <param name="username">The created by username.</param>
		/// <returns>The cache key.</returns>
		public static string AllPagesCreatedByKey(string username)
		{
			string key = LIST_CACHE_PREFIX + ALLPAGES_CREATEDBY;
			key = key.Replace("{username}", username);

			return key;
		}

		/// <summary>
		/// Gets the cache key for the "pages created by tag xyz" page.
		/// </summary>
		/// <param name="tag">The tag name.</param>
		/// <returns>The cache key.</returns>
		public static string PagesByTagKey(string tag)
		{
			string key = LIST_CACHE_PREFIX + PAGES_BY_TAG;
			key = key.Replace("{tag}", tag);

			return key;
		}

		/// <summary>
		/// Gets the cache key for a plugin's settings.
		/// </summary>
		/// <param name="plugin">The plugin. - its Id and Name is used in the cache key.</param>
		/// <returns>The cache key.</returns>
		public static string PluginSettingsKey(TextPlugin plugin)
		{
			string key = SITE_CACHE_PREFIX + PLUGIN_SETTINGS;
			key = key.Replace("{type}", plugin.GetType().Name);
			key = key.Replace("{id}", plugin.Id);

			return key;
		}

		/// <summary>
		/// Gets the cache key for the homepage.
		/// </summary>
		/// <returns>The cache key.</returns>
		public static string HomepageKey()
		{
			return PAGEVIEWMODEL_CACHE_PREFIX + HOMEPAGE;
		}

		/// <summary>
		/// Gets the cache key for the navigation menu
		/// </summary>
		/// <returns>The cache key.</returns>
		public static string MenuKey()
		{
			return SITE_CACHE_PREFIX + MENU;
		}

		/// <summary>
		/// Gets the cache key for the navigation menu when you're logged in.
		/// </summary>
		/// <returns>The cache key.</returns>
		public static string LoggedInMenuKey()
		{
			return SITE_CACHE_PREFIX + LOGGEDINMENU;
		}

		/// <summary>
		/// Gets the cache key for the navigation menu when you're logged in as an admin.
		/// </summary>
		/// <returns>The cache key.</returns>
		public static string AdminMenuKey()
		{
			return SITE_CACHE_PREFIX + ADMINMENU;
		}

		/// <summary>
		/// Gets the cache key for the list with the given key.
		/// </summary>
		/// <param name="key">The name of the list.</param>
		/// <returns>The cache key.</returns>
		public static string ListCacheKey(string key)
		{
			return LIST_CACHE_PREFIX + key;
		}

		/// <summary>
		/// Gets the cache key for the list of pages.
		/// </summary>
		/// <returns>The cache key.</returns>
		public static string AllPages()
		{
			return LIST_CACHE_PREFIX + ALLPAGES;
		}

		/// <summary>
		/// Gets the cache key for the list of tags.
		/// </summary>
		/// <returns>The cache key.</returns>
		public static string AllTags()
		{
			return LIST_CACHE_PREFIX + ALLTAGS;
		}

		/// <summary>
		/// Gets the cache key for the list of pages which also contain their markup contents.
		/// </summary>
		/// <returns>The cache key.</returns>
		public static string AllPagesWithContent()
		{
			return LIST_CACHE_PREFIX + ALLPAGES_WITH_CONTENT;
		}
	}
}
