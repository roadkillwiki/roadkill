using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class SearchTests : AcceptanceTestBase
	{
		[SetUp]
		public void BeforeEachTest()
		{
			// Recreate the lucene index as it will be out of sync with the db
			foreach (string file in Directory.GetFiles(Path.Combine(Settings.WEB_PATH, "App_Data", "Internal", "search")))
			{
				File.Delete(file);
			}
		
			LoginAsAdmin();
			Driver.Navigate().GoToUrl(BaseUrl + "/settings/updatesearchindex");
			Logout();
		}

		[Test]
		public void Search_From_Global_Search_Bar_Returns_Results()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage");
			CreatePageWithTitleAndTags("Another page 1", "Another");
			CreatePageWithTitleAndTags("Another page 2", "Another");
			Driver.Navigate().GoToUrl(BaseUrl + "/SiteSettings/tools/updatesearchindex");
			Logout();

			// Act
			Driver.FindElement(By.CssSelector("#search input[name='q']")).Clear();
			Driver.FindElement(By.CssSelector("#search input[name='q']")).SendKeys("Another page");
			Driver.FindElement(By.CssSelector("#search input[name='q']")).SendKeys(Keys.Return);
			
			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".searchresult-title a")).Count, Is.EqualTo(2));
			Assert.That(Driver.FindElements(By.CssSelector(".searchresult-title a"))[0].Text, Is.EqualTo("Another page 1"));
			Assert.That(Driver.FindElements(By.CssSelector(".searchresult-title a"))[1].Text, Is.EqualTo("Another page 2"));
		}

		[Test]
		public void Search_From_Search_Page_Returns_Results()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage");
			CreatePageWithTitleAndTags("Another page 1", "Another");
			CreatePageWithTitleAndTags("Another page 2", "Another");
			Driver.Navigate().GoToUrl(BaseUrl + "/SiteSettings/tools/updatesearchindex");
			Logout();

			// Act
			Driver.FindElement(By.CssSelector("#search input[name='q']")).SendKeys(Keys.Return);

			Driver.FindElement(By.CssSelector("#content input[name='q']")).Clear();
			Driver.FindElement(By.CssSelector("#content input[name='q']")).SendKeys("Another page");
			Driver.FindElement(By.CssSelector("input[value='Search']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".searchresult-title a")).Count, Is.EqualTo(2));
			Assert.That(Driver.FindElements(By.CssSelector(".searchresult-title a"))[0].Text, Is.EqualTo("Another page 1"));
			Assert.That(Driver.FindElements(By.CssSelector(".searchresult-title a"))[1].Text, Is.EqualTo("Another page 2"));
		}

		[Test]
		public void Search_With_No_Results_Shows_Message()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Page 1", "Another");
			CreatePageWithTitleAndTags("Page 2", "Another");
			Driver.Navigate().GoToUrl(BaseUrl + "/SiteSettings/tools/updatesearchindex");
			Logout();

			// Act
			Driver.FindElement(By.CssSelector("#search input[name='q']")).SendKeys(Keys.Return);

			Driver.FindElement(By.CssSelector("#content input[name='q']")).Clear();
			Driver.FindElement(By.CssSelector("#content input[name='q']")).SendKeys("test");
			Driver.FindElement(By.CssSelector("input[value='Search']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".searchresult")).Count, Is.EqualTo(0));
			Assert.That(Driver.FindElement(By.CssSelector("#content h1")).Text, Is.EqualTo("Your search 'test' did not match any pages"));
		}
	}
}
