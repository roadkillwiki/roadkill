using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;

namespace Roadkill.Tests.Acceptance
{
	/// <summary>
	/// Homepage web tests using a headless browser (non-javascript interaction)
	/// </summary>
	[TestFixture]
	[Category("Acceptance")]
	[Explicit]
	public class DemoSiteTests
	{
		private IWebDriver _driver;
		private string _baseUrl;

		[SetUp]
		public void Setup()
		{
			_baseUrl = "http://www.roadkillwiki.net";
			_driver = new PhantomJSDriver();
			_driver.Navigate().GoToUrl(_baseUrl);
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
