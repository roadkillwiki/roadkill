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
	public class LoginTests : AcceptanceTestsBase
	{		
		[Test]
		public void CanLogin_AsAdmin()
		{
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys("admin@localhost");
			Driver.FindElement(By.Name("password")).SendKeys("password");
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			
			Thread.Sleep(2000);
			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas a")).Text,Is.EqualTo("Logged in as admin"));
		}

		[Test]
		public void BadEmail_AsAdmin_ShowsError()
		{
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys("badlogin@roadkillwiki.org");
			Driver.FindElement(By.Name("password")).SendKeys("editor");
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			Thread.Sleep(2000);
			Assert.That(Driver.FindElement(By.CssSelector(".validation-summary-errors span")).Text, Is.EqualTo("Login was unsuccessful"));
		}

		[Test]
		public void BadPassword_AsAdmin_ShowsError()
		{
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys("admin@localhost");
			Driver.FindElement(By.Name("password")).SendKeys("badpassword");
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			Thread.Sleep(2000);
			Assert.That(Driver.FindElement(By.CssSelector(".validation-summary-errors span")).Text, Is.EqualTo("Login was unsuccessful"));
		}

		[Test]
		public void CanLogout_AsAdmin()
		{
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys("admin@localhost");
			Driver.FindElement(By.Name("password")).SendKeys("password");
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			Thread.Sleep(2000);
			Driver.FindElement(By.CssSelector("#loggedinas a[href=/user/logout]")).Click(); // do it by url to be safe
			Thread.Sleep(2000);

			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas a")).Text, Is.EqualTo("Login"));			
			string actual = Driver.FindElement(By.CssSelector("#loggedinas")).Text;
			Assert.That(actual, Is.StringContaining("Not logged in"));
		}
	}
}
