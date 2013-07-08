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
	[Category("Smoke Tests")]
	public class DemoSiteTests : AcceptanceTestBase
	{
		[SetUp]
		public void Setup()
		{
			BaseUrl = "http://www.roadkillwiki.net";
			Driver.Navigate().GoToUrl(BaseUrl);
		}

		[Test]
		public void ShouldHave_HeaderTitle()
		{
			IWebElement h1Element = Driver.FindElement(By.CssSelector("h1"));
			Assert.That(h1Element.Text, Is.EqualTo("Welcome to the Roadkill .NET Wiki demo site"));
		}

		[Test]
		public void ShouldHave_Title()
		{
			IWebElement title = Driver.FindElement(By.CssSelector("title"));
			Assert.That(title.Text, Is.EqualTo("Welcome to the Roadkill .NET Wiki demo site"));
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
			Assert.That(title.GetAttribute("href"), Is.StringEnding("/user/login?returnurl=%2f"));
		}
	}
}
