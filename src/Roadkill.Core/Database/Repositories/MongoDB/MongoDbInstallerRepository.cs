using System;
using MongoDB.Driver;

namespace Roadkill.Core.Database.MongoDB
{
	public class MongoDbInstallerRepository : IInstallerRepository
	{
		public string ConnectionString { get; }

		public MongoDbInstallerRepository(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public void Install()
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

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="DatabaseException">Can't connect to the MongoDB server (but it's a valid connection string)</exception>
		public void TestConnection()
		{
			try
			{
				string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
				MongoClient client = new MongoClient(ConnectionString);
				MongoServer server = client.GetServer();
				MongoDatabase database = server.GetDatabase(databaseName);
				database.GetCollectionNames();
			}
			catch (Exception e)
			{
				throw new DatabaseException(e, "Unable to connect to the MongoDB database using {0} - {1}", ConnectionString, e.Message);
			}
		}

		public void Dispose()
		{
		}
	}
}