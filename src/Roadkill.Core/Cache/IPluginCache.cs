using System;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Cache
{
	public interface IPluginCache
	{
		Settings GetPluginSettings(TextPlugin plugin);
		void UpdatePluginSettings(TextPlugin plugin);
	}
}
