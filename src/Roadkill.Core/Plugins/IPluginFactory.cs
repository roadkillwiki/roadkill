using System;
using System.Collections.Generic;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Plugins
{
	public interface IPluginFactory
	{
		void CopyTextPlugins(ApplicationSettings applicationSettings);
		IEnumerable<TextPlugin> GetTextPlugins();
		IEnumerable<TextPlugin> GetEnabledTextPlugins();
		void RegisterTextPlugin(TextPlugin plugin);

		/// <summary>
		/// Case insensitive search for a plugin.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		TextPlugin GetTextPlugin(string id);
	}
}
