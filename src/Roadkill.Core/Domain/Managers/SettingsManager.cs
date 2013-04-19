using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
using System.DirectoryServices;
using System.Web.Management;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Managers
{
	/// <summary>
	/// Provides common tasks for changing the Roadkill application settings.
	/// </summary>
	public class SettingsManager : ServiceBase
	{
		public SettingsManager(ApplicationSettings settings, IRepository repository)
			: base(settings, repository)
		{
		}

		/// <summary>
		/// Clears all pages and page content from the database.
		/// </summary>
		/// <exception cref="DatabaseException">An datastore error occurred while clearing the page data.</exception>
		public void ClearPageTables()
		{
			try
			{
				Repository.DeleteAllPages();
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while clearing all page tables.");
			}
		}

		/// <summary>
		/// Clears all users from the system.
		/// </summary>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while clearing the user table.</exception>
		public void ClearUserTable()
		{
			try
			{
				Repository.DeleteAllUsers();
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while clearing the user tables.");
			}
		}

		/// <summary>
		/// Creates the database schema tables.
		/// </summary>
		/// <param name="summary">The settings data.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while creating the database tables.</exception>
		public void CreateTables(SettingsSummary summary)
		{
			try
			{
				DataStoreType dataStoreType = DataStoreType.ByName(summary.DataStoreTypeName);
				Repository.Install(dataStoreType, summary.ConnectionString, summary.UseObjectCache);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while creating the site schema tables.");
			}
		}

		/// <summary>
		/// Retrieves the current site settings.
		/// </summary>
		/// <returns></returns>
		public SiteSettings GetSiteSettings()
		{
			return Repository.GetSiteSettings();
		}

		/// <summary>
		/// Saves all settings that are stored in the database, to the configuration table.
		/// </summary>
		/// <param name="summary">Summary data containing the settings.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while saving the configuration.</exception>
		public void SaveSiteSettings(SettingsSummary summary)
		{
			try
			{
				SiteSettings siteSettings = new SiteSettings();
				siteSettings.AllowedFileTypes = summary.AllowedFileTypes;
				siteSettings.AllowUserSignup = summary.AllowUserSignup;
				siteSettings.IsRecaptchaEnabled = summary.IsRecaptchaEnabled;
				siteSettings.MarkupType = summary.MarkupType;
				siteSettings.RecaptchaPrivateKey = summary.RecaptchaPrivateKey;
				siteSettings.RecaptchaPublicKey = summary.RecaptchaPublicKey;
				siteSettings.SiteUrl = summary.SiteUrl;
				siteSettings.SiteName = summary.SiteName;
				siteSettings.Theme = summary.Theme;

				Repository.SaveSiteSettings(siteSettings);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while saving the site configuration.");
			}
		}
	}
}
