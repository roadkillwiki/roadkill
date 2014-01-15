using System;
using System.Linq;
using Roadkill.Core;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit;

namespace Roadkill.Tests.Unit
{
	/// <summary>
	/// Tests the FormsAuthUser SQL-based class (the default auth mechanism in Roadkill)
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class FormsAuthUserServiceTests
	{
		// TODO: Pattern up with Arrange, Act, Assert

		private MocksAndStubsContainer _container;
		private FormsAuthUserService _userService;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
			_userService = new FormsAuthUserService(_container.ApplicationSettings, _container.Repository);
		}

		[Test]
		public void AddAdmin_And_GetUserByEmail()
		{
			Assert.IsNull(_userService.GetUser("admin@localhost"));
			Assert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = _userService.GetUser("admin@localhost");
			Assert.IsNotNull(actual);
			Assert.IsNull(actual.ActivationKey);
			Assert.AreEqual("admin@localhost",actual.Email);
			Assert.IsNull(actual.Firstname);
			Assert.IsNull(actual.Lastname);
			Assert.IsNull(actual.PasswordResetKey);
			Assert.AreEqual("admin", actual.Username);
			Assert.AreEqual(true, actual.IsAdmin);
		}

		[Test]
		public void AddEditor_And_GetUserByEmail()
		{
			Assert.IsNull(_userService.GetUser("editor@localhost"));
			Assert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _userService.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
			Assert.IsNull(actual.ActivationKey);
			Assert.AreEqual("editor@localhost", actual.Email);
			Assert.IsNull(actual.Firstname);
			Assert.IsNull(actual.Lastname);
			Assert.IsNull(actual.PasswordResetKey);
			Assert.AreEqual("editor", actual.Username);
			Assert.AreEqual(false, actual.IsAdmin);
			Assert.AreEqual(true, actual.IsEditor);
		}

		[Test]
		public void AddUser_With_Existing_Username_ShouldFail()
		{
			Assert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _userService.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
			Assert.IsFalse(_userService.AddUser("editor2@localhost", "editor", "password", false, true));
		}

		[Test]
		public void AddUser_With_Existing_Email_ShouldFail()
		{
			Assert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _userService.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
			Assert.IsFalse(_userService.AddUser("editor@localhost", "editor2", "password", false, true));
		}

		[Test]
		public void Authenticate_Should_Succeed()
		{
			Assert.IsNull(_userService.GetUser("admin@localhost"));
			Assert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsTrue(_userService.Authenticate("admin@localhost", "password"));
		}

		[Test]
		public void Authenticate_BadUsername_ShouldFail()
		{
			Assert.IsNull(_userService.GetUser("admin@localhost"));
			Assert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsFalse(_userService.Authenticate("admin2@localhost", "password"));
		}

		[Test]
		public void Authenticate_BadPassword_ShouldFail()
		{
			Assert.IsNull(_userService.GetUser("admin@localhost"));
			Assert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsFalse(_userService.Authenticate("admin@localhost", "wrongpassword"));
		}

		[Test]
		public void ChangePassword_With_No_Existing_Password_And_Authenticate()
		{
			CreateEditorWithAsserts();

			_userService.ChangePassword("editor@localhost", "newpassword");
			Assert.IsTrue(_userService.Authenticate("editor@localhost", "newpassword"));
		}

