using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.DI;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins.BuiltIn;
using StructureMap;

namespace Roadkill.Core.Plugins
{
	public class PluginFactory
	{
		/// <summary>
		/// Copies the custom variable plugins from their storage location to the bin folder.
		/// </summary>
		public static void CopyCustomVariablePlugins(ApplicationSettings applicationSettings)
		{
			try
			{
				string pluginsourcePath = applicationSettings.CustomVariablePluginsPath;
				string pluginDestinationPath = applicationSettings.CustomVariablePluginsBinPath;

				if (Directory.Exists(pluginsourcePath))
				{
					if (!Directory.Exists(pluginDestinationPath))
						Directory.CreateDirectory(pluginDestinationPath);

					foreach (string subdirectory in Directory.GetDirectories(pluginsourcePath))
					{
						DirectoryInfo dirInfo = new DirectoryInfo(subdirectory);
						string destination = Path.Combine(pluginDestinationPath, dirInfo.Name);
						if (!Directory.Exists(destination))
							Directory.CreateDirectory(destination);

						foreach (string file in Directory.EnumerateFiles(subdirectory, "*.dll"))
						{
							FileInfo fileInfo = new FileInfo(file);
							string destPath = Path.Combine(destination, fileInfo.Name);
							File.Copy(file, destPath, true);
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
		public static void RegisterCustomVariablePlugin(CustomVariablePlugin plugin)
		{
			ServiceLocator.RegisterType<CustomVariablePlugin>(plugin);
		}

		/// <summary>
		/// Retrieves all custom variable plugins from the IoC container.
		/// </summary>
		public static IEnumerable<CustomVariablePlugin> GetCustomVariablePlugins()
		{
			return ServiceLocator.GetAllInstances<CustomVariablePlugin>();
		}
	}
}