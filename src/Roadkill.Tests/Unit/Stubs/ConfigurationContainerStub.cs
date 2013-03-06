using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;

namespace Roadkill.Tests.Unit
{
	internal class ConfigurationContainerStub : IConfigurationContainer
	{
		public SitePreferences SitePreferences { get; set; }
		public ApplicationSettings ApplicationSettings { get; set; }

		public ConfigurationContainerStub()
		{
			ApplicationSettings = new ApplicationSettings();
			SitePreferences = new SitePreferences();
		}
	}
}
