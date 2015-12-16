using System.Collections.Generic;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Services
{
	public interface IInstallationService
	{
		void AddAdminUser(string email, string password);
		IEnumerable<RepositoryInfo> GetSupportedDatabases();

		/// <summary>
		/// Clears all users from the system.
		/// </summary>
		/// <exception cref="DatabaseException">An databaseerror occurred while clearing the user table.</exception>
		void ClearUserTable();

		/// <summary>
		/// Creates the database schema tables.
		/// </summary>
		/// <exception cref="DatabaseException">An datastore error occurred while creating the database tables.</exception>
		void CreateTables();

		/// <summary>
		/// Saves all settings that are stored in the database, to the configuration table.
		/// </summary>
		/// <param name="model">Summary data containing the settings.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while saving the configuration.</exception>
		void SaveSiteSettings(SettingsViewModel model);
	}
}