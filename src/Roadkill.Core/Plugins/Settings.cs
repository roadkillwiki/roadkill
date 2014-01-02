using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Plugins
{
	// don't use inheritence for settings as it makes serialization a pain (every plugin 
	// would have to have its own over-ridden serialize method).
	// It's also a lot easier to use like this, and more error tolerant.

	public class Settings
	{
		private List<SettingValue> _values;

		/// <summary>
		/// Makes the settings more readable/easy to debug.
		/// </summary>
		public string PluginId { get; set; }

		/// <summary>
		/// Makes the settings more readable/easy to debug.
		/// </summary>
		public string Version { get; set; }

		public bool IsEnabled { get; set; }

		/// <summary>
		/// Gets all the setting values.
		/// </summary>
		public IEnumerable<SettingValue> Values
		{
			get
			{
				return _values;
			}
		}

		public Settings(string pluginId, string version)
		{
			PluginId = pluginId;
			Version = version;
			_values = new List<SettingValue>();
		}

		public void SetValue(string name, string value, SettingFormType formType = SettingFormType.Textbox)
		{
			SettingValue settingValue = _values.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) &&
																	x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			if (settingValue == null)
			{
				settingValue = new SettingValue();
				settingValue.Name = name;
				_values.Add(settingValue);
			}
			
			settingValue.Value = value;
			settingValue.FormType = formType;
		}

		public string GetValue(string name)
		{
			SettingValue settingValue = _values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			if (settingValue != null)
				return settingValue.Value;

			return "";
		}

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
				return new Settings("error - dummy id: " +Guid.NewGuid(), "1.0");
			}

			try
			{
				return JsonConvert.DeserializeObject<Settings>(json);
			}
			catch (JsonReaderException ex)
			{
				Log.Error(ex, "Settings.LoadFromJson - an exception occurred deserializing the JSON");
				return new Settings("error - dummy id:" + Guid.NewGuid(), "1.0");
			}
		}
	}
}
