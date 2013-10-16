using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DI;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins.BuiltIn;
using StructureMap;

namespace Roadkill.Core.Plugins
{
	public class PluginFactory : IPluginFactory
	{
		/// <summary>
		/// Copies the custom variable plugins from their storage location to the bin folder.
		/// </summary>
		public void CopyTextPlugins(ApplicationSettings applicationSettings)
		{
			try
			{
				string pluginsourcePath = applicationSettings.TextPluginsPath;
				string pluginDestinationPath = applicationSettings.TextPluginsBinPath;

				if (Directory.Exists(pluginsourcePath))
				{
					if (!Directory.Exists(pluginDestinationPath))
						Directory.CreateDirectory(pluginDestinationPath);

					foreach (string subdirectory in Directory.GetDirectories(pluginsourcePath))
					{
						// Create the irectory in the /bin/Plugins/CustomVariables folder,
						// e.g. /bin/Plugins/CustomVariables/MyPlugin
						DirectoryInfo dirInfo = new DirectoryInfo(subdirectory);
						string destination = Path.Combine(pluginDestinationPath, dirInfo.Name);
						if (!Directory.Exists(destination))
						{
							Directory.CreateDirectory(destination);
							Log.Information("Created directory {0} for plugin", destination);
						}

						foreach (string sourceFile in Directory.EnumerateFiles(subdirectory, "*.dll"))
						{
							// Copy the plugin's dlls only - but only if the file write time is more recent.
							// If this check is removed, a looping app restart occurs (as the bin folder
							// changes, so an app start occurs, and this method is called on app start, which then triggers another restart).
							FileInfo sourceInfo = new FileInfo(sourceFile);
							string destPath = Path.Combine(destination, sourceInfo.Name);
							FileInfo destInfo = new FileInfo(destPath);

							if (sourceInfo.LastWriteTimeUtc > destInfo.LastWriteTimeUtc)
							{
								File.Copy(sourceFile, destPath, true);
								Log.Information("Copied text plugin file '{0}' to '{1}' as it's newer ({2} > {3})", sourceInfo.FullName, destInfo.FullName,
																											 sourceInfo.LastWriteTimeUtc, destInfo.LastWriteTimeUtc);
							}
						}
					}
				}
				else
				{
					Directory.CreateDirectory(pluginsourcePath);
				}
			}
			catch (IOException e)
			{
				Log.Error(e, "Unable to copy custom variable plugins to the bin folder");
			}
		}

		/// <summary>
		/// Allows additional custom variable plugins to be registered at runtime.
		/// </summary>
		public void RegisterTextPlugin(TextPlugin plugin)
		{
			ServiceLocator.RegisterType<TextPlugin>(plugin);
		}

		/// <summary>
		/// Retrieves all custom variable plugins from the IoC container.
		/// </summary>
		public IEnumerable<TextPlugin> GetTextPlugins()
		{
			return ServiceLocator.GetAllInstances<TextPlugin>();
		}

		public TextPlugin GetTextPlugin(string id)
		{
			return ServiceLocator.GetAllInstances<TextPlugin>().FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}