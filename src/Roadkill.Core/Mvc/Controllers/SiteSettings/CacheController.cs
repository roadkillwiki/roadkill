using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using Ionic.Zip;
using Roadkill.Core.Localization;
using Roadkill.Core.Configuration;
using Roadkill.Core.Cache;
using Roadkill.Core.Services;
using Roadkill.Core.Import;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Logging;
using Roadkill.Core.Database.Export;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides functionality for the cache management for admins.
	/// </summary>
	/// <remarks>All actions in this controller require admin rights.</remarks>
	[AdminRequired]
	public class CacheController : ControllerBase
	{
		private SettingsService _settingsService;
		private PageService _pageService;
		private SearchService _searchService;
		private IWikiImporter _wikiImporter;
		private ListCache _listCache;
		private PageViewModelCache _pageViewModelCache;
		private SiteCache _siteCache;
		private IRepository _repository;
		private IPluginFactory _pluginFactory;

		public CacheController(ApplicationSettings settings, UserServiceBase userManager,
			SettingsService settingsService, PageService pageService, SearchService searchService, IUserContext context,
			ListCache listCache, PageViewModelCache pageViewModelCache, SiteCache siteCache, IWikiImporter wikiImporter, 
			IRepository repository, IPluginFactory pluginFactory)
			: base(settings, userManager, context, settingsService) 
		{
			_settingsService = settingsService;
			_pageService = pageService;
			_searchService = searchService;
			_listCache = listCache;
			_pageViewModelCache = pageViewModelCache;
			_siteCache = siteCache;
			_wikiImporter = wikiImporter;			
			_repository = repository;
			_pluginFactory = pluginFactory;
		}

		/// <summary>
		/// Displays all items in the cache
		/// </summary>
		/// <param name="clear">If not empty, then signals the action to clear the cache</param>
		/// <returns></returns>
		public ActionResult Index()
		{
			List<IEnumerable<string>> cacheKeys = new List<IEnumerable<string>>()
			{
				_pageViewModelCache.GetAllKeys(),
				_listCache.GetAllKeys()
			};

			return View(cacheKeys);
		}

		/// <summary>
		/// Clears all items in the database caches (not sitecache)
		/// </summary>
		/// <returns></returns>
		public ActionResult Clear()
		{
			_pageViewModelCache.RemoveAll();
			_listCache.RemoveAll();
			ViewData["CacheCleared"] = true;

			return RedirectToAction("Index");
		}
	}
}
