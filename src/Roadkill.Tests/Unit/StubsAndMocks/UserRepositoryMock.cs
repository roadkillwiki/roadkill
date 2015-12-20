using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Database;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class UserRepositoryMock : IUserRepository
	{
		public List<User> Users { get; set; }

		public string InstalledConnectionString { get; private set; }
		public bool InstalledEnableCache { get; private set; }

		public UserRepositoryMock()
		{
			Users = new List<User>();
		}

		public User GetAdminById(Guid id)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsAdmin);
		}

		public User GetUserByActivationKey(string key)
		{
			return Users.FirstOrDefault(x => x.ActivationKey == key && x.IsActivated == false);
		}

		public User GetEditorById(Guid id)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsEditor);
		}

		public User GetUserByEmail(string email, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated);
			else
				return Users.FirstOrDefault(x => x.Email == email);
		}

		public User GetUserById(Guid id, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated);
			else
				return Users.FirstOrDefault(x => x.Id == id);
		}

		public User GetUserByPasswordResetKey(string key)
		{
			return Users.FirstOrDefault(x => x.PasswordResetKey == key);
		}

		public User GetUserByUsername(string username)
		{
			return Users.FirstOrDefault(x => x.Username == username);
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			return Users.FirstOrDefault(x => x.Username == username || x.Email == email);
		}

		public IEnumerable<User> FindAllEditors()
		{
			return Users.Where(x => x.IsEditor).ToList();
		}

		public IEnumerable<User> FindAllAdmins()
		{
			return Users.Where(x => x.IsAdmin).ToList();
		}

		public void DeleteUser(User user)
		{
			Users.Remove(user);
		}

		public void DeleteAllUsers()
		{
			Users = new List<User>();
		}

		public User SaveOrUpdateUser(User user)
		{
			User existingUser = Users.FirstOrDefault(x => x.Id == user.Id);

			if (existingUser == null)
			{
				user.Id = Guid.NewGuid();
				Users.Add(user);
			}
			else
			{
				user.ActivationKey = user.ActivationKey;
				user.Email = user.Email;
				user.Firstname = user.Firstname;
				user.IsActivated = user.IsActivated;
				user.IsAdmin = user.IsAdmin;
				user.IsEditor = user.IsEditor;
				user.Lastname = user.Lastname;
				user.Password = user.Password;
				user.PasswordResetKey = user.PasswordResetKey;
				user.Username = user.Username;
				user.Salt = user.Salt;
			}

			return user;
		}

		public void Dispose()
		{
			
		}
	}
}
