using MongoDB.Driver;

namespace Roadkill.Core.Database.MongoDB
{
	public class MongoDBRepositoryInstaller : IRepositoryInstaller
	{
		public string ConnectionString { get; }

		public MongoDBRepositoryInstaller(string connectionString)
		{
			ConnectionString = connectionString;
		}

		private MongoCollection<T> GetCollection<T>()
		{
			string connectionString = ConnectionString;

			string databaseName = MongoUrl.Create(connectionString).DatabaseName;
			MongoClient client = new MongoClient(connectionString);
			MongoServer server = client.GetServer();
			MongoDatabase database = server.GetDatabase(databaseName);

			return database.GetCollection<T>(typeof(T).Name);
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
		/// <exception cref="MongoConnectionException">Can't connect to the MongoDB server (but it's a valid connection string)</exception>
		public void TestConnection()
		{
			string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
			MongoClient client = new MongoClient(ConnectionString);
			MongoServer server = client.GetServer();
			MongoDatabase database = server.GetDatabase(databaseName);
			database.GetCollectionNames();
		}

		public void Upgrade()
		{
			// TODO
		}
	}
}