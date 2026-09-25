using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Core.Database.MongoDB
{
	public class MongoDBSettingsRepository : ISettingsRepository
	{
		internal readonly string ConnectionString;

		public MongoDBSettingsRepository(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public void Wipe()
		{
			MongoDbStore.Wipe(ConnectionString);
		}

		public IQueryable<T> Queryable<T>() where T : IDataStoreEntity
		{
			return MongoDbStore.Queryable<T>(ConnectionString);
		}

		public void SaveOrUpdate<T>(T obj) where T : IDataStoreEntity
		{
			MongoDbStore.SaveOrUpdate(ConnectionString, obj);
		}

		public SiteSettings GetSiteSettings()
		{
			SiteConfigurationEntity entity = Queryable<SiteConfigurationEntity>()
												.FirstOrDefault(x => x.Id == SiteSettings.SiteSettingsId);
			SiteSettings siteSettings = new SiteSettings();

			if (entity != null)
			{
				siteSettings = SiteSettings.LoadFromJson(entity.Content);
			}
			else
			{
				Log.Warn("MongoDB: No site settings could be found in the database, using a default SiteSettings");
			}

			return siteSettings;
		}

		public PluginSettings GetTextPluginSettings(Guid databaseId)
		{
			SiteConfigurationEntity entity = Queryable<SiteConfigurationEntity>()
												.FirstOrDefault(x => x.Id == databaseId);

			PluginSettings pluginSettings = null;

			if (entity != null)
			{
				pluginSettings = PluginSettings.LoadFromJson(entity.Content);
			}

			return pluginSettings;
		}

		public void SaveTextPluginSettings(TextPlugin plugin)
		{
			SiteConfigurationEntity entity = Queryable<SiteConfigurationEntity>()
												.FirstOrDefault(x => x.Id == plugin.DatabaseId);

			if (entity == null)
				entity = new SiteConfigurationEntity();

			entity.Id = plugin.DatabaseId;
			entity.Version = plugin.Version;
			entity.Content = plugin.Settings.GetJson();
			SaveOrUpdate<SiteConfigurationEntity>(entity);
		}

		public void SaveSiteSettings(SiteSettings preferences)
		{
			// Get the fresh db entity first
			SiteConfigurationEntity entity = Queryable<SiteConfigurationEntity>()
												.FirstOrDefault(x => x.Id == SiteSettings.SiteSettingsId);
			if (entity == null)
				entity = new SiteConfigurationEntity();

			entity.Id = SiteSettings.SiteSettingsId;
			entity.Version = ApplicationSettings.ProductVersion.ToString();
			entity.Content = preferences.GetJson();
			SaveOrUpdate<SiteConfigurationEntity>(entity);
		}


		public void Dispose()
		{
			
		}
	}
}
