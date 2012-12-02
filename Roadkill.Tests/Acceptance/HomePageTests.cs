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

namespace Roadkill.Tests.Acceptance.Headless
{
	/// <summary>
	/// Homepage web tests using a headless browser (non-javascript interaction)
	/// </summary>
	[TestFixture]
	[Category("Acceptance")]
	[Explicit]
	public class HomePageTests : AcceptanceTestsBase
	{
		[Test]
		[Explicit]
		public void DemoSite_ShouldHave_HeaderTitle()
		{	
			// For the demonstration site only
			IWebElement h1Element = Driver.FindElement(By.CssSelector("h1"));
			Assert.That(h1Element.Text, Is.EqualTo("Homepage"));
		}

		[Test]
		[Explicit]
		public void DemoSite_ShouldHave_Title()
		{
			// For the demonstration site only
			IWebElement title = Driver.FindElement(By.CssSelector("title"));
			Assert.That(title.Text, Is.EqualTo("Homepage"));
		}

		[Test]
		public void ShouldHave_LeftMenu()
		{
			IEnumerable<IWebElement> leftmenu = Driver.FindElements(By.CssSelector("div#leftmenu li"));
			Assert.That(leftmenu.Count(), Is.EqualTo(3));
		}

		[Test]
		public void ShouldHave_HistoryLink()
		{
			IWebElement title = Driver.FindElement(By.CssSelector("div#viewhistory a"));
			Assert.That(title.Text, Is.EqualTo("View History"));
		}

		[Test]
		public void ShouldHave_LoginLink()
		{
			IWebElement title = Driver.FindElement(By.CssSelector("span#loggedinas a"));
			Assert.That(title.Text, Is.EqualTo("Login"));
			Assert.That(title.GetAttribute("href"), Is.EqualTo("/user/login"));
		}
	}
}
