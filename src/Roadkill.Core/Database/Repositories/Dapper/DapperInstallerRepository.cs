using System;
using System.Data;
using Dapper;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Core.Database.Repositories.Dapper
{
	public class DapperInstallerRepository : IInstallerRepository
	{
		private readonly IDbConnectionFactory _dbConnectionFactory;
		internal static readonly string UserTableName = "roadkill_users";
		internal static readonly string ConfigurationTableName = "roadkill_siteconfiguration";
		public SchemaBase Schema { get; }

		public DapperInstallerRepository(IDbConnectionFactory dbConnectionFactory, SchemaBase schema)
		{
			_dbConnectionFactory = dbConnectionFactory;
			Schema = schema;
		}

		public void Dispose()
		{
		}

		public void AddAdminUser(string email, string username, string password)
		{
			var user = new User();
			user.Id = Guid.NewGuid();
			user.Email = email;
			user.Username = username;
			user.SetPassword(password);
			user.IsAdmin = true;
			user.IsEditor = true;
			user.IsActivated = true;

			try
			{
				using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
				{
					connection.Open();

					string sql = $"insert into {UserTableName} ";
					sql += "(id, email, firstname, iseditor, isadmin, isactivated, lastname, username, ";
					sql += "password, salt, activationkey, PasswordResetKey) ";
					sql += "values (@Id, @Email, @Firstname, @IsEditor, @IsAdmin, @IsActivated, @Lastname, @Username, ";
					sql += "@Password, @Salt, @ActivationKey, @PasswordResetKey)";

					connection.Execute(sql, user);
				}
			}
			catch (Exception ex)
			{
				throw new DatabaseException(ex, "Install failed: unable to create the admin user {0}", ex.Message);
			}
		}

		public void CreateSchema()
		{
			try
			{
				using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
				{
					connection.Open();

					IDbCommand command = connection.CreateCommand();
					Schema.Drop(command);
					Schema.Create(command);
				}
			}
			catch (Exception ex)
			{
				throw new DatabaseException(ex, "Install failed: unable to create the schema {0}", ex.Message);
			}
		}

		public void SaveSettings(SiteSettings siteSettings)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				var entity = new SiteConfigurationEntity();
				entity.Id = SiteSettings.SiteSettingsId;
				entity.Version = ApplicationSettings.ProductVersion;
				entity.Content = siteSettings.GetJson();

				string sql = $"insert into {ConfigurationTableName} ";
				sql += "(id, version, content) ";
				sql += "values (@Id, @Version, @Content)";

				connection.Execute(sql, entity);
			}
		}
	}
}