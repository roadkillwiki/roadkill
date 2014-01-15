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
		// Settings doesn't use inheritence for its values (e.g. a plugin would define its own classes derived from Setting)
		// as it makes serialization a pain (every plugin would have to have its own over-ridden serialize method).
		// It's also a lot easier to use like this, and more error tolerant.
		private List<SettingValue> _values;

		/// <summary>
		/// The id of the plugin that the setting belongs to, used primarily for readability in the JSON. 
		/// </summary>
		public string PluginId { get; set; }

		/// <summary>
		/// The version of the plugin that the setting belongs to, used primarily for readability in the JSON.
		/// </summary>
		public string Version { get; set; }

		/// <summary>
		/// Whether the plugin is enabled or not.
		/// </summary>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="Settings"/> class.
		/// </summary>
		/// <param name="pluginId">The plugin id, used for reference only.</param>
		/// <param name="version">The plugin version the settings are for, used for reference only.</param>
		public Settings(string pluginId, string version)
		{
			PluginId = pluginId;
			Version = version;
			_values = new List<SettingValue>();
		}

		/// <summary>
		/// Sets the setting value.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		/// <param name="formType">The UI type that should be used to represent the value (not currently implemented).</param>
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

		/// <summary>
		/// Retrieves the setting value from this instance's current values.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting, or an empty string if the setting name is not found.</returns>
		public string GetValue(string name)
		{
			SettingValue settingValue = _values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			if (settingValue != null)
				return settingValue.Value;

			return "";
		}

		/// <summary>
		/// Serializes this <see cref="Settings"/> instance and returns it as a JSON string.
		/// </summary>
		/// <returns></returns>
		public string GetJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		/// <summary>
		/// Creates an new <see cref="Settings"/> instance from the JSON provided.
		/// </summary>
		/// <param name="json">The json.</param>
		/// <returns>A new <see cref="Settings"/> instance or if the JSON couldn't be deserialized, a <see cref="Settings"/> 
		/// instance with a dummy (random) plugin ID and version, and errors logged to the log.</returns>
		public static Settings LoadFromJson(string json)
		{
			// An exact copy of SiteSettings.LoadFromJson
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
