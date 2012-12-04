using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core;
using Roadkill.Tests.Core;
using NUnit.Framework;
using System.IO;

using Plasma.Core;
using Plasma.WebDriver;
using OpenQA.Selenium;
using System.Diagnostics;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using SimpleBrowser.WebDriver;
using System.Threading;

namespace Roadkill.Tests.Acceptance
{
	/// <summary>
	/// Web tests using a headless browser (non-javascript interaction)
	/// </summary>
	[TestFixture]
	[Category("Acceptance")]
	[Explicit]
	public class LoginTests : AcceptanceTestBase
	{
		[Test]
		public void Can_Login_As_Admin()
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
		public void Can_Login_As_Editor()
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
		public void Incorrect_Email_For_Login_Shows_Error()
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
		public void Incorrect_Password_For_Admin_ShowsError()
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
		public void Can_Logout_As_Admin()
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
