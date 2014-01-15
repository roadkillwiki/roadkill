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
		private ListCache _listCache;
		private PageViewModelCache _pageViewModelCache;
		private SiteCache _siteCache;
		private IRepository _repository;
		private IPluginFactory _pluginFactory;

		public CacheController(ApplicationSettings settings, UserServiceBase userService,
			SettingsService settingsService, IUserContext context,
			ListCache listCache, PageViewModelCache pageViewModelCache, SiteCache siteCache)
			: base(settings, userService, context, settingsService) 
		{
			_settingsService = settingsService;
			_listCache = listCache;
			_pageViewModelCache = pageViewModelCache;
			_siteCache = siteCache;
		}

		/// <summary>
		/// Displays all items in the cache
		/// </summary>
		/// <returns></returns>
		[ImportModelState]
		public ActionResult Index()
		{
			CacheViewModel viewModel = new CacheViewModel()
			{
				IsCacheEnabled = ApplicationSettings.UseObjectCache,
				PageKeys = _pageViewModelCache.GetAllKeys(),
				ListKeys = _listCache.GetAllKeys(),
				SiteKeys = _siteCache.GetAllKeys()
			};

			return View(viewModel);
		}

		/// <summary>
		/// Clears all items in the database caches (not sitecache)
		/// </summary>
		/// <param name="clear">If not empty, then signals the action to clear the cache</param>
		/// <returns></returns>
		[ExportModelState]
		[HttpPost]
		public ActionResult Clear()
		{
			_pageViewModelCache.RemoveAll();
			_listCache.RemoveAll();
			_siteCache.RemoveAll();
			TempData["CacheCleared"] = true;

			return RedirectToAction("Index");
		}
	}
}
