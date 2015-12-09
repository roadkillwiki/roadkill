using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Roadkill.Core.Configuration;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Core.Logging;
using StructureMap;

namespace Roadkill.Core.Plugins
{
	/// <summary>
	/// The default <see cref="IPluginFactory"/> implementation, that uses the <see cref="StructureMapServiceLocator"/> class.
	/// </summary>
	public class PluginFactory : IPluginFactory
	{
		private readonly IContainer _container;

		public PluginFactory(IContainer container)
		{
			_container = container;
		}

		/// <summary>
		/// Allows additional text plugins to be registered at runtime.
		/// </summary>
		public void RegisterTextPlugin(TextPlugin plugin)
		{
			_container.Configure(x => x.For<TextPlugin>().Add(plugin));
		}

		/// <summary>
		/// Retrieves all text plugins from the IoC container.
		/// </summary>
		public IEnumerable<TextPlugin> GetTextPlugins()
		{
			return _container.GetAllInstances<TextPlugin>();
		}

		/// <summary>
		/// Retrieves all text plugins from the IoC container.
		/// </summary>
		public IEnumerable<TextPlugin> GetEnabledTextPlugins()
		{
			return _container.GetAllInstances<TextPlugin>().Where(x => x.Settings.IsEnabled);
		}

		/// <summary>
		/// Case insensitive search for a text plugin. Returns null if it doesn't exist.
		/// </summary>
		public TextPlugin GetTextPlugin(string id)
		{
			return _container.GetAllInstances<TextPlugin>().FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
		}

		public IEnumerable<SpecialPagePlugin> GetSpecialPagePlugins()
		{
			return _container.GetAllInstances<SpecialPagePlugin>();
		}

		/// <summary>
		/// Case insensitive search for a special page plugin. Returns null if it doesn't exist.
		/// </summary>
		public SpecialPagePlugin GetSpecialPagePlugin(string name)
		{
			return _container.GetAllInstances<SpecialPagePlugin>().FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}

		/// <summary>
		/// Copies the plugins from the /Plugins directory to the bin folder. This is run at application startup.
		/// </summary>
		public static void CopyPlugins(ApplicationSettings applicationSettings)
		{
			CopyAssemblies(applicationSettings.PluginsPath, applicationSettings.PluginsBinPath);
		}

		/// <summary>
		/// Copies plugins from their storage location to the bin folder.
		/// </summary>
		internal static void CopyAssemblies(string pluginsourcePath, string pluginDestinationPath, bool hasSubDirectories = true)
		{
			try
			{
				if (Directory.Exists(pluginsourcePath))
				{
					if (!Directory.Exists(pluginDestinationPath))
						Directory.CreateDirectory(pluginDestinationPath);

					if (hasSubDirectories)
					{
						foreach (string subdirectory in Directory.GetDirectories(pluginsourcePath))
						{
							CopyDirectoryContents(subdirectory, pluginDestinationPath);
						}
					}
					else
					{
						CopyDirectoryContents(pluginsourcePath, pluginDestinationPath);
					}
				}
				else
				{
					Directory.CreateDirectory(pluginsourcePath);
				}
			}
			catch (IOException e)
			{
				Log.Error(e, "Unable to copy plugins to the bin folder");
			}
		}

		private static void CopyDirectoryContents(string subdirectory, string pluginDestinationPath)
		{
			// Create the directory in the /bin/Plugins/CustomVariables folder,
			// e.g. /bin/Plugins/TextPlugins/MyPlugin
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
					Log.Information("Copied plugin file '{0}' to '{1}' as it's newer ({2} > {3})", sourceInfo.FullName, destInfo.FullName,
																								 sourceInfo.LastWriteTimeUtc, destInfo.LastWriteTimeUtc);
				}
			}
		}
	}
}