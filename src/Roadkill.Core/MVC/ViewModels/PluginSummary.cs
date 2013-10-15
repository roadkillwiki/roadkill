using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SettingValue = Roadkill.Core.Plugins.SettingValue;

namespace Roadkill.Core.Mvc.ViewModels
{
	public class PluginSummary
	{
		public string Id { get; set; }
		public Guid DatabaseId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsEnabled { get; set; }

		/// <summary>
		/// This property should be lazy loaded when then settings are loaded.
		/// </summary>
		public List<SettingValue> SettingValues { get; set; }
	}
}
