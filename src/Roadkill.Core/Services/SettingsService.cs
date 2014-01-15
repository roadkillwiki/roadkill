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

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides common tasks for changing the Roadkill application settings.
	/// </summary>
	public class SettingsService : ServiceBase
	{
		public SettingsService(ApplicationSettings settings, IRepository repository)
			: base(settings, repository)
		{
		}

		/// <summary>
		/// Clears all users from the system.
		/// </summary>
		/// <exception cref="DatabaseException">An databaseerror occurred while clearing the user table.</exception>
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
		/// <param name="model">The settings data.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while creating the database tables.</exception>
		public void CreateTables(SettingsViewModel model)
		{
			try
			{
				DataStoreType dataStoreType = DataStoreType.ByName(model.DataStoreTypeName);
				Repository.Install(dataStoreType, model.ConnectionString, model.UseObjectCache);
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
		/// <param name="model">Summary data containing the settings.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while saving the configuration.</exception>
		public void SaveSiteSettings(SettingsViewModel model)
		{
			try
			{
				SiteSettings siteSettings = new SiteSettings();
				siteSettings.AllowedFileTypes = model.AllowedFileTypes;
				siteSettings.AllowUserSignup = model.AllowUserSignup;
				siteSettings.IsRecaptchaEnabled = model.IsRecaptchaEnabled;
				siteSettings.MarkupType = model.MarkupType;
				siteSettings.RecaptchaPrivateKey = model.RecaptchaPrivateKey;
				siteSettings.RecaptchaPublicKey = model.RecaptchaPublicKey;
				siteSettings.SiteUrl = model.SiteUrl;
				siteSettings.SiteName = model.SiteName;
				siteSettings.Theme = model.Theme;

				// v2.0
				siteSettings.OverwriteExistingFiles = model.OverwriteExistingFiles;
				siteSettings.HeadContent = model.HeadContent;
				siteSettings.MenuMarkup = model.MenuMarkup;

				Repository.SaveSiteSettings(siteSettings);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while saving the site configuration.");
			}
		}
	}
}
