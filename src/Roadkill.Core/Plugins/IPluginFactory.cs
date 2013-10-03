using System;
using System.Collections.Generic;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Plugins
{
	public interface IPluginFactory
	{
		void CopyTextPlugins(ApplicationSettings applicationSettings);
		IEnumerable<TextPlugin> GetTextPlugins();
		void RegisterTextPlugin(TextPlugin plugin);
	}
}
