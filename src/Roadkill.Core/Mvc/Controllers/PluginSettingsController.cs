using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;
using PluginSettings = Roadkill.Core.Plugins.Settings;
using Roadkill.Core.Cache;

namespace Roadkill.Core.Mvc.Controllers
{
	[AdminRequired]
	public class PluginSettingsController : ControllerBase
	{
		private IPluginFactory _pluginFactory;
		private IRepository _repository;
		private SiteCache _siteCache;

		public PluginSettingsController(ApplicationSettings settings, UserServiceBase userManager, IUserContext context, 
			SettingsService settingsService, IPluginFactory pluginFactory, IRepository repository, SiteCache siteCache)
			: base (settings, userManager, context, settingsService)
		{
			_pluginFactory = pluginFactory;
			_repository = repository;
			_siteCache = siteCache;
		}

		public ActionResult Index()
		{
			IEnumerable<TextPlugin> plugins = _pluginFactory.GetTextPlugins().OrderBy(x => x.Name);
			List<PluginViewModel> modelList = new List<PluginViewModel>();

			foreach (TextPlugin plugin in plugins)
			{
				modelList.Add(new PluginViewModel(plugin));
			}

			return View(modelList);
		}

		public ActionResult Edit(string id)
		{
			// Guards
			if (string.IsNullOrEmpty(id))
				return RedirectToAction("Index");

			TextPlugin plugin = _pluginFactory.GetTextPlugin(id);
			if (plugin == null)
				return RedirectToAction("Index");

			PluginViewModel model = new PluginViewModel()
			{
				Id = plugin.Id,
				DatabaseId = plugin.DatabaseId,
				Name = plugin.Name,
				Description = plugin.Description,
			};

			// Try to load the settings from the database, fall back to defaults
			model.SettingValues = new List<SettingValue>(plugin.Settings.Values);
			model.IsEnabled = plugin.Settings.IsEnabled;

			return View(model);
		}

		[HttpPost]
		public ActionResult Edit(PluginViewModel model)
		{
			TextPlugin plugin = _pluginFactory.GetTextPlugin(model.Id);
			if (plugin == null)
				return RedirectToAction("Index");

			// Update the plugin settings with the values from the summary
			plugin.Settings.IsEnabled = model.IsEnabled;

			foreach (SettingValue summaryValue in model.SettingValues)
			{
				SettingValue pluginValue = plugin.Settings.Values.FirstOrDefault(x => x.Name == summaryValue.Name);
				if (pluginValue != null)
					pluginValue.Value = summaryValue.Value;
			}

			// Save and clear the cached settings
			_repository.SaveTextPluginSettings(plugin);
			_siteCache.RemovePluginSettings(plugin);

			return RedirectToAction("Index");
		}
	}
}