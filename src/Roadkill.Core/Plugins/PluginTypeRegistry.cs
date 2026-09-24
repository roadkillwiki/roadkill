using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Plugins
{
	/// <summary>
	/// Holds the concrete <see cref="TextPlugin"/> and <see cref="SpecialPagePlugin"/> types, found by scanning the
	/// Roadkill assemblies and the plugin assemblies (in the Plugins folder). This is registered as a singleton.
	/// </summary>
	public class PluginTypeRegistry
	{
		private readonly List<Type> _textPluginTypes = new List<Type>();
		private readonly List<Type> _specialPagePluginTypes = new List<Type>();
		private readonly List<TextPlugin> _textPluginInstances = new List<TextPlugin>();
		private readonly object _lock = new object();

		public IEnumerable<Type> TextPluginTypes => _textPluginTypes;
		public IEnumerable<Type> SpecialPagePluginTypes => _specialPagePluginTypes;

		public IEnumerable<TextPlugin> TextPluginInstances
		{
			get
			{
				lock (_lock)
				{
					return _textPluginInstances.ToList();
				}
			}
		}

		public PluginTypeRegistry()
		{
		}

		public PluginTypeRegistry(IEnumerable<Assembly> assemblies)
		{
			foreach (Assembly assembly in assemblies)
			{
				AddTypesFromAssembly(assembly);
			}
		}

		public void AddTextPluginInstance(TextPlugin plugin)
		{
			lock (_lock)
			{
				_textPluginInstances.Add(plugin);
			}
		}

		public void AddTypesFromAssembly(Assembly assembly)
		{
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				Log.Warn(ex, "Unable to load all types from the assembly {0}", assembly.FullName);
				types = ex.Types.Where(t => t != null).ToArray();
			}

			foreach (Type type in types.Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters))
			{
				if (typeof(TextPlugin).IsAssignableFrom(type) && !_textPluginTypes.Contains(type))
					_textPluginTypes.Add(type);
				else if (typeof(SpecialPagePlugin).IsAssignableFrom(type) && !_specialPagePluginTypes.Contains(type))
					_specialPagePluginTypes.Add(type);
			}
		}

		/// <summary>
		/// Creates a registry from all Roadkill assemblies in the application's base directory, and all plugin assemblies
		/// (copied from the Plugins folder to the bin/Plugins folder).
		/// </summary>
		public static PluginTypeRegistry Create(ApplicationSettings applicationSettings)
		{
			var assemblies = new List<Assembly>();

			foreach (string file in Directory.GetFiles(AppContext.BaseDirectory, "Roadkill*.dll"))
			{
				Assembly assembly = LoadAssembly(file);
				if (assembly != null)
					assemblies.Add(assembly);
			}

			try
			{
				PluginFileManager.CopyPlugins(applicationSettings);

				if (Directory.Exists(applicationSettings.PluginsBinPath))
				{
					foreach (string file in Directory.GetFiles(applicationSettings.PluginsBinPath, "*.dll", SearchOption.AllDirectories))
					{
						Assembly assembly = LoadAssembly(file);
						if (assembly != null)
							assemblies.Add(assembly);
					}
				}
			}
			catch (IOException ex)
			{
				Log.Error(ex, "Unable to load the plugins from {0}", applicationSettings.PluginsBinPath);
			}

			return new PluginTypeRegistry(assemblies.Distinct());
		}

		private static Assembly LoadAssembly(string path)
		{
			try
			{
				AssemblyName name = AssemblyName.GetAssemblyName(path);
				Assembly loaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), name));
				return loaded ?? AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
			}
			catch (Exception ex) when (ex is BadImageFormatException || ex is FileLoadException || ex is FileNotFoundException)
			{
				Log.Warn(ex, "Unable to load the assembly {0}", path);
				return null;
			}
		}
	}
}
