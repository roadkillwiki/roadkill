using System;
using System.Linq;
using Roadkill.Core;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Security;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit;

namespace Roadkill.Tests.Unit
{
	/// <summary>
	/// Tests the SQL User manager class (the default auth mechanism in Roadkill)
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class FormsAuthUserManagerTests
	{
		private FormsAuthUserManager _defaultUserManager;

		[SetUp]
		public void Setup()
		{
			ApplicationSettings settings = new ApplicationSettings();
			settings.Installed = true;

			IRepository repository = new LightSpeedRepository(settings);
			repository = new RepositoryMock();

			_defaultUserManager = new FormsAuthUserManager(settings, repository);
		}

		[Test]
		public void AddAdmin_And_GetUserByEmail()
		{
			Assert.IsNull(_defaultUserManager.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserManager.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = _defaultUserManager.GetUser("admin@localhost");
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
			Assert.IsNull(_defaultUserManager.GetUser("editor@localhost"));
			Assert.IsTrue(_defaultUserManager.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _defaultUserManager.GetUser("editor@localhost");
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
			Assert.IsTrue(_defaultUserManager.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _defaultUserManager.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
			Assert.IsFalse(_defaultUserManager.AddUser("editor2@localhost", "editor", "password", false, true));
		}

		[Test]
		public void AddUser_With_Existing_Email_ShouldFail()
		{
			Assert.IsTrue(_defaultUserManager.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _defaultUserManager.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
			Assert.IsFalse(_defaultUserManager.AddUser("editor@localhost", "editor2", "password", false, true));
		}

		[Test]
		public void Authenticate_Should_Succeed()
		{
			Assert.IsNull(_defaultUserManager.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserManager.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsTrue(_defaultUserManager.Authenticate("admin@localhost", "password"));
		}

		[Test]
		public void Authenticate_BadUsername_ShouldFail()
		{
			Assert.IsNull(_defaultUserManager.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserManager.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsFalse(_defaultUserManager.Authenticate("admin2@localhost", "password"));
		}

		[Test]
		public void Authenticate_BadPassword_ShouldFail()
		{
			Assert.IsNull(_defaultUserManager.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserManager.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsFalse(_defaultUserManager.Authenticate("admin@localhost", "wrongpassword"));
		}

		[Test]
		public void ChangePassword_With_No_Existing_Password_And_Authenticate()
		{
			CreateEditorWithAsserts();

			_defaultUserManager.ChangePassword("editor@localhost", "newpassword");
			Assert.IsTrue(_defaultUserManager.Authenticate("editor@localhost", "newpassword"));
		}

		[Test]
		public void ChangePassword_Using_Correct_ExistingPassword()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserManager.ChangePassword("editor@localhost","password", "newpassword"));
		}

		[Test]
		public void ChangePassword_Using_Incorrect_ExistingPassword()
		{
			CreateEditorWithAsserts();
			Assert.IsFalse(_defaultUserManager.ChangePassword("editor@localhost", "wrongpasword", "newpassword"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void ChangePassword_With_EmptyPassword_ShouldFail()
		{
			CreateEditorWithAsserts();
			_defaultUserManager.ChangePassword("editor@localhost","");
		}

		[Test]
		public void DeleteUser()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserManager.DeleteUser("editor@localhost"));
			Assert.IsFalse(_defaultUserManager.DeleteUser("editor2@localhost"));
		}

		[Test]
		public void GetUserById()
		{
			Assert.IsNull(_defaultUserManager.GetUser("editor@localhost"));
			Assert.IsTrue(_defaultUserManager.AddUser("editor@localhost", "editor", "password", false, true));

			User expected = _defaultUserManager.GetUser("editor@localhost");
			User actual = _defaultUserManager.GetUserById(expected.Id);
			Assert.AreEqual(expected.Id, actual.Id);
			Assert.AreEqual("editor@localhost", actual.Email);

			Assert.IsNull(_defaultUserManager.GetUserById(Guid.NewGuid()));
		}

		[Test]
		public void IsEditor_And_Is_Not_Admin()
		{
			CreateEditorWithAsserts();

			User actual = _defaultUserManager.GetUser("editor@localhost");
			Assert.IsTrue(_defaultUserManager.IsEditor(actual.Id.ToString()));
			Assert.IsFalse(_defaultUserManager.IsAdmin(actual.Id.ToString()));
		}

		[Test]
		public void IsAdmin_And_IsEditor()
		{
			Assert.IsNull(_defaultUserManager.GetUser("admin@localhost"));
			Assert.IsTrue(_defaultUserManager.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = _defaultUserManager.GetUser("admin@localhost");
			Assert.IsNotNull(actual);

			Assert.IsTrue(_defaultUserManager.IsEditor(actual.Id.ToString()));
			Assert.IsTrue(_defaultUserManager.IsAdmin(actual.Id.ToString()));
		}

		[Test]
		public void ListAdmins_And_ListEditors()
		{
			Assert.IsTrue(_defaultUserManager.AddUser("editor1@localhost", "editor1", "password", false, true));
			Assert.IsTrue(_defaultUserManager.AddUser("editor2@localhost", "editor2", "password", false, true));
			Assert.IsTrue(_defaultUserManager.AddUser("admin1@localhost", "admin1", "password", true, false));
			Assert.IsTrue(_defaultUserManager.AddUser("admin2@localhost", "admin2", "password", true, false));

			Assert.AreEqual(2,_defaultUserManager.ListAdmins().ToList().Count);
			Assert.AreEqual(2, _defaultUserManager.ListEditors().ToList().Count);
		}

		[Test]
		public void ResetPassword()
		{
			CreateEditorWithAsserts();
			string key = _defaultUserManager.ResetPassword("editor@localhost");

			Assert.IsNotNull(key);
			User actual = _defaultUserManager.GetUser("editor@localhost");
			Assert.AreEqual(key, actual.PasswordResetKey);
		}

		[Test]
		public void Signup_And_Activate()
		{
			// Signup
			UserSummary summary = new UserSummary();
			summary.Firstname = "Harry";
			summary.Lastname = "Houdini";
			summary.NewEmail = "harry@localhost";
			summary.NewUsername = "hazza100";
			summary.Password = "password";

			string key = _defaultUserManager.Signup(summary,null);
			Assert.IsNotNull(key);

			User actual = _defaultUserManager.GetUser("harry@localhost", false);
			Assert.IsNotNull(actual);
			Assert.AreEqual(key,actual.ActivationKey);

			//
			// Activate
			//
			Assert.IsTrue(_defaultUserManager.ActivateUser(key));
			actual = _defaultUserManager.GetUser("harry@localhost");
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.IsActivated);
		}

		[Test]
		public void ToggleAdmin_And_ToggleEditor()
		{
			CreateEditorWithAsserts();
			User actual = _defaultUserManager.GetUser("editor@localhost");

			// Admin on
			_defaultUserManager.ToggleAdmin("editor@localhost");
			actual = _defaultUserManager.GetUser("editor@localhost");
			Assert.IsTrue(actual.IsAdmin);

			// Admin off
			_defaultUserManager.ToggleAdmin("editor@localhost");
			actual = _defaultUserManager.GetUser("editor@localhost");
			Assert.IsFalse(actual.IsAdmin);

			// Editor of
			_defaultUserManager.ToggleEditor("editor@localhost");
			actual = _defaultUserManager.GetUser("editor@localhost");
			Assert.IsFalse(actual.IsEditor);

			// Editor onn
			_defaultUserManager.ToggleEditor("editor@localhost");
			actual = _defaultUserManager.GetUser("editor@localhost");
			Assert.IsTrue(actual.IsEditor);
		}

		[Test]
		public void UserExists()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserManager.UserExists("editor@localhost"));
			Assert.IsFalse(_defaultUserManager.UserExists("editor2@localhost"));
		}

		[Test]
		public void UsernameExists()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserManager.UserNameExists("editor"));
			Assert.IsFalse(_defaultUserManager.UserNameExists("editor2"));
		}

		[Test]
		public void UpdateUser()
		{
			CreateEditorWithAsserts();

			// Update the user
			User actual = _defaultUserManager.GetUser("editor@localhost");
			UserSummary summary = actual.ToSummary();
			summary.Firstname = "Harold";
			summary.Lastname = "Bishop";
			summary.NewEmail = "harold@localhost";
			summary.NewUsername = "harryB";
			Assert.IsTrue(_defaultUserManager.UpdateUser(summary));

			// Check the updates persisted
			actual = _defaultUserManager.GetUser("harold@localhost");
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
			Assert.IsTrue(_defaultUserManager.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

			// Update the user
			User actual = _defaultUserManager.GetUser("editor@localhost");
			UserSummary summary = actual.ToSummary();
			summary.Firstname = "Harold";
			summary.Lastname = "Bishop";
			summary.NewEmail = "harold@localhost";
			summary.NewUsername = "editor2";
			Assert.IsFalse(_defaultUserManager.UpdateUser(summary));
			Assert.IsFalse(_defaultUserManager.Authenticate("harold@localhost", "password"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void UpdateUser_With_Existing_Email_Fails()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(_defaultUserManager.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

			// Update the user
			User actual = _defaultUserManager.GetUser("editor@localhost");
			UserSummary summary = actual.ToSummary();
			summary.Firstname = "Harold";
			summary.Lastname = "Bishop";
			summary.NewEmail = "editor2@localhost";
			summary.NewUsername = "harryB";
			Assert.IsFalse(_defaultUserManager.UpdateUser(summary));
			Assert.IsFalse(_defaultUserManager.Authenticate("editor2@localhost", "password"));
		}

		/// <summary>
		/// Helper for adding editor@localhost. Checks the user doesn't exist before, and does exist after the AddUser call.
		/// </summary>
		private void CreateEditorWithAsserts()
		{
			Assert.IsNull(_defaultUserManager.GetUser("editor@localhost"));
			Assert.IsTrue(_defaultUserManager.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = _defaultUserManager.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
		}
	}
}
