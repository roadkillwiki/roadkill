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
using OpenQA.Selenium.Firefox;

namespace Roadkill.Tests.Acceptance
{
	/// <summary>
	/// Web tests using a headless browser (non-javascript interaction)
	/// </summary>
	[TestFixture]
	[Category("Acceptance")]
	[Explicit]
	public class PageTests : AcceptanceTestsBase
	{
		[Test]
		public void No_MainPage_Set()
		{
			// Arrange + Act
			Driver.Navigate().GoToUrl(BaseUrl);
			
			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Is.EqualTo("You have no mainpage set"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Is.EqualTo("To set a main page, create a page and assign the tag 'homepage' to it."));
		}

		[Test]
		public void Anonymous_Has_Limited_Menu_Options()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);

			// Act
			IEnumerable<IWebElement> leftmenuItems = Driver.FindElements(By.CssSelector("div#leftmenu li"));

			// Assert
			Assert.That(leftmenuItems.Count(), Is.EqualTo(3));
		}

		[Test]
		public void Login_As_Admin_Shows_All_Left_Menu_Options()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// Act
			IEnumerable<IWebElement> leftmenuItems = Driver.FindElements(By.CssSelector("div#leftmenu li"));

			// Assert
			Assert.That(leftmenuItems.Count(), Is.EqualTo(5));
		}

		[Test]
		public void Login_As_Editor_Shows_Extra_Menu_Option()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(EDITOR_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// Act
			IEnumerable<IWebElement> leftmenuItems = Driver.FindElements(By.CssSelector("div#leftmenu li"));

			// Assert
			Assert.That(leftmenuItems.Count(), Is.EqualTo(4));
		}

		[Test]
		public void NewPage_With_Homepage_Tag_Shows_As_Homepage()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/new']")).Click();
			Driver.FindElement(By.Name("Title")).SendKeys("My title");
			Driver.FindElement(By.Name("RawTags")).SendKeys("Homepage");
			Driver.FindElement(By.Name("Content")).SendKeys("Some content goes here");
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();

			// Assert
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("My title"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		public void AllTagsPage_Displays_Correct_Tags()
		{

		}

		[Test]
		public void EditIcon_Exists_For_Editors()
		{

		}

		[Test]
		public void EditIcon_Exists_For_Admins()
		{

		}

		[Test]
		public void Properties_Icon_Exists_For_Editors()
		{

		}

		[Test]
		public void Properties_Icon_Exists_For_Admin()
		{

		}

		[Test]
		public void View_History_Link_Exists()
		{

		}
	}
}
