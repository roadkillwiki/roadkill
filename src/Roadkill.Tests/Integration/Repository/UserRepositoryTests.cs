using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Integration")]
	public abstract class UserRepositoryTests
	{
		protected User _adminUser;
		protected User _editor;
		protected User _inactiveUser;

		protected IUserRepository Repository;

		protected abstract string ConnectionString { get; }
		protected abstract IUserRepository GetRepository();
		protected abstract void Clearup();
		protected virtual void CheckDatabaseProcessIsRunning() { }

		[SetUp]
		public void SetUp()
		{
			// Setup the repository
			Repository = GetRepository();
			Clearup();
			_adminUser = NewUser("admin@localhost", "admin", true, true);
			_adminUser = Repository.SaveOrUpdateUser(_adminUser);

			_editor = NewUser("editor1@localhost", "editor1", false, true);
			_editor = Repository.SaveOrUpdateUser(_editor);

			_inactiveUser = NewUser("editor2@localhost", "editor2", false, true, false);
			_inactiveUser = Repository.SaveOrUpdateUser(_inactiveUser);
		}

		protected User NewUser(string email, string username, bool isAdmin, bool isEditor, bool isActive = true)
		{
			return new User()
			{
				Email = email,
				Username = username,
				Password = "password",
				Salt = "123",
				IsActivated = isActive,
				IsAdmin = isAdmin,
				IsEditor = isEditor,
				ActivationKey = Guid.NewGuid().ToString(),
				Firstname = "Firstname",
				Lastname = "Lastname",
				PasswordResetKey = Guid.NewGuid().ToString()
			};
		}

		[Test]
		public void getadminbyid()
		{
			// Arrange
			User expectedUser = _adminUser;

			// Act
			User noUser = Repository.GetAdminById(_editor.Id);
			User actualUser = Repository.GetAdminById(expectedUser.Id);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void getuserbyactivationkey_with_inactiveuser()
		{
			// Arrange
			User expectedUser = _inactiveUser;

			// Act
			User noUser = Repository.GetUserByActivationKey("badkey");
			User actualUser = Repository.GetUserByActivationKey(expectedUser.ActivationKey);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void getuserbyactivationkey_with_activeuser()
		{
			// Arrange
			User expectedUser = _adminUser;

			// Act
			User actualUser = Repository.GetUserByActivationKey(expectedUser.ActivationKey);

			// Assert
			Assert.That(actualUser, Is.Null);
		}

		[Test]
		public void geteditorbyid()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = Repository.GetEditorById(Guid.Empty);
			User actualUser = Repository.GetEditorById(_editor.Id);
			User adminUser = Repository.GetEditorById(_adminUser.Id);

			// Assert
			Assert.That(noUser, Is.Null);
			Assert.That(adminUser, Is.Not.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void getuserbyemail()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = Repository.GetUserByEmail("invalid@email.com");
			User actualUser = Repository.GetUserByEmail(_editor.Email);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void getuserbyemail_with_inactive_user_and_no_flag_set_should_return_user()
		{
			// Arrange
			User expectedUser = _inactiveUser;

			// Act
			User actualUser = Repository.GetUserByEmail(_inactiveUser.Email);

			// Assert
			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void getuserbyemail_with_inactive_user_and_active_only_flag_should_return_null()
		{
			// Arrange

			// Act
			User actualUser = Repository.GetUserByEmail(_inactiveUser.Email, true);

			// Assert
			Assert.That(actualUser, Is.Null);
		}

		[Test]
		public void getuserbyid()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = Repository.GetUserById(Guid.Empty);
			User actualUser = Repository.GetUserById(_editor.Id);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void getuserbyid_should_return_null_when_user_is_inactive_and_active_flag_is_true()
		{
			// Arrange
			User expectedUser = null;

			// Act
			User actualUser = Repository.GetUserById(_inactiveUser.Id, true);

			// Assert
			Assert.That(actualUser, Is.EqualTo(expectedUser));
		}

		[Test]
		public void getuserbyid_should_return_user_when_user_is_inactive_and_flag_is_not_set()
		{
			// Arrange
			User expectedUser = _inactiveUser;

			// Act
			User actualUser = Repository.GetUserById(_inactiveUser.Id);

			// Assert
			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void getuserbyid_should_return_null_for_active_user_when_flag_is_false()
		{
			// Arrange
			User expectedUser = _inactiveUser;

			// Act
			User noUser = Repository.GetUserById(_editor.Id, false);

			// Assert
			Assert.That(noUser, Is.Null);
		}

		[Test]
		public void getuserbypasswordresetkey()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = Repository.GetUserByUsername("badkey");
			User actualUser = Repository.GetUserByPasswordResetKey(_editor.PasswordResetKey);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void getuserbyusername()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = Repository.GetUserByUsername("nobody");
			User actualUser = Repository.GetUserByUsername(_editor.Username);

			// Assert
			Assert.That(noUser, Is.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void getuserbyusernameoremail()
		{
			// Arrange
			User expectedUser = _editor;

			// Act
			User noUser = Repository.GetUserByUsernameOrEmail("nobody", "nobody@nobody.com");
			User emailUser = Repository.GetUserByUsernameOrEmail("nousername", _editor.Email);
			User actualUser = Repository.GetUserByUsernameOrEmail(_editor.Username, "doesntexist@email.com");

			// Assert
			Assert.That(noUser, Is.Null);
			Assert.That(emailUser, Is.Not.Null);

			Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(expectedUser.ActivationKey));
			Assert.That(actualUser.Email, Is.EqualTo(expectedUser.Email));
			Assert.That(actualUser.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(expectedUser.IsActivated));
			Assert.That(actualUser.IsAdmin, Is.EqualTo(expectedUser.IsAdmin));
			Assert.That(actualUser.IsEditor, Is.EqualTo(expectedUser.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(expectedUser.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(expectedUser.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(expectedUser.Salt));
		}

		[Test]
		public void findalleditors()
		{
			// Arrange


			// Act
			List<User> allEditors = Repository.FindAllEditors().ToList();

			// Assert
			Assert.That(allEditors.Count, Is.EqualTo(3)); // includes the admin
		}

		[Test]
		public void findalladmins()
		{
			// Arrange


			// Act
			List<User> allEditors = Repository.FindAllAdmins().ToList();

			// Assert
			Assert.That(allEditors.Count, Is.EqualTo(1));
		}

		[Test]
		public void deleteuser()
		{
			// Arrange
			User user = Repository.GetUserByUsername("admin");
			Guid id = user.Id;

			// Act
			Repository.DeleteUser(user);

			// Assert
			Assert.That(Repository.GetUserById(user.Id), Is.Null);
		}

		[Test]
		public void deleteallusers()
		{
			// Arrange


			// Act
			Repository.DeleteAllUsers();

			// Assert
			Assert.That(Repository.FindAllAdmins().Count(), Is.EqualTo(0));
			Assert.That(Repository.FindAllEditors().Count(), Is.EqualTo(0));
		}

		[Test]
		public void saveorupdateuser()
		{
			// Arrange
			User user = _adminUser;
			user.ActivationKey = "2key";
			user.Email = "2email@email.com";
			user.Firstname = "2firstname";
			user.IsActivated = true;
			user.IsEditor = true;
			user.Lastname = "2lastname";
			user.Password = "2password";
			user.PasswordResetKey = "2passwordkey";
			user.Salt = "2salt";
			user.Username = "2username";

			// Act
			Repository.SaveOrUpdateUser(user);
			User actualUser = Repository.GetUserById(user.Id);

			// Assert
			Assert.That(actualUser, Is.Not.Null);
			Assert.That(actualUser.Id, Is.EqualTo(user.Id));
			Assert.That(actualUser.ActivationKey, Is.EqualTo(user.ActivationKey));
			Assert.That(actualUser.Firstname, Is.EqualTo(user.Firstname));
			Assert.That(actualUser.IsActivated, Is.EqualTo(user.IsActivated));
			Assert.That(actualUser.IsEditor, Is.EqualTo(user.IsEditor));
			Assert.That(actualUser.Lastname, Is.EqualTo(user.Lastname));
			Assert.That(actualUser.Password, Is.EqualTo(user.Password));
			Assert.That(actualUser.PasswordResetKey, Is.EqualTo(user.PasswordResetKey));
			Assert.That(actualUser.Salt, Is.EqualTo(user.Salt));
			Assert.That(actualUser.Username, Is.EqualTo(user.Username));
		}
	}
}
