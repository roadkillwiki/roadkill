using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Configuration;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Core.Database.Repositories
{
	public interface ISettingsRepository
	{
		void Install(DataStoreType dataStoreType, string connectionString, bool enableCache);
		void Upgrade(ApplicationSettings applicationSettings);

		void SaveSiteSettings(SiteSettings siteSettings);
		SiteSettings GetSiteSettings();
		void SaveTextPluginSettings(TextPlugin plugin);
		PluginSettings GetTextPluginSettings(Guid databaseId);
	}
}
