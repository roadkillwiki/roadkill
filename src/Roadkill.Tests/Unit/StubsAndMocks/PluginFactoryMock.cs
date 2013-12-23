using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Plugins;
using Roadkill.Core.Plugins.SpecialPages;

namespace Roadkill.Tests.Unit
{
	public class PluginFactoryMock : IPluginFactory
	{
		public List<TextPlugin> TextPlugins { get; set; }
		public List<SpecialPagePlugin> SpecialPages { get; set; }

		public PluginFactoryMock()
		{
			TextPlugins = new List<TextPlugin>();
			SpecialPages = new List<SpecialPagePlugin>();
		}

		public void CopyPlugins(ApplicationSettings applicationSettings)
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

		public IEnumerable<SpecialPagePlugin> GetSpecialPagePlugins()
		{
			return SpecialPages;
		}

		public SpecialPagePlugin GetSpecialPagePlugin(string name)
		{
			return SpecialPages.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
