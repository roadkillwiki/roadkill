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

namespace Roadkill.Core.Mvc.Controllers
{
	[AdminRequired]
	public class PluginSettingsController : ControllerBase
	{
		private IPluginFactory _pluginFactory;
		private IRepository _repository;

		public PluginSettingsController(ApplicationSettings settings, UserServiceBase userManager, IUserContext context, 
			SettingsService settingsService, IPluginFactory pluginFactory, IRepository repository)
			: base (settings, userManager, context, settingsService)
		{
			_pluginFactory = pluginFactory;
			_repository = repository;
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

			PluginViewModel summary = new PluginViewModel()
			{
				Id = plugin.Id,
				DatabaseId = plugin.DatabaseId,
				Name = plugin.Name,
				Description = plugin.Description,
			};

			// Try to load the settings from the database, or use the defaults
			PluginSettings dbSettings = _repository.GetTextPluginSettings(plugin);
			if (dbSettings != null)
				summary.SettingValues = new List<SettingValue>(dbSettings.Values);
			else
				summary.SettingValues = new List<SettingValue>(plugin.Settings.Values);

			return View(summary);
		}

		[HttpPost]
		public ActionResult Edit(PluginViewModel summary)
		{
			TextPlugin plugin = _pluginFactory.GetTextPlugin(summary.Id);
			if (plugin == null)
				return RedirectToAction("Index");

			// Update the plugin settings with the values from the summary
			foreach (SettingValue summaryValue in summary.SettingValues)
			{
				SettingValue pluginValue = plugin.Settings.Values.FirstOrDefault(x => x.Name == summaryValue.Name);
				if (pluginValue != null)
					pluginValue.Value = summaryValue.Value;
			}
			_repository.SaveTextPluginSettings(plugin);

			return RedirectToAction("Index");
		}
	}
}