		[Test]
		public void ChangePassword_Using_Correct_ExistingPassword()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_userService.ChangePassword("editor@localhost","password", "newpassword"));
		}

		[Test]
		public void ChangePassword_Using_Incorrect_ExistingPassword()
		{
			CreateEditorWithAsserts();
			Assert.IsFalse(_userService.ChangePassword("editor@localhost", "wrongpasword", "newpassword"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void ChangePassword_With_EmptyPassword_ShouldFail()
		{
			CreateEditorWithAsserts();
			_userService.ChangePassword("editor@localhost","");
		}

		[Test]
		public void DeleteUser()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_userService.DeleteUser("editor@localhost"));
			Assert.IsFalse(_userService.DeleteUser("editor2@localhost"));
		}

		[Test]
		public void GetUserById()
		{
			Assert.IsNull(_userService.GetUser("editor@localhost"));
			Assert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User expected = _userService.GetUser("editor@localhost");
			User actual = _userService.GetUserById(expected.Id);
			Assert.AreEqual(expected.Id, actual.Id);
			Assert.AreEqual("editor@localhost", actual.Email);

			Assert.IsNull(_userService.GetUserById(Guid.NewGuid()));
		}

		[Test]
		public void IsEditor_And_Is_Not_Admin()
		{
			CreateEditorWithAsserts();

			User actual = _userService.GetUser("editor@localhost");
			Assert.IsTrue(_userService.IsEditor(actual.Id.ToString()));
			Assert.IsFalse(_userService.IsAdmin(actual.Id.ToString()));
		}

		[Test]
		public void IsAdmin_And_IsEditor()
		{
			Assert.IsNull(_userService.GetUser("admin@localhost"));
			Assert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = _userService.GetUser("admin@localhost");
			Assert.IsNotNull(actual);

			Assert.IsTrue(_userService.IsEditor(actual.Id.ToString()));
			Assert.IsTrue(_userService.IsAdmin(actual.Id.ToString()));
		}

		[Test]
		public void ListAdmins_And_ListEditors()
		{
			Assert.IsTrue(_userService.AddUser("editor1@localhost", "editor1", "password", false, true));
			Assert.IsTrue(_userService.AddUser("editor2@localhost", "editor2", "password", false, true));
			Assert.IsTrue(_userService.AddUser("admin1@localhost", "admin1", "password", true, false));
			Assert.IsTrue(_userService.AddUser("admin2@localhost", "admin2", "password", true, false));

			Assert.AreEqual(2,_userService.ListAdmins().ToList().Count);
			Assert.AreEqual(2, _userService.ListEditors().ToList().Count);
		}

		[Test]
		public void ResetPassword()
		{
			CreateEditorWithAsserts();
			string key = _userService.ResetPassword("editor@localhost");

			Assert.IsNotNull(key);
			User actual = _userService.GetUser("editor@localhost");
			Assert.AreEqual(key, actual.PasswordResetKey);
		}

		[Test]
		public void Signup_And_Activate()
		{
			// Signup
			UserViewModel model = new UserViewModel();
			model.Firstname = "Harry";
			model.Lastname = "Houdini";
			model.NewEmail = "harry@localhost";
			model.NewUsername = "hazza100";
			model.Password = "password";

			string key = _userService.Signup(model,null);
			Assert.IsNotNull(key);

			User actual = _userService.GetUser("harry@localhost", false);
			Assert.IsNotNull(actual);
			Assert.AreEqual(key,actual.ActivationKey);

			//
			// Activate
			//
			Assert.IsTrue(_userService.ActivateUser(key));
			actual = _userService.GetUser("harry@localhost");
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.IsActivated);
		}

		[Test]
		public void ToggleAdmin_And_ToggleEditor()
		{
			CreateEditorWithAsserts();
			User actual = _userService.GetUser("editor@localhost");

			// Admin on
			_userService.ToggleAdmin("editor@localhost");
			actual = _userService.GetUser("editor@localhost");
			Assert.IsTrue(actual.IsAdmin);

			// Admin off
			_userService.ToggleAdmin("editor@localhost");
			actual = _userService.GetUser("editor@localhost");
			Assert.IsFalse(actual.IsAdmin);

			// Editor of
			_userService.ToggleEditor("editor@localhost");
			actual = _userService.GetUser("editor@localhost");
			Assert.IsFalse(actual.IsEditor);

			// Editor onn
			_userService.ToggleEditor("editor@localhost");
			actual = _userService.GetUser("editor@localhost");
			Assert.IsTrue(actual.IsEditor);
		}

		[Test]
		public void UserExists()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_userService.UserExists("editor@localhost"));
			Assert.IsFalse(_userService.UserExists("editor2@localhost"));
		}

		[Test]
		public void UsernameExists()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_userService.UserNameExists("editor"));
			Assert.IsFalse(_userService.UserNameExists("editor2"));
		}

		[Test]
		public void UpdateUser()
		{
			CreateEditorWithAsserts();

			// Update the user
			User actual = _userService.GetUser("editor@localhost");
			UserViewModel model = new UserViewModel(actual);
			model.Firstname = "Harold";
			model.Lastname = "Bishop";
			model.NewEmail = "harold@localhost";
			model.NewUsername = "harryB";
			Assert.IsTrue(_userService.UpdateUser(model));

			// Check the updates persisted
			actual = _userService.GetUser("harold@localhost");
			Assert.AreEqual("harold@localhost",actual.Email);
			Assert.AreEqual("harryB",actual.Username);
			Assert.AreEqual("Harold", actual.Firstname);
			Assert.AreEqual("Bishop",actual.Lastname);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void UpdateUser_With_Existing_Username_Fails()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_userService.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

			// Update the user
			User actual = _userService.GetUser("editor@localhost");
			UserViewModel model = new UserViewModel(actual);
			model.Firstname = "Harold";
			model.Lastname = "Bishop";
			model.NewEmail = "harold@localhost";
			model.NewUsername = "editor2";
			Assert.IsFalse(_userService.UpdateUser(model));
			Assert.IsFalse(_userService.Authenticate("harold@localhost", "password"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void UpdateUser_With_Existing_Email_Fails()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_userService.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

			// Update the user
			User actual = _userService.GetUser("editor@localhost");
			UserViewModel model = new UserViewModel(actual);
			model.Firstname = "Harold";
			model.Lastname = "Bishop";
			model.NewEmail = "editor2@localhost";
			model.NewUsername = "harryB";
			Assert.IsFalse(_userService.UpdateUser(model));
			Assert.IsFalse(_userService.Authenticate("editor2@localhost", "password"));
		}

		/// <summary>
		/// Helper for adding editor@localhost. Checks the user doesn't exist before, and does exist after the AddUser call.
		/// </summary>
		private void CreateEditorWithAsserts()
		{
			Assert.IsNull(_userService.GetUser("editor@localhost"));
			Assert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _userService.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
		}
	}
}
