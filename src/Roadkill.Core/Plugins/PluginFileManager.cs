using System.IO;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Plugins
{
	public class PluginFileManager
	{
		/// <summary>
		/// Copies the plugins from the /Plugins directory to the bin folder.
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