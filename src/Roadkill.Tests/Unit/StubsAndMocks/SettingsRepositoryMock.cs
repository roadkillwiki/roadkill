using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class SettingsRepositoryMock : ISettingsRepository
	{
		public SiteSettings SiteSettings { get; set; }
		public List<TextPlugin> TextPlugins { get; set; }

		public PluginSettings PluginSettings { get; set; }
		public string InstalledConnectionString { get; private set; }
		public bool InstalledEnableCache { get; private set; }
		public bool ThrowSaveSiteSettingsException { get; set; }

		public SettingsRepositoryMock()
		{
			SiteSettings = new SiteSettings();
			TextPlugins = new List<TextPlugin>();
		}

		public void SaveSiteSettings(SiteSettings settings)
		{
			if (ThrowSaveSiteSettingsException)
				throw new DatabaseException("Something happened", null);

			SiteSettings = settings;
		}

		public SiteSettings GetSiteSettings()
		{
			return SiteSettings;
		}

		public void SaveTextPluginSettings(TextPlugin plugin)
		{
			int index = TextPlugins.IndexOf(plugin);

			if (index == -1)
				TextPlugins.Add(plugin);
			else
				TextPlugins[index] = plugin;
		}

		public PluginSettings GetTextPluginSettings(Guid databaseId)
		{
			if (PluginSettings != null)
				return PluginSettings;

			TextPlugin savedPlugin = TextPlugins.FirstOrDefault(x => x.DatabaseId == databaseId);

			if (savedPlugin != null)
				return savedPlugin._settings; // DON'T CALL Settings - you'll get a StackOverflowException
			else
				return null;
		}

		public void Dispose()
		{
			
		}
	}
}
