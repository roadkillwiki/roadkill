using System;
using MongoDB.Driver;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Database.MongoDB
{
	public class MongoDbInstallerRepository : IInstallerRepository
	{
		public string ConnectionString { get; }

		public MongoDbInstallerRepository(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public void Wipe()
		{
			try
			{
				MongoDbStore.Wipe(ConnectionString);
			}
			catch (Exception ex)
			{
				throw new DatabaseException(ex, "An error occurred connecting to the MongoDB using the connection string {0}", ConnectionString);
			}
		}

		public void AddAdminUser(string email, string username, string password)
		{
			var user = new User();
			user.Email = email;
			user.Username = username;
			user.SetPassword(password);
			user.IsAdmin = true;
			user.IsEditor = true;
			user.IsActivated = true;

			SaveOrUpdate<User>(user);
		}

		public void CreateSchema()
		{
			try
			{
				MongoDbStore.Wipe(ConnectionString);
			}
			catch (Exception e)
			{
				throw new DatabaseException(e, "Install failed: unable to connect to the database using {0} - {1}", ConnectionString, e.Message);
			}
		}

		public void SaveSettings(SiteSettings siteSettings)
		{
			var entity = new SiteConfigurationEntity();

			entity.Id = SiteSettings.SiteSettingsId;
			entity.Version = ApplicationSettings.ProductVersion.ToString();
			entity.Content = siteSettings.GetJson();
			SaveOrUpdate<SiteConfigurationEntity>(entity);
		}

		public void SaveOrUpdate<T>(T obj) where T : IDataStoreEntity
		{
			MongoDbStore.SaveOrUpdate(ConnectionString, obj);
		}

		public void Dispose()
		{
		}
	}
}