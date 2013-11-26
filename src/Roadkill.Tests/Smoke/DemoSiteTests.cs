using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;

namespace Roadkill.Tests.Acceptance.Smoke
{
	/// <summary>
	/// Demo site web tests.
	/// </summary>
	[TestFixture]
	[Category("Smoke")]
	public class DemoSiteTests : AcceptanceTestBase
	{
		[SetUp]
		public void Setup()
		{
			BaseUrl = "http://demo.roadkillwiki.net";
			Driver.Navigate().GoToUrl(BaseUrl);
		}

		[Test]
		public void Should_Have_Page_Title()
		{
			IWebElement title = Driver.FindElement(By.CssSelector(".pagetitle"));
			Assert.That(title.Text, Is.EqualTo("Welcome to the Roadkill .NET Wiki demo site"));
		}

		[Test]
		public void Should_Have_LeftMenu()
		{
			IEnumerable<IWebElement> leftmenu = Driver.FindElements(By.CssSelector("ul.nav li"));
			Assert.That(leftmenu.Count(), Is.EqualTo(3));
		}

		[Test]
		public void Should_Have_HistoryLink()
		{
			IWebElement title = Driver.FindElement(By.CssSelector("div#viewhistory a"));
			Assert.That(title.Text, Is.EqualTo("View History"));
		}

		[Test]
		public void Should_Have_LoginLink()
		{
			IWebElement title = Driver.FindElement(By.CssSelector("span#loggedinas a"));
			Assert.That(title.Text, Is.EqualTo("Login"));
			Assert.That(title.GetAttribute("href"), Is.StringEnding("/user/login?returnurl=%2f"));
		}

		[Test]
		public void Should_Have_Search_Box()
		{
			IWebElement input = Driver.FindElement(By.CssSelector("form input[name=q]"));
			Assert.That(input, Is.Not.Null);
		}
	}
}
