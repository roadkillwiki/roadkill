using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Services
{
	public interface ISettingsService
	{
		IEnumerable<RepositoryInfo> GetSupportedDatabases();

		/// <summary>
		/// Retrieves the current site settings.
		/// </summary>
		/// <returns></returns>
		SiteSettings GetSiteSettings();

		/// <summary>
		/// Saves all settings that are stored in the database, to the configuration table.
		/// </summary>
		/// <param name="model">Summary data containing the settings.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while saving the configuration.</exception>
		void SaveSiteSettings(SettingsViewModel model);
	}
}