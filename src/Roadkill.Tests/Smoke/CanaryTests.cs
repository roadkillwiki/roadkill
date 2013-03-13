using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using Roadkill.Tests.Acceptance;

namespace Roadkill.Tests.Acceptance.Smoke
{
	/// <summary>
	/// Acceptance tests to ensure the site works (no yellow screen of death), 
	/// before running main batch of acceptance tests.
	/// </summary>
	[TestFixture]
	[Category("Smoke Tests")]
	public class CanaryTests : AcceptanceTestBase
	{
		[Test]
		public void Can_Reach_Homepage()
		{
			// Arrange
			

			// Act
			Driver.Navigate().GoToUrl(BaseUrl);

			// Assert
			Assert.That(Driver.FindElements(By.Id("container")).Count, Is.GreaterThan(0), "FAILED: \n"+Driver.PageSource);
		}

		[Test]
		public void Can_Login_As_Admin()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			IWebElement emailBox = Driver.FindElements(By.Name("email")).FirstOrDefault();
			IWebElement passwordBox = Driver.FindElements(By.Name("password")).FirstOrDefault();

			// Act
			if (emailBox != null && passwordBox != null)
			{
				emailBox.SendKeys(ADMIN_EMAIL);
				passwordBox.SendKeys(ADMIN_PASSWORD);
				Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			}

			// Assert
			IWebElement loggedInElement = Driver.FindElements(By.CssSelector("#loggedinas a")).FirstOrDefault();
			Assert.IsNotNull(loggedInElement, "FAILED: \n" + Driver.PageSource);
			Assert.That(loggedInElement.Text, Is.EqualTo("Logged in as admin"), "FAILED: \n" + Driver.PageSource);
		}
	}
}
