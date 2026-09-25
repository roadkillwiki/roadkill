using System;
using System.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Roadkill.Core.Database
{
	public class DatabaseTester : IDatabaseTester
	{
		public void TestConnection(string databaseProvider, string connectionString)
		{
			try
			{
				if (databaseProvider == SupportedDatabases.MongoDB)
				{
					string databaseName = MongoUrl.Create(connectionString).DatabaseName;
					MongoClient client = new MongoClient(connectionString);
					IMongoDatabase database = client.GetDatabase(databaseName);
					database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
				}
				else
				{
					using (IDbConnection connection = RepositoryFactory.CreateConnectionFactory(databaseProvider, connectionString).CreateConnection())
					{
						connection.Open();
					}
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException(e, "Unable to connect to the database using '{0}' - {1}", connectionString, e.Message);
			}
		}
	}
}
