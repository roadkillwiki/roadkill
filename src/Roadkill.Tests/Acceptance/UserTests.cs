using System.Linq;
using NUnit.Framework;
using System.IO;

using OpenQA.Selenium;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class UserTests : AcceptanceTestBase
	{
		[Test]
		public void Reset_Password_Sends_Email()
		{
			// Arrange
			string pickupPath = Path.Combine(Settings.WEB_PATH, "App_Data", "TempSmtp");
			if (!Directory.Exists(pickupPath))
				Directory.CreateDirectory(pickupPath);

			foreach (string file in Directory.GetFiles(pickupPath, "*.eml"))
			{
				File.Delete(file);
			}
			
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);

			// Act
			Driver.FindElement(By.CssSelector("a[href='/user/resetpassword']")).Click();
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.CssSelector("input[value='Reset password']")).Click();
			
			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#content h1"))[0].Text, Contains.Substring("Your password reset request was sent."), Driver.PageSource);
			Assert.That(Driver.FindElement(By.CssSelector("#content p")).Text, Contains.Substring("Thank you, an email has been sent to admin@localhost with details on how to reset your password."));
			Assert.That(Directory.GetFiles(pickupPath, "*.eml").Count(), Is.EqualTo(1));
		}

		[Test]
		public void View_Profile_Shows_Correct_Information()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/user/profile']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#Firstname")).GetAttribute("value"), Is.EqualTo("Chris"));
			Assert.That(Driver.FindElement(By.CssSelector("#Lastname")).GetAttribute("value"), Is.EqualTo("Admin"));
			Assert.That(Driver.FindElement(By.CssSelector("#NewEmail")).GetAttribute("value"), Is.EqualTo("admin@localhost"));
			Assert.That(Driver.FindElement(By.CssSelector("#NewUsername")).GetAttribute("value"), Is.EqualTo("admin"));
			Assert.That(Driver.FindElement(By.CssSelector("#Password")).GetAttribute("value"), Is.EqualTo(""));
			Assert.That(Driver.FindElement(By.CssSelector("#PasswordConfirmation")).GetAttribute("value"), Is.EqualTo(""));
		}

		[Test]
		public void Save_Profile_Saves_And_Shows_Correct_Information()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			Driver.FindElement(By.CssSelector("a[href='/user/profile']")).Click();	

			// Act
			Driver.FindElement(By.CssSelector("#Firstname")).Clear();
			Driver.FindElement(By.CssSelector("#Firstname")).SendKeys("NewFirstName");
			Driver.FindElement(By.CssSelector("#Lastname")).Clear();
			Driver.FindElement(By.CssSelector("#Lastname")).SendKeys("NewLastName");
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();
			Driver.FindElement(By.CssSelector("a[href='/user/profile']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#Firstname")).GetAttribute("value"), Is.EqualTo("NewFirstName"));
			Assert.That(Driver.FindElement(By.CssSelector("#Lastname")).GetAttribute("value"), Is.EqualTo("NewLastName"));
			Assert.That(Driver.FindElement(By.CssSelector("#NewEmail")).GetAttribute("value"), Is.EqualTo("admin@localhost"));
			Assert.That(Driver.FindElement(By.CssSelector("#NewUsername")).GetAttribute("value"), Is.EqualTo("admin"));
			Assert.That(Driver.FindElement(By.CssSelector("#Password")).GetAttribute("value"), Is.EqualTo(""));
			Assert.That(Driver.FindElement(By.CssSelector("#PasswordConfirmation")).GetAttribute("value"), Is.EqualTo(""));
		}

		[Test]
		public void Save_Profile_Password_Changes_Password()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(EDITOR_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			Driver.FindElement(By.CssSelector("a[href='/user/profile']")).Click();	

			// Act
			Driver.FindElement(By.CssSelector("#Password")).Clear();
			Driver.FindElement(By.CssSelector("#Password")).SendKeys("newpassword");
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).Clear();
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).SendKeys("newpassword");
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();
			Driver.Navigate().GoToUrl(LogoutUrl);

			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys("newpassword");
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			
			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas a")).Text, Is.EqualTo("Logged in as editor"));
		}

		[Test]
		public void Register_Shows_Confirmation_Page_And_Sends_Email()
		{
			// Arrange
			string pickupPath = Path.Combine(Settings.WEB_PATH, "App_Data", "TempSmtp");
			if (!Directory.Exists(pickupPath))
				Directory.CreateDirectory(pickupPath);

			foreach (string file in Directory.GetFiles(pickupPath, "*.eml"))
			{
				File.Delete(file);
			}

			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.FindElement(By.CssSelector("a[href='/user/signup']")).Click();
			Driver.FindElement(By.CssSelector("#Firstname")).Clear();
			Driver.FindElement(By.CssSelector("#Firstname")).SendKeys("My Firstname");
			Driver.FindElement(By.CssSelector("#Lastname")).Clear();
			Driver.FindElement(By.CssSelector("#Lastname")).SendKeys("My Lastname");
			Driver.FindElement(By.CssSelector("#NewEmail")).Clear();
			Driver.FindElement(By.CssSelector("#NewEmail")).SendKeys("newemail@localhost");
			Driver.FindElement(By.CssSelector("#NewUsername")).Clear();
			Driver.FindElement(By.CssSelector("#NewUsername")).SendKeys("myusername");
			Driver.FindElement(By.CssSelector("#Password")).Clear();
			Driver.FindElement(By.CssSelector("#Password")).SendKeys("password");
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).Clear();
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).SendKeys("password");

			// Act
			Driver.FindElement(By.CssSelector("input[value='Register new user']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#content h1")).Text, Is.EqualTo("Signup complete."));
			Assert.That(Driver.FindElement(By.CssSelector("#content p")).Text, Is.EqualTo("Thank you, an email has been sent to newemail@localhost with details on how to complete the signup."));
			Assert.That(Driver.FindElements(By.CssSelector("input[value='Resend email confirmation']")).Count, Is.EqualTo(1));
			Assert.That(Directory.GetFiles(pickupPath, "*.eml").Count(), Is.EqualTo(1));
		}

		[Test]
		public void Register_With_Missing_Email_Shows_Validation_Errors()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.FindElement(By.CssSelector("a[href='/user/signup']")).Click();
			Driver.FindElement(By.CssSelector("#Firstname")).Clear();
			Driver.FindElement(By.CssSelector("#Firstname")).SendKeys("My Firstname");
			Driver.FindElement(By.CssSelector("#Lastname")).Clear();
			Driver.FindElement(By.CssSelector("#Lastname")).SendKeys("My Lastname");
			
			Driver.FindElement(By.CssSelector("#NewEmail")).Clear();
			// Missing
			
			Driver.FindElement(By.CssSelector("#NewUsername")).Clear();
			Driver.FindElement(By.CssSelector("#NewUsername")).SendKeys("myusername");
			Driver.FindElement(By.CssSelector("#Password")).Clear();
			Driver.FindElement(By.CssSelector("#Password")).SendKeys("password");
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).Clear();
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).SendKeys("password");

			// Act
			Driver.FindElement(By.CssSelector("input[value='Register new user']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".validation-summary-errors li")).Text, Is.EqualTo("The email field is required."));
		}

		[Test]
		public void Register_With_Missing_Password_Shows_Validation_Errors()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.FindElement(By.CssSelector("a[href='/user/signup']")).Click();
			Driver.FindElement(By.CssSelector("#Firstname")).Clear();
			Driver.FindElement(By.CssSelector("#Firstname")).SendKeys("My Firstname");
			Driver.FindElement(By.CssSelector("#Lastname")).Clear();
			Driver.FindElement(By.CssSelector("#Lastname")).SendKeys("My Lastname");
			Driver.FindElement(By.CssSelector("#NewEmail")).Clear();
			Driver.FindElement(By.CssSelector("#NewEmail")).SendKeys("newemail@localhost");
			Driver.FindElement(By.CssSelector("#NewUsername")).Clear();
			Driver.FindElement(By.CssSelector("#NewUsername")).SendKeys("myusername");
			Driver.FindElement(By.CssSelector("#Password")).Clear();
			Driver.FindElement(By.CssSelector("#Password")).SendKeys("");
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).Clear();
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).SendKeys("");

			// Act
			Driver.FindElement(By.CssSelector("input[value='Register new user']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".validation-summary-errors li")).Text, Is.EqualTo("The password is less than 6 characters"));
		}

		[Test]
		public void Register_And_Resend_Email_Confirmation_Sends_Email()
		{
			// Arrange
			string pickupPath = Path.Combine(Settings.WEB_PATH, "App_Data", "TempSmtp");
			if (!Directory.Exists(pickupPath))
				Directory.CreateDirectory(pickupPath);

			foreach (string file in Directory.GetFiles(pickupPath, "*.eml"))
			{
				File.Delete(file);
			}

			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.FindElement(By.CssSelector("a[href='/user/signup']")).Click();
			Driver.FindElement(By.CssSelector("#Firstname")).Clear();
			Driver.FindElement(By.CssSelector("#Firstname")).SendKeys("My Firstname");
			Driver.FindElement(By.CssSelector("#Lastname")).Clear();
			Driver.FindElement(By.CssSelector("#Lastname")).SendKeys("My Lastname");
			Driver.FindElement(By.CssSelector("#NewEmail")).Clear();
			Driver.FindElement(By.CssSelector("#NewEmail")).SendKeys("newemail@localhost");
			Driver.FindElement(By.CssSelector("#NewUsername")).Clear();
			Driver.FindElement(By.CssSelector("#NewUsername")).SendKeys("myusername");
			Driver.FindElement(By.CssSelector("#Password")).Clear();
			Driver.FindElement(By.CssSelector("#Password")).SendKeys("password");
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).Clear();
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).SendKeys("password");
			Driver.FindElement(By.CssSelector("input[value='Register new user']")).Click();

			// Act
			Driver.FindElement(By.CssSelector("input[value='Resend email confirmation']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".alert")).Text, Is.EqualTo("The confirmation email was resent."));
			Assert.That(Directory.GetFiles(pickupPath, "*.eml").Count(), Is.EqualTo(2));
		}
	}
}
