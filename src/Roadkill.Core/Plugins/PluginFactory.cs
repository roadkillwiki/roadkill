using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.Plugins
{
	/// <summary>
	/// The default <see cref="IPluginFactory"/> implementation. Plugin types are discovered at startup by the
	/// <see cref="PluginTypeRegistry"/>, and new instances are created (per call) from the current request's services.
	/// </summary>
	public class PluginFactory : IPluginFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly PluginTypeRegistry _registry;

		public PluginFactory(IServiceProvider serviceProvider, PluginTypeRegistry registry)
		{
			_serviceProvider = serviceProvider;
			_registry = registry;
		}

		/// <summary>
		/// Allows additional text plugins to be registered at runtime.
		/// </summary>
		public void RegisterTextPlugin(TextPlugin plugin)
		{
			_registry.AddTextPluginInstance(plugin);
		}

		/// <summary>
		/// Retrieves all text plugins.
		/// </summary>
		public IEnumerable<TextPlugin> GetTextPlugins()
		{
			var plugins = new List<TextPlugin>();

			foreach (Type type in _registry.TextPluginTypes)
			{
				plugins.Add(InjectTextPluginProperties((TextPlugin)ActivatorUtilities.CreateInstance(_serviceProvider, type)));
			}

			foreach (TextPlugin plugin in _registry.TextPluginInstances)
			{
				plugins.Add(InjectTextPluginProperties(plugin));
			}

			return plugins;
		}

		/// <summary>
		/// Retrieves all enabled text plugins.
		/// </summary>
		public IEnumerable<TextPlugin> GetEnabledTextPlugins()
		{
			return GetTextPlugins().Where(x => x.Settings.IsEnabled);
		}

		/// <summary>
		/// Case insensitive search for a text plugin. Returns null if it doesn't exist.
		/// </summary>
		public TextPlugin GetTextPlugin(string id)
		{
			return GetTextPlugins().FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
		}

		public IEnumerable<SpecialPagePlugin> GetSpecialPagePlugins()
		{
			var plugins = new List<SpecialPagePlugin>();

			foreach (Type type in _registry.SpecialPagePluginTypes)
			{
				var plugin = (SpecialPagePlugin)ActivatorUtilities.CreateInstance(_serviceProvider, type);
				plugin.ApplicationSettings = _serviceProvider.GetService<ApplicationSettings>();
				plugin.Context = _serviceProvider.GetService<IUserContext>();
				plugin.UserService = _serviceProvider.GetService<UserServiceBase>();
				plugin.PageService = _serviceProvider.GetService<IPageService>();
				plugin.SettingsService = _serviceProvider.GetService<SettingsService>();
				plugins.Add(plugin);
			}

			return plugins;
		}

		/// <summary>
		/// Case insensitive search for a special page plugin. Returns null if it doesn't exist.
		/// </summary>
		public SpecialPagePlugin GetSpecialPagePlugin(string name)
		{
			return GetSpecialPagePlugins().FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}

		private TextPlugin InjectTextPluginProperties(TextPlugin plugin)
		{
			plugin.ApplicationSettings = plugin.ApplicationSettings ?? _serviceProvider.GetService<ApplicationSettings>();
			plugin.PluginCache = plugin.PluginCache ?? _serviceProvider.GetService<IPluginCache>();
			plugin.Repository = plugin.Repository ?? _serviceProvider.GetService<ISettingsRepository>();
			return plugin;
		}
	}
}
