using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Plugins;

namespace Roadkill.Tests.Unit
{
	public class PluginFactoryMock : IPluginFactory
	{
		public List<TextPlugin> TextPlugins { get; set; }

		public PluginFactoryMock()
		{
			TextPlugins = new List<TextPlugin>();
		}

		public void CopyTextPlugins(ApplicationSettings applicationSettings)
		{
		}

		public IEnumerable<TextPlugin> GetTextPlugins()
		{
			return TextPlugins;
		}

		public void RegisterTextPlugin(TextPlugin plugin)
		{
			TextPlugins.Add(plugin);
		}
	}
}
