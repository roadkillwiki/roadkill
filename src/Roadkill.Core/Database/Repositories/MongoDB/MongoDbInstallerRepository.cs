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

		private MongoCollection<T> GetCollection<T>()
		{
			try
			{
				string connectionString = ConnectionString;

				string databaseName = MongoUrl.Create(connectionString).DatabaseName;
				MongoClient client = new MongoClient(connectionString);
				MongoServer server = client.GetServer();
				MongoDatabase database = server.GetDatabase(databaseName);

				return database.GetCollection<T>(typeof(T).Name);
			}
			catch (Exception ex)
			{
				throw new DatabaseException(ex, "An error occurred connecting to the MongoDB using the connection string {0}", ConnectionString);
			}
		}

		public void Wipe()
		{
			try
			{
				string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
				MongoClient client = new MongoClient(ConnectionString);
				MongoServer server = client.GetServer();
				MongoDatabase database = server.GetDatabase(databaseName);

				database.DropCollection(typeof(PageContent).Name);
				database.DropCollection(typeof(Page).Name);
				database.DropCollection(typeof(User).Name);
				database.DropCollection(typeof(SiteConfigurationEntity).Name);
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
				string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
				MongoClient client = new MongoClient(ConnectionString);
				MongoServer server = client.GetServer();
				MongoDatabase database = server.GetDatabase(databaseName);
				database.DropCollection("Page");
				database.DropCollection("PageContent");
				database.DropCollection("User");
				database.DropCollection("SiteConfiguration");
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
			MongoCollection<T> collection = GetCollection<T>();
			collection.Save<T>(obj);
		}

		public void Dispose()
		{
		}
	}
}