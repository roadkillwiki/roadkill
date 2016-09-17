using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Roadkill.Core.Database.Repositories.Dapper
{
	public class DapperUserRepository : IUserRepository
	{
		private readonly IDbConnectionFactory _dbConnectionFactory;
		internal static readonly string TableName = "roadkill_users";

		public DapperUserRepository(IDbConnectionFactory dbConnectionFactory)
		{
			_dbConnectionFactory = dbConnectionFactory;
		}

		public void DeleteAllUsers()
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"delete from {TableName}";
				connection.Execute(sql);
			}
		}

		public void DeleteUser(User user)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"delete from {TableName} where id=@Id";
				connection.Execute(sql, user);
			}
		}

		public IEnumerable<User> FindAllEditors()
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where iseditor='1'";
				return connection.Query<User>(sql);
			}
		}

		public IEnumerable<User> FindAllAdmins()
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where isadmin='1'";
				return connection.Query<User>(sql);
			}
		}

		public User GetAdminById(Guid id)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where id=@id and isadmin='1'";
				return connection.QueryFirstOrDefault<User>(sql, new { id = id });
			}
		}

		public User GetUserByActivationKey(string key)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where activationkey=@key and isactivated='0'";
				return connection.QueryFirstOrDefault<User>(sql, new { key = key });
			}
		}

		public User GetEditorById(Guid id)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where id=@id and iseditor='1'";
				return connection.QueryFirstOrDefault<User>(sql, new { id = id });
			}
		}

		public User GetUserByEmail(string email, bool? isActivated = null)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where email=@email";

				if (isActivated.HasValue)
					sql += " and isActivated=@isActivated";

				return connection.QueryFirstOrDefault<User>(sql, new { email = email, isActivated = isActivated });
			}
		}

		public User GetUserById(Guid id, bool? isActivated = null)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where id=@id";

				if (isActivated.HasValue)
					sql += " and isActivated=@isActivated";

				return connection.QueryFirstOrDefault<User>(sql, new { id = id, isActivated = isActivated });
			}
		}

		public User GetUserByPasswordResetKey(string key)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where passwordresetkey=@key";
				return connection.QueryFirstOrDefault<User>(sql, new { key = key});
			}
		}

		public User GetUserByUsername(string username)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where username=@username";
				return connection.QueryFirstOrDefault<User>(sql, new { username = username });
			}
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {TableName} where username=@username or email=@email";
				return connection.QueryFirstOrDefault<User>(sql, new { username = username, email = email });
			}
		}

		public User SaveOrUpdateUser(User user)
		{
			bool userExists = GetUserById(user.Id) != null;

			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();
				string sql;

				if (userExists)
				{
					sql = $"update {TableName} set ";
					sql += "email=@Email, firstname=@Firstname, iseditor=@IsEditor, ";
					sql += "isadmin=@IsAdmin, isactivated=@IsActivated, lastname=@Lastname, username=@Username, ";
					sql += "password=@Password, salt=@Salt, activationkey=@ActivationKey, passwordresetkey=@PasswordResetKey ";
					sql += "where id=@Id";
				}
				else
				{
					user.Id = Guid.NewGuid();
					sql = $"insert into {TableName} ";
					sql += "(id, email, firstname, iseditor, isadmin, isactivated, lastname, username, ";
					sql += "password, salt, activationkey, PasswordResetKey) ";
					sql += "values (@Id, @Email, @Firstname, @IsEditor, @IsAdmin, @IsActivated, @Lastname, @Username, ";
					sql += "@Password, @Salt, @ActivationKey, @PasswordResetKey)";
				}

				connection.Execute(sql, user);

				return user;
			}
		}
	}
}
