using System;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Cache
{
	/// <summary>
	/// Defines a class that is capable of updating and retrieving cache plugin settings.
	/// </summary>
	public interface IPluginCache
	{
		Settings GetPluginSettings(TextPlugin plugin);
		void UpdatePluginSettings(TextPlugin plugin);
	}
}
