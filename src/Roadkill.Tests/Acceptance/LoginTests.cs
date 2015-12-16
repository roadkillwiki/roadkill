using NUnit.Framework;
using OpenQA.Selenium;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class LoginTests : AcceptanceTestBase
	{
		[Test]
		public void can_login_as_admin()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);

			// Act
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			
			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas a")).Text,Is.EqualTo("Logged in as admin"));
		}

		[Test]
		public void can_login_as_editor()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(EDITOR_PASSWORD);

			// Act
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas a")).Text, Is.EqualTo("Logged in as editor"));
		}

		[Test]
		public void incorrect_email_for_login_shows_error()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys("badlogin@roadkillwiki.org");
			Driver.FindElement(By.Name("password")).SendKeys("editor");

			// Act
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".validation-summary-errors span")).Text, Is.EqualTo("Login was unsuccessful"));
		}

		[Test]
		public void incorrect_password_for_admin_showserror()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys("badpassword");

			// Act
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".validation-summary-errors span")).Text, Is.EqualTo("Login was unsuccessful"));
		}

		[Test]
		public void can_logout_as_admin()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// Act
			Driver.FindElement(By.CssSelector("#loggedinas a[href='/user/logout']")).Click(); // do it by url to be safe

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas a")).Text, Is.EqualTo("Login"));			
			string actual = Driver.FindElement(By.CssSelector("#loggedinas")).Text;
			Assert.That(actual, Is.StringContaining("Not logged in"));
		}
	}
}
