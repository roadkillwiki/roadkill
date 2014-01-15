using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Unit")]
	public abstract class UserRepositoryTests : RepositoryTests
	{
		protected User _adminUser;
		protected User _editor;
		protected User _inactiveUser;

		[SetUp]
		public void SetUp()
		{
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
		public void GetAdminById()
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
		public void GetUserByActivationKey_With_InactiveUser()
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
		public void GetUserByActivationKey_With_ActiveUser()
		{
			// Arrange
			User expectedUser = _adminUser;

			// Act
			User actualUser = Repository.GetUserByActivationKey(expectedUser.ActivationKey);

			// Assert
			Assert.That(actualUser, Is.Null);
		}

		[Test]
		public void GetEditorById()
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
		public void GetUserByEmail()
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
		public void GetUserByEmail_With_Inactive_User_And_No_Flag_Set_Should_Return_User()
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
		public void GetUserByEmail_With_Inactive_User_And_Active_Only_Flag_Should_Return_Null()
		{
			// Arrange

			// Act
			User actualUser = Repository.GetUserByEmail(_inactiveUser.Email, true);

			// Assert
			Assert.That(actualUser, Is.Null);
		}

		[Test]
		public void GetUserById()
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
		public void GetUserById_Should_Return_Null_When_User_Is_InActive_And_Active_Flag_Is_True()
		{
			// Arrange
			User expectedUser = null;

			// Act
			User actualUser = Repository.GetUserById(_inactiveUser.Id, true);

			// Assert
			Assert.That(actualUser, Is.EqualTo(expectedUser));
		}

		[Test]
		public void GetUserById_Should_Return_User_When_User_Is_InActive_And_Flag_Is_Not_Set()
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
		public void GetUserById_Should_Return_Null_For_Active_User_When_Flag_Is_False()
		{
			// Arrange
			User expectedUser = _inactiveUser;

			// Act
			User noUser = Repository.GetUserById(_editor.Id, false);

			// Assert
			Assert.That(noUser, Is.Null);
		}

		[Test]
		public void GetUserByPasswordResetKey()
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
		public void GetUserByUsername()
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
		public void GetUserByUsernameOrEmail()
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
		public void FindAllEditors()
		{
			// Arrange


			// Act
			List<User> allEditors = Repository.FindAllEditors().ToList();

			// Assert
			Assert.That(allEditors.Count, Is.EqualTo(3)); // includes the admin
		}

		[Test]
		public void FindAllAdmins()
		{
			// Arrange


			// Act
			List<User> allEditors = Repository.FindAllAdmins().ToList();

			// Assert
			Assert.That(allEditors.Count, Is.EqualTo(1));
		}

		[Test]
		public void DeleteUser()
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
		public void DeleteAllUsers()
		{
			// Arrange


			// Act
			Repository.DeleteAllUsers();

			// Assert
			Assert.That(Repository.FindAllAdmins().Count(), Is.EqualTo(0));
			Assert.That(Repository.FindAllEditors().Count(), Is.EqualTo(0));
		}

		[Test]
		public void SaveOrUpdateUser()
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
