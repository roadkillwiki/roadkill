using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit.Plugins
{
	[TestFixture]
	[Category("Unit")]
	public class SettingsTests
	{
		[Test]
		public void setvalue_should_add_new_value()
		{
			// Arrange
			PluginSettings settings = new PluginSettings("mockplugin","1.0");

			// Act
			settings.SetValue("name", "value");

			// Assert
			List<SettingValue> valueList = new List<SettingValue>(settings.Values);
			Assert.That(valueList.Count, Is.EqualTo(1));
			Assert.That(valueList[0].Name, Is.EqualTo("name"));
			Assert.That(valueList[0].Value, Is.EqualTo("value"));
		}

		[Test]
		public void setvalue_should_update_existing_value()
		{
			// Arrange
			PluginSettings settings = new PluginSettings("mockplugin", "1.0");

			// Act
			settings.SetValue("name", "value");
			settings.SetValue("name", "new value");

			// Assert
			List<SettingValue> valueList = new List<SettingValue>(settings.Values);
			Assert.That(valueList.Count, Is.EqualTo(1));
			Assert.That(valueList[0].Value, Is.EqualTo("new value"));
		}

		[Test]
		public void setvalue_should_be_case_insensitive_when_updating_existing_value()
		{
			// Arrange
			PluginSettings settings = new PluginSettings("mockplugin", "1.0");

			// Act
			settings.SetValue("name", "value");
			settings.SetValue("NaME", "new value");

			// Assert
			List<SettingValue> valueList = new List<SettingValue>(settings.Values);
			Assert.That(valueList.Count, Is.EqualTo(1));
			Assert.That(valueList[0].Value, Is.EqualTo("new value"));
		}

		[Test]
		public void getvalue_should_return_known_value()
		{
			// Arrange
			PluginSettings settings = new PluginSettings("mockplugin", "1.0");

			// Act
			settings.SetValue("name1", "value1");
			settings.SetValue("name2", "value2");
			string value = settings.GetValue("name1");

			// Assert
			Assert.That(value, Is.EqualTo("value1"));
		}

		[Test]
		public void getvalue_should_be_case_insensitive()
		{
			// Arrange
			PluginSettings settings = new PluginSettings("mockplugin", "1.0");

			// Act
			settings.SetValue("name", "value");
			string value = settings.GetValue("NaME");

			// Assert
			Assert.That(value, Is.EqualTo("value"));
		}

		[Test]
		public void should_contain_empty_values_list()
		{
			// Arrange
			PluginSettings settings = new PluginSettings("mockplugin", "1.0");

			// Act + Assert
			Assert.That(settings.Values, Is.Not.Null);
			Assert.That(settings.Values.Count(), Is.EqualTo(0));
		}
	}
}
