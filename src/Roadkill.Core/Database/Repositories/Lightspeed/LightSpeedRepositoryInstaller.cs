using System;
using System.Data;
using Mindscape.LightSpeed;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.Schema;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Database
{
	public class LightSpeedRepositoryInstaller : IRepositoryInstaller
	{
		public DataProvider DataProvider { get; }
		public SchemaBase Schema { get; }
		public string ConnectionString { get; }

		public LightSpeedRepositoryInstaller(DataProvider dataDataProvider, SchemaBase schema, string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
				throw new DatabaseException("The connection string is empty", null);

			DataProvider = dataDataProvider;
			Schema = schema;
			ConnectionString = connectionString;
		}

		private LightSpeedContext CreateLightSpeedContext()
		{
			LightSpeedContext context = new LightSpeedContext();
			context.ConnectionString = ConnectionString;
			context.DataProvider = DataProvider;
			context.IdentityMethod = IdentityMethod.GuidComb;
			context.CascadeDeletes = true;

#if DEBUG
			context.VerboseLogging = true;
			context.Logger = new DatabaseLogger();
#endif

			return context;
		}

		public void TestConnection()
		{
			LightSpeedContext context = CreateLightSpeedContext();

			using (IDbConnection connection = context.DataProviderObjectFactory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();
			}
		}

		public void Install()
		{
			LightSpeedContext context = CreateLightSpeedContext();

			using (IDbConnection connection = context.DataProviderObjectFactory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				IDbCommand command = context.DataProviderObjectFactory.CreateCommand();
				command.Connection = connection;

				Schema.Drop(command);
				Schema.Create(command);
			}
		}

		public void Upgrade()
		{
			try
			{
				LightSpeedContext context = CreateLightSpeedContext();

				using (IDbConnection connection = context.DataProviderObjectFactory.CreateConnection())
				{
					connection.ConnectionString = ConnectionString;
					connection.Open();

					IDbCommand command = context.DataProviderObjectFactory.CreateCommand();
					command.Connection = connection;

					Schema.Upgrade(command);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Upgrade failed: {0}", ex);
				throw new UpgradeException("A problem occurred upgrading the database schema.\n\n", ex);
			}

			//try
			//{
			//	SaveSiteSettings(new SiteSettings());
			//}
			//catch (Exception ex)
			//{
			//	Log.Error("Upgrade failed: {0}", ex);
			//	throw new UpgradeException("A problem occurred saving the site preferences.\n\n", ex);
			//}
		}
	}
}