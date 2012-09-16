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

namespace Roadkill.Tests.Headless
{
	/// <summary>
	/// Web tests using a headless browser (non-javascript interaction)
	/// </summary>
	[TestFixture]
	public class SearchTests
	{
		private SimpleBrowserDriver _driver;

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
		public void ShouldHave_HeaderTitle()
		{	
			IWebElement h1Element = _driver.FindElement(By.CssSelector("h1"));
			Assert.That(h1Element.Text, Is.EqualTo("Homepage"));
		}

		[Test]
		public void ShouldHave_Title()
		{
			IWebElement title = _driver.FindElement(By.CssSelector("title"));
			Assert.That(title.Text, Is.EqualTo("Homepage"));
		}

		[Test]
		public void ShouldHave_LeftMenu()
		{
			IEnumerable<IWebElement> leftmenu = _driver.FindElements(By.CssSelector("div#leftmenu li"));
			Assert.That(leftmenu.Count(), Is.EqualTo(3));
		}

		[Test]
		public void ShouldHave_HistoryLink()
		{
			IWebElement title = _driver.FindElement(By.CssSelector("div#viewhistory a"));
			Assert.That(title.Text, Is.EqualTo("View History"));
		}

		[Test]
		public void ShouldHave_LoginLink()
		{
			IWebElement title = _driver.FindElement(By.CssSelector("span#loggedinas a"));
			Assert.That(title.Text, Is.EqualTo("Login"));
			Assert.That(title.GetAttribute("href"), Is.EqualTo("/user/login"));
		}
	}
}
