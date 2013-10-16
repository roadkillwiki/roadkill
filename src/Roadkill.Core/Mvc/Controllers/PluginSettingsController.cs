using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Controllers
{
	[AdminRequired]
	public class PluginSettingsController : ControllerBase
	{
		private IPluginFactory _pluginFactory;

		public PluginSettingsController(ApplicationSettings settings, UserManagerBase userManager, IUserContext context, 
			SettingsManager siteSettingsManager, IPluginFactory pluginFactory) : base (settings, userManager, context, siteSettingsManager)
		{
			_pluginFactory = pluginFactory;
		}

		public ActionResult Index()
		{
			IEnumerable<TextPlugin> plugins = _pluginFactory.GetTextPlugins().OrderBy(x => x.Name);
			List<PluginSummary> summaryList = new List<PluginSummary>();

			foreach (TextPlugin plugin in plugins)
			{
				PluginSummary summary = new PluginSummary()
				{
					Id = plugin.Id,
					DatabaseId = plugin.DatabaseId,
					Name = plugin.Name,
					Description = plugin.Description,
					IsEnabled = true, //plugin.Settings.IsEnabled
				};

				if (!string.IsNullOrEmpty(summary.Description))
					summary.Description = summary.Description.Replace("\n", "<br/>");

				summaryList.Add(summary);
			}

			return View(summaryList);
		}

		public ActionResult Edit(string id)
		{
			if (string.IsNullOrEmpty(id))
				return RedirectToAction("Index");

			TextPlugin plugin = _pluginFactory.GetTextPlugin(id);
			PluginSummary summary = new PluginSummary()
			{
				Id = plugin.Id,
				DatabaseId = plugin.DatabaseId,
				Name = plugin.Name,
				Description = plugin.Description,
				SettingValues = new List<SettingValue>(plugin.Settings.Values)
			};

			return View(summary);
		}

		[HttpPost]
		public ActionResult Edit(PluginSummary summary)
		{
			var p = false;

			return View();
		}
	}
}
