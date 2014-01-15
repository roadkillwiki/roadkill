using System;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Cache
{
	/// <summary>
	/// Defines a class that is capable of updating and retrieving cache plugin settings.
	/// </summary>
	public interface IPluginCache
	{
		/// <summary>
		/// Gets the plugin settings.
		/// </summary>
		/// <param name="plugin">The text plugin.</param>
		/// <returns>Returns the <see cref="Settings"/> or null if the plugin is not in the cache.</returns>
		Settings GetPluginSettings(TextPlugin plugin);

		/// <summary>
		/// Updates the plugin settings.
		/// </summary>
		/// <param name="plugin">The text plugin.</param>
		void UpdatePluginSettings(TextPlugin plugin);
	}
}
