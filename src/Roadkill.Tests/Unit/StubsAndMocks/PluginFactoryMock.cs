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

		public IEnumerable<TextPlugin> GetEnabledTextPlugins()
		{
			return TextPlugins.Where(x => x.Settings.IsEnabled);
		}

		public void RegisterTextPlugin(TextPlugin plugin)
		{
			TextPlugins.Add(plugin);
		}

		public TextPlugin GetTextPlugin(string id)
		{
			return TextPlugins.FirstOrDefault(x => x.Id == id);
		}

		public void UpdateInstance(TextPlugin plugin)
		{
			throw new NotImplementedException();
		}
	}
}
