using System;
using System.Data;
using Mindscape.LightSpeed;
using MongoDB.Driver;
using Roadkill.Core.Database.LightSpeed;

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
					MongoServer server = client.GetServer();
					MongoDatabase database = server.GetDatabase(databaseName);
					database.GetCollectionNames();
				}
				else
				{
					var dataProvider = DataProvider.SqlServer2008;

					if (databaseProvider == SupportedDatabases.MySQL)
					{
						dataProvider = DataProvider.MySql5;
					}
					else if (databaseProvider == SupportedDatabases.Postgres)
					{
						dataProvider = DataProvider.PostgreSql9;
					}

					LightSpeedContext context = CreateLightSpeedContext(dataProvider, connectionString);

					using (IDbConnection connection = context.DataProviderObjectFactory.CreateConnection())
					{
						connection.ConnectionString = connectionString;
						connection.Open();
					}
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException(e, "Unable to connect to the database using '{0}' - {1}", connectionString, e.Message);
			}
		}

		private LightSpeedContext CreateLightSpeedContext(DataProvider dataProvider, string connectionString)
		{
			LightSpeedContext context = new LightSpeedContext();
			context.ConnectionString = connectionString;
			context.DataProvider = dataProvider;
			context.IdentityMethod = IdentityMethod.GuidComb;
			context.CascadeDeletes = true;

#if DEBUG
			context.VerboseLogging = true;
			context.Logger = new DatabaseLogger();
#endif

			return context;
		}
	}
}