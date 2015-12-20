using System;
using System.Data;
using Mindscape.LightSpeed;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Core.Database
{
	public class LightSpeedInstallerRepository : IInstallerRepository
	{
		private LightSpeedContext _context;
		public DataProvider DataProvider { get; }
		public SchemaBase Schema { get; }
		public string ConnectionString { get; }

		public LightSpeedInstallerRepository(DataProvider dataProvider, SchemaBase schema, string connectionString)
		{
			DataProvider = dataProvider;
			Schema = schema;
			ConnectionString = connectionString;

			_context = CreateLightSpeedContext(dataProvider, connectionString);
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

		public void AddAdminUser(string email, string username, string password)
		{
			try
			{
				using (IUnitOfWork unitOfWork = _context.CreateUnitOfWork())
				{
					var user = new User();
					user.Email = email;
					user.Username = username;
					user.SetPassword(password);
					user.IsAdmin = true;
					user.IsEditor = true;
					user.IsActivated = true;

					var entity = new UserEntity();
					ToEntity.FromUser(user, entity);

					unitOfWork.Add(entity);
					unitOfWork.SaveChanges();
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException(e, "Install failed: unable to create the admin user using '{0}' - {1}", ConnectionString, e.Message);
			}
		}

		public void SaveSettings(SiteSettings siteSettings)
		{
			try
			{
				using (IUnitOfWork unitOfWork = _context.CreateUnitOfWork())
				{
					var entity = new Roadkill.Core.Database.LightSpeed.SiteConfigurationEntity();
					entity.Id = SiteSettings.SiteSettingsId;
					entity.Version = ApplicationSettings.ProductVersion;
					entity.Content = siteSettings.GetJson();

					unitOfWork.Add(entity);
					unitOfWork.SaveChanges();
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException(e, "Install failed: unable to connect to the database using '{0}' - {1}", ConnectionString, e.Message);
			}
		}

		public void CreateSchema()
		{
			try
			{
				using (IDbConnection connection = _context.DataProviderObjectFactory.CreateConnection())
				{
					connection.ConnectionString = ConnectionString;
					connection.Open();

					IDbCommand command = _context.DataProviderObjectFactory.CreateCommand();
					command.Connection = connection;

					Schema.Drop(command);
					Schema.Create(command);
				}
			}
			catch (Exception e)
			{
				throw new DatabaseException(e, "Install failed: unable to connect to the database using '{0}' - {1}", ConnectionString, e.Message);
			}
		}

		public void Dispose()
		{
			
		}
	}
}