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
			IEnumerable<TextPlugin> plugins = _pluginFactory.GetTextPlugins();
			List<PluginSettingsSummary> summaryList = new List<PluginSettingsSummary>();

			foreach (TextPlugin plugin in plugins)
			{
				PluginSettingsSummary summary = new PluginSettingsSummary()
				{
					Id = plugin.Id,
					DatabaseId = plugin.DatabaseId,
					Name = plugin.Name,
					Description = plugin.Description,
					IsEnabled = plugin.Settings.IsEnabled
				};

				summaryList.Add(summary);
			}

			return View(summaryList);
		}
	}
}
