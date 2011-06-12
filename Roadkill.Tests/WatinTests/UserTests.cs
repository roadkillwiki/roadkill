using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Roadkill.Tests.WatinTests
{
	/// <summary>
	/// For these tests to work correctly, disable recaptcha.
	/// </summary>
	/// <remarks>
	/// Tests that need to be performed manually:
	// - Activate an account and login
	// - Reset password and login
	/// </remarks>
	[TestClass]
	public class UserTests : WatinTestBase
	{

		[TestMethod]
		public void Register_With_Valid_Data()
		{
			using (NewSession())
			{
				GoToUrl("/user/signup");

				SetTextfieldValue("Firstname", "Watin");
				SetTextfieldValue("Lastname", "Tester");
				SetTextfieldValue("NewEmail", "watin@localhost" +Guid.NewGuid());
				SetTextfieldValue("NewUsername", "Watinuser" + Guid.NewGuid());
				SetTextfieldValue("Password", "password");
				SetTextfieldValue("PasswordConfirmation", "password");

				ClickButton("userbutton", "Failed to click the register button");
				PageShouldContainText("Signup complete");
			}
		}

		[TestMethod]
		public void Register_And_Login_Without_Activating_Should_Fail()
		{
			using (NewSession())
			{
				Logout();
				GoToUrl("/user/signup/");

				string email = "watin@localhost" + Guid.NewGuid();
				string username = "watin" + Guid.NewGuid();

				SetTextfieldValue("Firstname", "Watin");
				SetTextfieldValue("Lastname", "Tester");
				SetTextfieldValue("NewEmail", email);
				SetTextfieldValue("NewUsername", username);
				SetTextfieldValue("Password", "password");
				SetTextfieldValue("PasswordConfirmation", "password");

				ClickButton("userbutton", "Failed to click the register button");
				PageShouldContainText("Signup complete");

				// Login
				GoToUrl("/user/login");
				SetTextfieldValue("email", email);
				SetTextfieldValue("password", "password");
				ClickButton("userbutton", "Failed to click the login button");
				PageShouldContainText("Login was unsucessful");
			}
		}

		[TestMethod]
		public void Register_With_Mismatching_Passwords()
		{
			using (NewSession())
			{
				Logout();
				GoToUrl("/user/signup");

				SetTextfieldValue("Firstname", "Watin");
				SetTextfieldValue("Lastname", "Tester");
				SetTextfieldValue("NewEmail", "watin@localhost" + Guid.NewGuid());
				SetTextfieldValue("NewUsername", "Watinuser" + Guid.NewGuid());
				SetTextfieldValue("Password", "password");
				SetTextfieldValue("PasswordConfirmation", "password2"); // mismatch

				ClickButton("userbutton", "Failed to click the register button");
				PageShouldContainText("The passwords don't match");
			}
		}

		[TestMethod]
		public void Register_With_Existing_Email()
		{
			using (NewSession())
			{
				Logout();
				GoToUrl("/user/signup");

				string email = "watin@localhost" + Guid.NewGuid();

				// Add the initial email
				SetTextfieldValue("Firstname", "Watin");
				SetTextfieldValue("Lastname", "Tester");
				SetTextfieldValue("NewEmail", email);
				SetTextfieldValue("NewUsername", "Watinuser");
				SetTextfieldValue("Password", "password");
				SetTextfieldValue("PasswordConfirmation", "password");

				ClickButton("userbutton", "Failed to click the register button");
				PageShouldContainText("Signup complete");

				// Try to add again
				GoToUrl("/user/signup");
				SetTextfieldValue("Firstname", "Watin");
				SetTextfieldValue("Lastname", "Tester");
				SetTextfieldValue("NewEmail", email);
				SetTextfieldValue("NewUsername", "Watinuser");
				SetTextfieldValue("Password", "password");
				SetTextfieldValue("PasswordConfirmation", "password");

				ClickButton("userbutton", "Failed to click the register button");
				PageShouldContainText("An error occurred");
			}
		}

		[TestMethod]
		public void ResetPassword_With_Valid_Email()
		{
			using (NewSession())
			{
				GoToUrl("/user/resetpassword");

				SetTextfieldValue("email", "admin@localhost");

				ClickButton("userbutton", "Failed to click the reset password button");
				PageShouldContainText("Your password reset request was sent");
			}
		}

		[TestMethod]
		public void ResetPassword_With_Non_Existent_Email()
		{
			using (NewSession())
			{
				GoToUrl("/user/resetpassword");

				SetTextfieldValue("email", "nobody@localhost");

				ClickButton("userbutton", "Failed to click the reset password button");
				PageShouldContainText("The email address could not be found");
			}
		}

		[TestMethod]
		public void Update_Userprofile_With_Valid_Data()
		{
			using (NewSession())
			{
				LoginAsEditor();
				GoToUrl("/user/profile");

				SetTextfieldValue("Firstname", "Watin");
				SetTextfieldValue("Lastname", "Tester");
				SetTextfieldValue("NewEmail", "watin@localhost" + Guid.NewGuid());
				SetTextfieldValue("NewUsername", "Watinuser" + Guid.NewGuid());
				SetTextfieldValue("Password", "password");
				SetTextfieldValue("PasswordConfirmation", "password");

				ClickButton("userbutton", "Failed to click the save profile button");
				PageShouldNotContainText("An error occured");
			}
		}
	}
}
