using System;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Tests.Unit.Services
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
			_userService = new FormsAuthUserService(_container.ApplicationSettings, _container.UserRepository, _container.PageRepository);
		}

		[Test]
		public void addadmin_and_getuserbyemail()
		{
			ClassicAssert.IsNull(_userService.GetUser("admin@localhost"));
			ClassicAssert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = _userService.GetUser("admin@localhost");
			ClassicAssert.IsNotNull(actual);
			ClassicAssert.IsNull(actual.ActivationKey);
			ClassicAssert.AreEqual("admin@localhost",actual.Email);
			ClassicAssert.IsNull(actual.Firstname);
			ClassicAssert.IsNull(actual.Lastname);
			ClassicAssert.IsNull(actual.PasswordResetKey);
			ClassicAssert.AreEqual("admin", actual.Username);
			ClassicAssert.AreEqual(true, actual.IsAdmin);
		}

		[Test]
		public void addeditor_and_getuserbyemail()
		{
			ClassicAssert.IsNull(_userService.GetUser("editor@localhost"));
			ClassicAssert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _userService.GetUser("editor@localhost");
			ClassicAssert.IsNotNull(actual);
			ClassicAssert.IsNull(actual.ActivationKey);
			ClassicAssert.AreEqual("editor@localhost", actual.Email);
			ClassicAssert.IsNull(actual.Firstname);
			ClassicAssert.IsNull(actual.Lastname);
			ClassicAssert.IsNull(actual.PasswordResetKey);
			ClassicAssert.AreEqual("editor", actual.Username);
			ClassicAssert.AreEqual(false, actual.IsAdmin);
			ClassicAssert.AreEqual(true, actual.IsEditor);
		}

		[Test]
		public void adduser_with_existing_username_shouldfail()
		{
			ClassicAssert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _userService.GetUser("editor@localhost");
			ClassicAssert.IsNotNull(actual);
			ClassicAssert.IsFalse(_userService.AddUser("editor2@localhost", "editor", "password", false, true));
		}

		[Test]
		public void adduser_with_existing_email_shouldfail()
		{
			ClassicAssert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _userService.GetUser("editor@localhost");
			ClassicAssert.IsNotNull(actual);
			ClassicAssert.IsFalse(_userService.AddUser("editor@localhost", "editor2", "password", false, true));
		}

		[Test]
		public void authenticate_should_succeed()
		{
			ClassicAssert.IsNull(_userService.GetUser("admin@localhost"));
			ClassicAssert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			ClassicAssert.IsTrue(_userService.Authenticate("admin@localhost", "password"));
		}

		[Test]
		public void authenticate_badusername_shouldfail()
		{
			ClassicAssert.IsNull(_userService.GetUser("admin@localhost"));
			ClassicAssert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			ClassicAssert.IsFalse(_userService.Authenticate("admin2@localhost", "password"));
		}

		[Test]
		public void authenticate_badpassword_shouldfail()
		{
			ClassicAssert.IsNull(_userService.GetUser("admin@localhost"));
			ClassicAssert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			ClassicAssert.IsFalse(_userService.Authenticate("admin@localhost", "wrongpassword"));
		}

		[Test]
		public void changepassword_with_no_existing_password_and_authenticate()
		{
			CreateEditorWithAsserts();

			_userService.ChangePassword("editor@localhost", "newpassword");
			ClassicAssert.IsTrue(_userService.Authenticate("editor@localhost", "newpassword"));
		}

		[Test]
		public void changepassword_using_correct_existingpassword()
		{
			CreateEditorWithAsserts();
			ClassicAssert.IsTrue(_userService.ChangePassword("editor@localhost","password", "newpassword"));
		}

		[Test]
		public void changepassword_using_incorrect_existingpassword()
		{
			CreateEditorWithAsserts();
			ClassicAssert.IsFalse(_userService.ChangePassword("editor@localhost", "wrongpasword", "newpassword"));
		}

		[Test]
		public void ChangePassword_With_EmptyPassword_ShouldFail()
		{
			Assert.Throws<SecurityException>(() =>
			{
				CreateEditorWithAsserts();
				_userService.ChangePassword("editor@localhost","");
			});
		}

		[Test]
		public void deleteuser()
		{
			CreateEditorWithAsserts();
			ClassicAssert.IsTrue(_userService.DeleteUser("editor@localhost"));
			ClassicAssert.IsFalse(_userService.DeleteUser("editor2@localhost"));
		}

		[Test]
		public void getuserbyid()
		{
			ClassicAssert.IsNull(_userService.GetUser("editor@localhost"));
			ClassicAssert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User expected = _userService.GetUser("editor@localhost");
			User actual = _userService.GetUserById(expected.Id);
			ClassicAssert.AreEqual(expected.Id, actual.Id);
			ClassicAssert.AreEqual("editor@localhost", actual.Email);

			ClassicAssert.IsNull(_userService.GetUserById(Guid.NewGuid()));
		}

		[Test]
		public void iseditor_and_is_not_admin()
		{
			CreateEditorWithAsserts();

			User actual = _userService.GetUser("editor@localhost");
			ClassicAssert.IsTrue(_userService.IsEditor(actual.Id.ToString()));
			ClassicAssert.IsFalse(_userService.IsAdmin(actual.Id.ToString()));
		}

		[Test]
		public void isadmin_and_iseditor()
		{
			ClassicAssert.IsNull(_userService.GetUser("admin@localhost"));
			ClassicAssert.IsTrue(_userService.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = _userService.GetUser("admin@localhost");
			ClassicAssert.IsNotNull(actual);

			ClassicAssert.IsTrue(_userService.IsEditor(actual.Id.ToString()));
			ClassicAssert.IsTrue(_userService.IsAdmin(actual.Id.ToString()));
		}

		[Test]
		public void listadmins_and_listeditors()
		{
			ClassicAssert.IsTrue(_userService.AddUser("editor1@localhost", "editor1", "password", false, true));
			ClassicAssert.IsTrue(_userService.AddUser("editor2@localhost", "editor2", "password", false, true));
			ClassicAssert.IsTrue(_userService.AddUser("admin1@localhost", "admin1", "password", true, false));
			ClassicAssert.IsTrue(_userService.AddUser("admin2@localhost", "admin2", "password", true, false));

			ClassicAssert.AreEqual(2,_userService.ListAdmins().ToList().Count);
			ClassicAssert.AreEqual(2, _userService.ListEditors().ToList().Count);
		}

		[Test]
		public void resetpassword()
		{
			CreateEditorWithAsserts();
			string key = _userService.ResetPassword("editor@localhost");

			ClassicAssert.IsNotNull(key);
			User actual = _userService.GetUser("editor@localhost");
			ClassicAssert.AreEqual(key, actual.PasswordResetKey);
		}

		[Test]
		public void signup_and_activate()
		{
			// Signup
			UserViewModel model = new UserViewModel();
			model.Firstname = "Harry";
			model.Lastname = "Houdini";
			model.NewEmail = "harry@localhost";
			model.NewUsername = "hazza100";
			model.Password = "password";

			string key = _userService.Signup(model,null);
			ClassicAssert.IsNotNull(key);

			User actual = _userService.GetUser("harry@localhost", false);
			ClassicAssert.IsNotNull(actual);
			ClassicAssert.AreEqual(key,actual.ActivationKey);

			//
			// Activate
			//
			ClassicAssert.IsTrue(_userService.ActivateUser(key));
			actual = _userService.GetUser("harry@localhost");
			ClassicAssert.IsNotNull(actual);
			ClassicAssert.IsTrue(actual.IsActivated);
		}

		[Test]
		public void toggleadmin_and_toggleeditor()
		{
			CreateEditorWithAsserts();
			User actual = _userService.GetUser("editor@localhost");

			// Admin on
			_userService.ToggleAdmin("editor@localhost");
			actual = _userService.GetUser("editor@localhost");
			ClassicAssert.IsTrue(actual.IsAdmin);

			// Admin off
			_userService.ToggleAdmin("editor@localhost");
			actual = _userService.GetUser("editor@localhost");
			ClassicAssert.IsFalse(actual.IsAdmin);

			// Editor of
			_userService.ToggleEditor("editor@localhost");
			actual = _userService.GetUser("editor@localhost");
			ClassicAssert.IsFalse(actual.IsEditor);

			// Editor onn
			_userService.ToggleEditor("editor@localhost");
			actual = _userService.GetUser("editor@localhost");
			ClassicAssert.IsTrue(actual.IsEditor);
		}

		[Test]
		public void userexists()
		{
			CreateEditorWithAsserts();
			ClassicAssert.IsTrue(_userService.UserExists("editor@localhost"));
			ClassicAssert.IsFalse(_userService.UserExists("editor2@localhost"));
		}

		[Test]
		public void usernameexists()
		{
			CreateEditorWithAsserts();
			ClassicAssert.IsTrue(_userService.UserNameExists("editor"));
			ClassicAssert.IsFalse(_userService.UserNameExists("editor2"));
		}

		[Test]
		public void updateuser()
		{
			CreateEditorWithAsserts();

			// Update the user
			User actual = _userService.GetUser("editor@localhost");
			UserViewModel model = new UserViewModel(actual);
			model.Firstname = "Harold";
			model.Lastname = "Bishop";
			model.NewEmail = "harold@localhost";
			model.NewUsername = "harryB";
			ClassicAssert.IsTrue(_userService.UpdateUser(model));

			// Check the updates persisted
			actual = _userService.GetUser("harold@localhost");
			ClassicAssert.AreEqual("harold@localhost",actual.Email);
			ClassicAssert.AreEqual("harryB",actual.Username);
			ClassicAssert.AreEqual("Harold", actual.Firstname);
			ClassicAssert.AreEqual("Bishop",actual.Lastname);
		}

		[Test]
		public void UpdateUser_With_Existing_Username_Fails()
		{
			Assert.Throws<SecurityException>(() =>
			{
				CreateEditorWithAsserts();
				ClassicAssert.IsTrue(_userService.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

				// Update the user
				User actual = _userService.GetUser("editor@localhost");
				UserViewModel model = new UserViewModel(actual);
				model.Firstname = "Harold";
				model.Lastname = "Bishop";
				model.NewEmail = "harold@localhost";
				model.NewUsername = "editor2";
				ClassicAssert.IsFalse(_userService.UpdateUser(model));
				ClassicAssert.IsFalse(_userService.Authenticate("harold@localhost", "password"));
			});
		}

		[Test]
		public void UpdateUser_With_Existing_Email_Fails()
		{
			Assert.Throws<SecurityException>(() =>
			{
				CreateEditorWithAsserts();
				ClassicAssert.IsTrue(_userService.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

				// Update the user
				User actual = _userService.GetUser("editor@localhost");
				UserViewModel model = new UserViewModel(actual);
				model.Firstname = "Harold";
				model.Lastname = "Bishop";
				model.NewEmail = "editor2@localhost";
				model.NewUsername = "harryB";
				ClassicAssert.IsFalse(_userService.UpdateUser(model));
				ClassicAssert.IsFalse(_userService.Authenticate("editor2@localhost", "password"));
			});
		}

		/// <summary>
		/// Helper for adding editor@localhost. Checks the user doesn't exist before, and does exist after the AddUser call.
		/// </summary>
		private void CreateEditorWithAsserts()
		{
			ClassicAssert.IsNull(_userService.GetUser("editor@localhost"));
			ClassicAssert.IsTrue(_userService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _userService.GetUser("editor@localhost");
			ClassicAssert.IsNotNull(actual);
		}
	}
}
