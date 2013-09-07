using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Plugins
{
	public class Settings
	{
		public bool IsEnabled { get; set; }
		public List<SettingValue> Values { get; set; }

		public string GetJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		public static Settings LoadFromJson(string json)
		{
			// This looks familiar...
			if (string.IsNullOrEmpty(json))
			{
				Log.Warn("PluginSettings.LoadFromJson - json string was empty (returning a default Settings object)");
				return new Settings();
			}

			try
			{
				return JsonConvert.DeserializeObject<Settings>(json);
			}
			catch (JsonReaderException ex)
			{
				Log.Error(ex, "Settings.LoadFromJson - an exception occurred deserializing the JSON");
				return new Settings();
			}
		}
	}
}
