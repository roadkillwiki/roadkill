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
			string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
			MongoClient client = new MongoClient(ConnectionString);
			MongoServer server = client.GetServer();
			MongoDatabase database = server.GetDatabase(databaseName);

			database.DropCollection(typeof(PageContent).Name);
			database.DropCollection(typeof(Page).Name);
			database.DropCollection(typeof(User).Name);
			database.DropCollection(typeof(SiteConfigurationEntity).Name);
		}

		public void AddAdminUser(string email, string username, string password)
		{
			
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
			
		}

		public void Dispose()
		{
		}
	}
}