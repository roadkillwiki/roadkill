using System;
using System.Data;
using Dapper;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Database.Repositories.Dapper
{
	public class DapperSettingsRepository : ISettingsRepository
	{
		private readonly IDbConnectionFactory _dbConnectionFactory;
		internal static readonly string TableName = "roadkill_siteconfiguration";

		public DapperSettingsRepository(IDbConnectionFactory dbConnectionFactory)
		{
			_dbConnectionFactory = dbConnectionFactory;
		}

		public void Dispose()
		{
		}

		public void SaveSiteSettings(SiteSettings siteSettings)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where id=@id";
				SiteConfigurationEntity entity = connection.QueryFirstOrDefault<SiteConfigurationEntity>(sql, new { id = SiteSettings.SiteSettingsId });
				bool settingsExist = (entity != null);

				// SiteSettings is the JSON object, SiteConfigurationEntity is the database entity.
				// In ideal world you would have a SiteConfigurationEntity.SiteSettings instead of a text column.
				if (entity == null)
					entity = new SiteConfigurationEntity();

				entity.Id = SiteSettings.SiteSettingsId;
				entity.Version = ApplicationSettings.ProductVersion;
				entity.Content = siteSettings.GetJson();

				sql = "";
				if (settingsExist)
				{
					sql = $"update {TableName} set ";
					sql += "version=@Version, content=@Content ";
					sql += "where id=@Id";
				}
				else
				{
					sql = $"insert into {TableName} ";
					sql += "(id, version, content) ";
					sql += "values (@Id, @Version, @Content)";
				}

				connection.Execute(sql, entity);
			}
		}

		public SiteSettings GetSiteSettings()
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where id=@id";
				SiteConfigurationEntity entity = connection.QueryFirstOrDefault<SiteConfigurationEntity>(sql, new { id = SiteSettings.SiteSettingsId });

				SiteSettings siteSettings = new SiteSettings();

				if (entity != null)
				{
					siteSettings = SiteSettings.LoadFromJson(entity.Content);
				}
				else
				{
					Log.Warn("Dapper: No site settings could be found in the database, using a default SiteSettings");
				}

				return siteSettings;
			}
		}

		public void SaveTextPluginSettings(TextPlugin plugin)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where id=@id";
				SiteConfigurationEntity entity = connection.QueryFirstOrDefault<SiteConfigurationEntity>(sql, new { id = plugin.DatabaseId });
				bool settingsExist = (entity != null);

				if (entity == null)
					entity = new SiteConfigurationEntity();

				entity.Id = plugin.DatabaseId;
				entity.Version = ApplicationSettings.ProductVersion;
				entity.Content = plugin.Settings.GetJson();

				sql = "";
				if (settingsExist)
				{
					sql = $"update {TableName} set ";
					sql += "version=@Version, content=@Content ";
					sql += "where id=@Id";
				}
				else
				{
					sql = $"insert into {TableName} ";
					sql += "(id, version, content) ";
					sql += "values (@Id, @Version, @Content)";
				}

				connection.Execute(sql, entity);
			}
		}

		public Settings GetTextPluginSettings(Guid databaseId)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where id=@id";
				SiteConfigurationEntity entity = connection.QueryFirstOrDefault<SiteConfigurationEntity>(sql, new { id = databaseId });

				Settings pluginSettings = null;
				if (entity != null)
				{
					pluginSettings = Settings.LoadFromJson(entity.Content);
				}

				return pluginSettings;
			}
		}
	}
}