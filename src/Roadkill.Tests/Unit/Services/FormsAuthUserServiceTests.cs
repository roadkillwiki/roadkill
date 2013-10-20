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
	/// Tests the SQL User manager class (the default auth mechanism in Roadkill)
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class FormsAuthUserServiceTests
	{
		private FormsAuthUserService _defaultUserService;

		// TODO: pattern up the tests using arrange, act, assert

		[SetUp]
		public void Setup()
		{
			ApplicationSettings settings = new ApplicationSettings();
			settings.Installed = true;

			IRepository repository = new LightSpeedRepository(settings);
			repository = new RepositoryMock();

			_defaultUserService = new FormsAuthUserService(settings, repository);
		}

		[Test]
		public void AddAdmin_And_GetUserByEmail()
		{
			Assert.IsNull(_defaultUserService.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserService.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = _defaultUserService.GetUser("admin@localhost");
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
			Assert.IsNull(_defaultUserService.GetUser("editor@localhost"));
			Assert.IsTrue(_defaultUserService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _defaultUserService.GetUser("editor@localhost");
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
			Assert.IsTrue(_defaultUserService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _defaultUserService.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
			Assert.IsFalse(_defaultUserService.AddUser("editor2@localhost", "editor", "password", false, true));
		}

		[Test]
		public void AddUser_With_Existing_Email_ShouldFail()
		{
			Assert.IsTrue(_defaultUserService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _defaultUserService.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
			Assert.IsFalse(_defaultUserService.AddUser("editor@localhost", "editor2", "password", false, true));
		}

		[Test]
		public void Authenticate_Should_Succeed()
		{
			Assert.IsNull(_defaultUserService.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserService.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsTrue(_defaultUserService.Authenticate("admin@localhost", "password"));
		}

		[Test]
		public void Authenticate_BadUsername_ShouldFail()
		{
			Assert.IsNull(_defaultUserService.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserService.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsFalse(_defaultUserService.Authenticate("admin2@localhost", "password"));
		}

		[Test]
		public void Authenticate_BadPassword_ShouldFail()
		{
			Assert.IsNull(_defaultUserService.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserService.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsFalse(_defaultUserService.Authenticate("admin@localhost", "wrongpassword"));
		}

		[Test]
		public void ChangePassword_With_No_Existing_Password_And_Authenticate()
		{
			CreateEditorWithAsserts();

			_defaultUserService.ChangePassword("editor@localhost", "newpassword");
			Assert.IsTrue(_defaultUserService.Authenticate("editor@localhost", "newpassword"));
		}

		[Test]
		public void ChangePassword_Using_Correct_ExistingPassword()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserService.ChangePassword("editor@localhost","password", "newpassword"));
		}

		[Test]
		public void ChangePassword_Using_Incorrect_ExistingPassword()
		{
			CreateEditorWithAsserts();
			Assert.IsFalse(_defaultUserService.ChangePassword("editor@localhost", "wrongpasword", "newpassword"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void ChangePassword_With_EmptyPassword_ShouldFail()
		{
			CreateEditorWithAsserts();
			_defaultUserService.ChangePassword("editor@localhost","");
		}

		[Test]
		public void DeleteUser()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserService.DeleteUser("editor@localhost"));
			Assert.IsFalse(_defaultUserService.DeleteUser("editor2@localhost"));
		}

		[Test]
		public void GetUserById()
		{
			Assert.IsNull(_defaultUserService.GetUser("editor@localhost"));
			Assert.IsTrue(_defaultUserService.AddUser("editor@localhost", "editor", "password", false, true));

			User expected = _defaultUserService.GetUser("editor@localhost");
			User actual = _defaultUserService.GetUserById(expected.Id);
			Assert.AreEqual(expected.Id, actual.Id);
			Assert.AreEqual("editor@localhost", actual.Email);

			Assert.IsNull(_defaultUserService.GetUserById(Guid.NewGuid()));
		}

		[Test]
		public void IsEditor_And_Is_Not_Admin()
		{
			CreateEditorWithAsserts();

			User actual = _defaultUserService.GetUser("editor@localhost");
			Assert.IsTrue(_defaultUserService.IsEditor(actual.Id.ToString()));
			Assert.IsFalse(_defaultUserService.IsAdmin(actual.Id.ToString()));
		}

		[Test]
		public void IsAdmin_And_IsEditor()
		{
			Assert.IsNull(_defaultUserService.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserService.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = _defaultUserService.GetUser("admin@localhost");
			Assert.IsNotNull(actual);

			Assert.IsTrue(_defaultUserService.IsEditor(actual.Id.ToString()));
			Assert.IsTrue(_defaultUserService.IsAdmin(actual.Id.ToString()));
		}

		[Test]
		public void ListAdmins_And_ListEditors()
		{
			Assert.IsTrue(_defaultUserService.AddUser("editor1@localhost", "editor1", "password", false, true));
			Assert.IsTrue(_defaultUserService.AddUser("editor2@localhost", "editor2", "password", false, true));
			Assert.IsTrue(_defaultUserService.AddUser("admin1@localhost", "admin1", "password", true, false));
			Assert.IsTrue(_defaultUserService.AddUser("admin2@localhost", "admin2", "password", true, false));

			Assert.AreEqual(2,_defaultUserService.ListAdmins().ToList().Count);
			Assert.AreEqual(2, _defaultUserService.ListEditors().ToList().Count);
		}

		[Test]
		public void ResetPassword()
		{
			CreateEditorWithAsserts();
			string key = _defaultUserService.ResetPassword("editor@localhost");

			Assert.IsNotNull(key);
			User actual = _defaultUserService.GetUser("editor@localhost");
			Assert.AreEqual(key, actual.PasswordResetKey);
		}

		[Test]
		public void Signup_And_Activate()
		{
			// Signup
			UserViewModel summary = new UserViewModel();
			summary.Firstname = "Harry";
			summary.Lastname = "Houdini";
			summary.NewEmail = "harry@localhost";
			summary.NewUsername = "hazza100";
			summary.Password = "password";

			string key = _defaultUserService.Signup(summary,null);
			Assert.IsNotNull(key);

			User actual = _defaultUserService.GetUser("harry@localhost", false);
			Assert.IsNotNull(actual);
			Assert.AreEqual(key,actual.ActivationKey);

			//
			// Activate
			//
			Assert.IsTrue(_defaultUserService.ActivateUser(key));
			actual = _defaultUserService.GetUser("harry@localhost");
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.IsActivated);
		}

		[Test]
		public void ToggleAdmin_And_ToggleEditor()
		{
			CreateEditorWithAsserts();
			User actual = _defaultUserService.GetUser("editor@localhost");

			// Admin on
			_defaultUserService.ToggleAdmin("editor@localhost");
			actual = _defaultUserService.GetUser("editor@localhost");
			Assert.IsTrue(actual.IsAdmin);

			// Admin off
			_defaultUserService.ToggleAdmin("editor@localhost");
			actual = _defaultUserService.GetUser("editor@localhost");
			Assert.IsFalse(actual.IsAdmin);

			// Editor of
			_defaultUserService.ToggleEditor("editor@localhost");
			actual = _defaultUserService.GetUser("editor@localhost");
			Assert.IsFalse(actual.IsEditor);

			// Editor onn
			_defaultUserService.ToggleEditor("editor@localhost");
			actual = _defaultUserService.GetUser("editor@localhost");
			Assert.IsTrue(actual.IsEditor);
		}

		[Test]
		public void UserExists()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserService.UserExists("editor@localhost"));
			Assert.IsFalse(_defaultUserService.UserExists("editor2@localhost"));
		}

		[Test]
		public void UsernameExists()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserService.UserNameExists("editor"));
			Assert.IsFalse(_defaultUserService.UserNameExists("editor2"));
		}

		[Test]
		public void UpdateUser()
		{
			CreateEditorWithAsserts();

			// Update the user
			User actual = _defaultUserService.GetUser("editor@localhost");
			UserViewModel summary = actual.ToSummary();
			summary.Firstname = "Harold";
			summary.Lastname = "Bishop";
			summary.NewEmail = "harold@localhost";
			summary.NewUsername = "harryB";
			Assert.IsTrue(_defaultUserService.UpdateUser(summary));

			// Check the updates persisted
			actual = _defaultUserService.GetUser("harold@localhost");
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
			Assert.IsTrue(_defaultUserService.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

			// Update the user
			User actual = _defaultUserService.GetUser("editor@localhost");
			UserViewModel summary = actual.ToSummary();
			summary.Firstname = "Harold";
			summary.Lastname = "Bishop";
			summary.NewEmail = "harold@localhost";
			summary.NewUsername = "editor2";
			Assert.IsFalse(_defaultUserService.UpdateUser(summary));
			Assert.IsFalse(_defaultUserService.Authenticate("harold@localhost", "password"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void UpdateUser_With_Existing_Email_Fails()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserService.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

			// Update the user
			User actual = _defaultUserService.GetUser("editor@localhost");
			UserViewModel summary = actual.ToSummary();
			summary.Firstname = "Harold";
			summary.Lastname = "Bishop";
			summary.NewEmail = "editor2@localhost";
			summary.NewUsername = "harryB";
			Assert.IsFalse(_defaultUserService.UpdateUser(summary));
			Assert.IsFalse(_defaultUserService.Authenticate("editor2@localhost", "password"));
		}

		/// <summary>
		/// Helper for adding editor@localhost. Checks the user doesn't exist before, and does exist after the AddUser call.
		/// </summary>
		private void CreateEditorWithAsserts()
		{
			Assert.IsNull(_defaultUserService.GetUser("editor@localhost"));
			Assert.IsTrue(_defaultUserService.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _defaultUserService.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
		}
	}
}
