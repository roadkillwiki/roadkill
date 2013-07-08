using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Plugins
{
	public class PluginSetting
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public SettingUIType UIType { get; set; }
	}
}
