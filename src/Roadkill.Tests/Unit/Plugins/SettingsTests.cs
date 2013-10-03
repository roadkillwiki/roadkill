using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit.Plugins
{
	[TestFixture]
	[Category("Unit")]
	public class SettingsTests
	{
		[Test]
		public void Should_Do_Amazing_Things()
		{
			// Arrange
			PluginSettings expectedSettings = new PluginSettings();
			expectedSettings.SetValue("somekey1", "thevalue1");
			expectedSettings.SetValue("somekey2", "thevalue2");

			// Act

			// Assert
		}
	}
}
