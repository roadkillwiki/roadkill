using System;
using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Plugins.SpecialPages;

namespace Roadkill.Core.Plugins
{
	public interface IPluginFactory
	{
		void CopyTextPlugins(ApplicationSettings applicationSettings);
		IEnumerable<SpecialPage> GetSpecialPagePlugins();
		IEnumerable<TextPlugin> GetTextPlugins();
		IEnumerable<TextPlugin> GetEnabledTextPlugins();
		void RegisterTextPlugin(TextPlugin plugin);

		/// <summary>
		/// Case insensitive search for a text plugin. Returns null if it doesn't exist.
		/// </summary>
		TextPlugin GetTextPlugin(string id);

		/// <summary>
		/// Case insensitive search for a special page plugin. Returns null if it doesn't exist.
		/// </summary>
		SpecialPage GetSpecialPagePlugin(string name);
	}
}
