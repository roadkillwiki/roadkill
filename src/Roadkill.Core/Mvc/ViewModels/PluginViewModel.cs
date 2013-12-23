using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Plugins;
using SettingValue = Roadkill.Core.Plugins.SettingValue;

namespace Roadkill.Core.Mvc.ViewModels
{
	public class PluginViewModel
	{
		public string Id { get; set; }
		public Guid DatabaseId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsEnabled { get; set; }
		public string EnabledSymbol
		{
			get
			{
				if (IsEnabled)
				{
					return "<div class=\"plugin-enabled\"></div>";
				}
				else 
				{
					return "<div class=\"plugin-disabled\"></div>";
				}
			}
		}

		/// <summary>
		/// This property should be lazy loaded when then settings are loaded.
		/// </summary>
		public List<SettingValue> SettingValues { get; set; }

		public PluginViewModel()
		{
			SettingValues = new List<SettingValue>();
		}

		public PluginViewModel(TextPlugin plugin) : this()
		{
			Id = plugin.Id;
			DatabaseId = plugin.DatabaseId;
			Name = plugin.Name;
			Description = plugin.Description;
			IsEnabled = plugin.Settings.IsEnabled;

			if (!string.IsNullOrEmpty(Description))
			{
				Description = Description.Replace("\n", "<br/>");
				Description = Description.Replace("\r", "<br/>");
			}
		}
	}
}
