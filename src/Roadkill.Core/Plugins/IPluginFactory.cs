using System;
using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Plugins.SpecialPages;

namespace Roadkill.Core.Plugins
{
	/// <summary>
	/// Represents a class that is responsible for managing all plugin instances in Roadkill.
	/// </summary>
	public interface IPluginFactory
	{
		/// <summary>
		/// Copies the plugins from the /Plugins directory to the bin folder. This is run at application startup.
		/// </summary>
		void CopyPlugins(ApplicationSettings applicationSettings);

		/// <summary>
		/// Retrieves all text plugins from the DI container.
		/// </summary>
		IEnumerable<TextPlugin> GetTextPlugins();

		/// <summary>
		/// Retrieves all text plugins with their Settings.IsEnabled set to true, from the IoC container.
		/// </summary>
		IEnumerable<TextPlugin> GetEnabledTextPlugins();

		/// <summary>
		/// Allows additional text plugins to be registered at runtime.
		/// </summary>
		void RegisterTextPlugin(TextPlugin plugin);

		/// <summary>
		/// Case insensitive search for a text plugin. Returns null if it doesn't exist.
		/// </summary>
		TextPlugin GetTextPlugin(string id);

		/// <summary>
		/// Gets all SpecialPage plugins registered in the DI container.
		/// </summary>
		IEnumerable<SpecialPagePlugin> GetSpecialPagePlugins();

		/// <summary>
		/// Case insensitive search for a special page plugin. Returns null if it doesn't exist.
		/// </summary>
		SpecialPagePlugin GetSpecialPagePlugin(string name);
	}
}
