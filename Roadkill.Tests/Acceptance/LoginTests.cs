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
	public class LoginTests
	{
		private SimpleBrowserDriver _driver;
		private readonly string LOGINURL = Settings.HeadlessUrl + "/user/login";

		[TestFixtureSetUp]
		public void Setup()
		{
			_driver = new SimpleBrowserDriver();
			_driver.Navigate().GoToUrl(Settings.HeadlessUrl);
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			_driver.Dispose();
		}

		
		[Test]
		public void CanLogin_AsEditor()
		{
			_driver.Navigate().GoToUrl(LOGINURL);
			_driver.FindElement(By.Name("email")).SendKeys("nobody@roadkillwiki.org");
			_driver.FindElement(By.Name("password")).SendKeys("editor");
			_driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			
			Thread.Sleep(2000);
			Assert.That(_driver.FindElement(By.CssSelector("#loggedinas a")).Text,Is.EqualTo("Logged in as editor"));
		}

		[Test]
		public void BadEmail_AsEditor_ShowsError()
		{
			_driver.Navigate().GoToUrl(LOGINURL);
			_driver.FindElement(By.Name("email")).SendKeys("badlogin@roadkillwiki.org");
			_driver.FindElement(By.Name("password")).SendKeys("editor");
			_driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			Thread.Sleep(2000);
			Assert.That(_driver.FindElement(By.CssSelector(".validation-summary-errors span")).Text, Is.EqualTo("Login was unsuccessful"));
		}

		[Test]
		public void BadPassword_AsEditor_ShowsError()
		{
			_driver.Navigate().GoToUrl(LOGINURL);
			_driver.FindElement(By.Name("email")).SendKeys("nobody@roadkillwiki.org");
			_driver.FindElement(By.Name("password")).SendKeys("badpassword");
			_driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			Thread.Sleep(2000);
			Assert.That(_driver.FindElement(By.CssSelector(".validation-summary-errors span")).Text, Is.EqualTo("Login was unsuccessful"));
		}

		[Test]
		public void CanLogout_AsEditor()
		{
			_driver.Navigate().GoToUrl(LOGINURL);
			_driver.FindElement(By.Name("email")).SendKeys("nobody@roadkillwiki.org");
			_driver.FindElement(By.Name("password")).SendKeys("editor");
			_driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			Thread.Sleep(2000);
			_driver.FindElement(By.CssSelector("#loggedinas a[href=/user/logout]")).Click(); // do it by url to be safe
			Thread.Sleep(2000);

			Assert.That(_driver.FindElement(By.CssSelector("#loggedinas a")).Text, Is.EqualTo("Login"));			
			string actual = _driver.FindElement(By.CssSelector("#loggedinas")).Text;
			Assert.That(actual, Is.StringContaining("Not logged in"));
		}
	}
}
