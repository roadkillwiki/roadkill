using System;
using System.Linq;
using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Linq;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Core.Database.LightSpeed
{
	public class LightSpeedSettingsRepository : ISettingsRepository
	{
		internal readonly IUnitOfWork _unitOfWork;

		internal IQueryable<PageEntity> Pages => UnitOfWork.Query<PageEntity>();
		internal IQueryable<PageContentEntity> PageContents => UnitOfWork.Query<PageContentEntity>();
		internal IQueryable<UserEntity> Users => UnitOfWork.Query<UserEntity>();

		public IUnitOfWork UnitOfWork
		{
			get
			{
				if (_unitOfWork == null)
				{
                    throw new DatabaseException("The IUnitOfWork for Lightspeed is null", null);
				}

				return _unitOfWork;
			}
		}

		public LightSpeedSettingsRepository(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		#region ISettingsRepository
		public void SaveSiteSettings(SiteSettings siteSettings)
		{
			SiteConfigurationEntity entity = UnitOfWork.FindById<SiteConfigurationEntity>(SiteSettings.SiteSettingsId);

			if (entity == null || entity.Id == Guid.Empty)
			{
				entity = new SiteConfigurationEntity();
				entity.Id = SiteSettings.SiteSettingsId;
				entity.Version = ApplicationSettings.ProductVersion.ToString();
				entity.Content = siteSettings.GetJson();
				UnitOfWork.Add(entity);
			}
			else
			{
				entity.Version = ApplicationSettings.ProductVersion.ToString();
				entity.Content = siteSettings.GetJson();
			}

			UnitOfWork.SaveChanges();
		}

		public SiteSettings GetSiteSettings()
		{
			SiteSettings siteSettings = new SiteSettings();
			SiteConfigurationEntity entity = UnitOfWork.FindById<SiteConfigurationEntity>(SiteSettings.SiteSettingsId);

			if (entity != null)
			{
				siteSettings = SiteSettings.LoadFromJson(entity.Content);
			}
			else
			{
				Log.Warn("No site settings could be found in the database, using a default instance");
			}

			return siteSettings;
		}

		public PluginSettings GetTextPluginSettings(Guid databaseId)
		{
			PluginSettings pluginSettings = null;
			SiteConfigurationEntity entity = UnitOfWork.FindById<SiteConfigurationEntity>(databaseId);

			if (entity != null)
			{
				pluginSettings = PluginSettings.LoadFromJson(entity.Content);
			}

			return pluginSettings;
		}



		public void SaveTextPluginSettings(TextPlugin plugin)
		{
			string version = plugin.Version;
			if (string.IsNullOrEmpty(version))
				version = "1.0.0";

			SiteConfigurationEntity entity = UnitOfWork.FindById<SiteConfigurationEntity>(plugin.DatabaseId);

			if (entity == null || entity.Id == Guid.Empty)
			{
				entity = new SiteConfigurationEntity();
				entity.Id = plugin.DatabaseId;
				entity.Version = version;
				entity.Content = plugin.Settings.GetJson();
				UnitOfWork.Add(entity);
			}
			else
			{
				entity.Version = version;
				entity.Content = plugin.Settings.GetJson();
			}

			UnitOfWork.SaveChanges();
		}

		#endregion

		#region IDisposable
		public void Dispose()
		{
			_unitOfWork.SaveChanges();
			_unitOfWork.Dispose();
		}
		#endregion
	}
}
