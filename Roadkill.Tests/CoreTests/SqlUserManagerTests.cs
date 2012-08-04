using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core;
using Roadkill.Tests.Core;
using NUnit.Framework;

namespace Roadkill.Tests.Core
{
	/// <summary>
	/// Tests the SQL User manager class (the default auth mechanism in Roadkill)
	/// </summary>
	[TestFixture]
	public class SqlUserManagerTests : TestBase
	{
		[Test]
		public void AddAdmin_And_GetUserByEmail()
		{
			Assert.IsNull(UserManager.Current.GetUser("admin@localhost"));
			Assert.IsTrue(UserManager.Current.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = UserManager.Current.GetUser("admin@localhost");
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
			Assert.IsNull(UserManager.Current.GetUser("editor@localhost"));
			Assert.IsTrue(UserManager.Current.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = UserManager.Current.GetUser("editor@localhost");
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
			Assert.IsTrue(UserManager.Current.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = UserManager.Current.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
			Assert.IsFalse(UserManager.Current.AddUser("editor2@localhost", "editor", "password", false, true));
		}

		[Test]
		public void AddUser_With_Existing_Email_ShouldFail()
		{
			Assert.IsTrue(UserManager.Current.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = UserManager.Current.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
			Assert.IsFalse(UserManager.Current.AddUser("editor@localhost", "editor2", "password", false, true));
		}

		[Test]
		public void Authenticate_Should_Succeed()
		{
			Assert.IsNull(UserManager.Current.GetUser("admin@localhost"));
			Assert.IsTrue(UserManager.Current.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsTrue(UserManager.Current.Authenticate("admin@localhost", "password"));
		}

		[Test]
		public void Authenticate_BadUsername_ShouldFail()
		{
			Assert.IsNull(UserManager.Current.GetUser("admin@localhost"));
			Assert.IsTrue(UserManager.Current.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsFalse(UserManager.Current.Authenticate("admin2@localhost", "password"));
		}

		[Test]
		public void Authenticate_BadPassword_ShouldFail()
		{
			Assert.IsNull(UserManager.Current.GetUser("admin@localhost"));
			Assert.IsTrue(UserManager.Current.AddUser("admin@localhost", "admin", "password", true, true));

			Assert.IsFalse(UserManager.Current.Authenticate("admin@localhost", "wrongpassword"));
		}

		[Test]
		public void ChangePassword_With_No_Existing_Password_And_Authenticate()
		{
			CreateEditorWithAsserts();

			UserManager.Current.ChangePassword("editor@localhost", "newpassword");
			Assert.IsTrue(UserManager.Current.Authenticate("editor@localhost", "newpassword"));
		}

		[Test]
		public void ChangePassword_Using_Correct_ExistingPassword()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(UserManager.Current.ChangePassword("editor@localhost","password", "newpassword"));
		}

		[Test]
		public void ChangePassword_Using_Incorrect_ExistingPassword()
		{
			CreateEditorWithAsserts();
			Assert.IsFalse(UserManager.Current.ChangePassword("editor@localhost", "wrongpasword", "newpassword"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void ChangePassword_With_EmptyPassword_ShouldFail()
		{
			CreateEditorWithAsserts();
			UserManager.Current.ChangePassword("editor@localhost","");
		}

		[Test]
		public void DeleteUser()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(UserManager.Current.DeleteUser("editor@localhost"));
			Assert.IsFalse(UserManager.Current.DeleteUser("editor2@localhost"));
		}

		[Test]
		public void GetUserById()
		{
			Assert.IsNull(UserManager.Current.GetUser("editor@localhost"));
			Assert.IsTrue(UserManager.Current.AddUser("editor@localhost", "editor", "password", false, true));

			User expected = UserManager.Current.GetUser("editor@localhost");
			User actual = UserManager.Current.GetUserById(expected.Id);
			Assert.AreEqual(expected.Id, actual.Id);
			Assert.AreEqual("editor@localhost", actual.Email);

			Assert.IsNull(UserManager.Current.GetUserById(new Guid()));
		}

		[Test]
		public void IsEditor_And_Is_Not_Admin()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(UserManager.Current.IsEditor("editor@localhost"));
			Assert.IsFalse(UserManager.Current.IsAdmin("editor@localhost"));
		}

		[Test]
		public void IsAdmin_And_IsEditor()
		{
			Assert.IsNull(UserManager.Current.GetUser("admin@localhost"));
			Assert.IsTrue(UserManager.Current.AddUser("admin@localhost", "admin", "password", true, true));

			User actual = UserManager.Current.GetUser("admin@localhost");
			Assert.IsNotNull(actual);

			Assert.IsTrue(UserManager.Current.IsEditor("admin@localhost"));
			Assert.IsTrue(UserManager.Current.IsAdmin("admin@localhost"));
		}

		[Test]
		public void ListAdmins_And_ListEditors()
		{
			Assert.IsTrue(UserManager.Current.AddUser("editor1@localhost", "editor1", "password", false, true));
			Assert.IsTrue(UserManager.Current.AddUser("editor2@localhost", "editor2", "password", false, true));
			Assert.IsTrue(UserManager.Current.AddUser("admin1@localhost", "admin1", "password", true, false));
			Assert.IsTrue(UserManager.Current.AddUser("admin2@localhost", "admin2", "password", true, false));

			Assert.AreEqual(2,UserManager.Current.ListAdmins().ToList().Count);
			Assert.AreEqual(2, UserManager.Current.ListEditors().ToList().Count);
		}

		[Test]
		public void ResetPassword()
		{
			CreateEditorWithAsserts();
			string key = UserManager.Current.ResetPassword("editor@localhost");

			Assert.IsNotNull(key);
			User actual = UserManager.Current.GetUser("editor@localhost");
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

			string key = UserManager.Current.Signup(summary,null);
			Assert.IsNotNull(key);

			User actual = UserManager.Current.GetUser("harry@localhost");
			Assert.IsNotNull(actual);
			Assert.AreEqual(key,actual.ActivationKey);

			//
			// Activate
			//
			Assert.IsTrue(UserManager.Current.ActivateUser(key));
			actual = UserManager.Current.GetUser("harry@localhost");
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.IsActivated);
		}

		[Test]
		public void ToggleAdmin_And_ToggleEditor()
		{
			CreateEditorWithAsserts();
			User actual = UserManager.Current.GetUser("editor@localhost");

			// Admin on
			UserManager.Current.ToggleAdmin("editor@localhost");
			actual = UserManager.Current.GetUser("editor@localhost");
			Assert.IsTrue(actual.IsAdmin);

			// Admin off
			UserManager.Current.ToggleAdmin("editor@localhost");
			actual = UserManager.Current.GetUser("editor@localhost");
			Assert.IsFalse(actual.IsAdmin);

			// Editor of
			UserManager.Current.ToggleEditor("editor@localhost");
			actual = UserManager.Current.GetUser("editor@localhost");
			Assert.IsFalse(actual.IsEditor);

			// Editor onn
			UserManager.Current.ToggleEditor("editor@localhost");
			actual = UserManager.Current.GetUser("editor@localhost");
			Assert.IsTrue(actual.IsEditor);
		}

		[Test]
		public void UserExists()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(UserManager.Current.UserExists("editor@localhost"));
			Assert.IsFalse(UserManager.Current.UserExists("editor2@localhost"));
		}

		[Test]
		public void UsernameExists()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(UserManager.Current.UserNameExists("editor"));
			Assert.IsFalse(UserManager.Current.UserNameExists("editor2"));
		}

		[Test]
		public void UpdateUser()
		{
			CreateEditorWithAsserts();

			// Update the user
			User actual = UserManager.Current.GetUser("editor@localhost");
			UserSummary summary = actual.ToSummary();
			summary.Firstname = "Harold";
			summary.Lastname = "Bishop";
			summary.NewEmail = "harold@localhost";
			summary.NewUsername = "harryB";
			Assert.IsTrue(UserManager.Current.UpdateUser(summary));

			// Check the updates persisted
			actual = UserManager.Current.GetUser("harold@localhost");
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
			Assert.IsTrue(UserManager.Current.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

			// Update the user
			User actual = UserManager.Current.GetUser("editor@localhost");
			UserSummary summary = actual.ToSummary();
			summary.Firstname = "Harold";
			summary.Lastname = "Bishop";
			summary.NewEmail = "harold@localhost";
			summary.NewUsername = "editor2";
			Assert.IsFalse(UserManager.Current.UpdateUser(summary));
			Assert.IsFalse(UserManager.Current.Authenticate("harold@localhost", "password"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void UpdateUser_With_Existing_Email_Fails()
		{
			CreateEditorWithAsserts();
			Assert.IsTrue(UserManager.Current.AddUser("editor2@localhost", "editor2", "anotherpassword", true, true));

			// Update the user
			User actual = UserManager.Current.GetUser("editor@localhost");
			UserSummary summary = actual.ToSummary();
			summary.Firstname = "Harold";
			summary.Lastname = "Bishop";
			summary.NewEmail = "editor2@localhost";
			summary.NewUsername = "harryB";
			Assert.IsFalse(UserManager.Current.UpdateUser(summary));
			Assert.IsFalse(UserManager.Current.Authenticate("editor2@localhost", "password"));
		}

		/// <summary>
		/// Helper for adding editor@localhost. Checks the user doesn't exist before, and does exist after the AddUser call.
		/// </summary>
		private void CreateEditorWithAsserts()
		{
			Assert.IsNull(UserManager.Current.GetUser("editor@localhost"));
			Assert.IsTrue(UserManager.Current.AddUser("editor@localhost", "editor", "password", false, true));

			User actual = UserManager.Current.GetUser("editor@localhost");
			Assert.IsNotNull(actual);
		}
	}
}
