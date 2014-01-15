using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using Roadkill.Core.Database;
using Roadkill.Tests.Acceptance;

namespace Roadkill.Tests.Acceptance.Smoke
{
	/// <summary>
	/// Acceptance tests to ensure the site works (no yellow screen of death), 
	/// before running main batch of acceptance tests.
	/// </summary>
	[TestFixture]
	[Category("Smoke")]
	public class CanaryTests : AcceptanceTestBase
	{
		[Test]
		public void Can_Reach_Homepage()
		{
			// Arrange + Act
			Driver.Navigate().GoToUrl(BaseUrl);

			// Assert
			Assert.That(Driver.FindElements(By.Id("container")).Count, Is.GreaterThan(0), "FAILED: \n"+Driver.PageSource);
		}

		[Test]
		public void WebApi_Help_Page_Has_Known_Text()
		{
			// Arrange
			string expectedText = "Roadkill REST API Help";

			// Act
			Driver.Navigate().GoToUrl(BaseUrl + "/api");

			string actualText = Driver.FindElement(By.CssSelector(".content-wrapper h1")).Text;

			// Assert
			Assert.That(actualText, Is.EqualTo(expectedText), actualText);
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

		[Test]
		[Description("Used to verify the release zip file before Creating a new Codeplex release")]
		[Explicit]
		public void Can_Install_SqlServer_Ce_From_Release_Zip_File()
		{
			// Arrange
			string installUrl = "http://roadkill.prerelease.local/";
			LoginUrl = string.Format("{0}/user/login", installUrl);
			Driver.Navigate().GoToUrl(installUrl);
			
			// Act
			// step 1
			Driver.FindElement(By.CssSelector("input[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector(".continue > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DataStoreTypeName")));
			select.SelectByValue(DataStoreType.SqlServerCe.Name);

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"Data Source=|DataDirectory|\roadkill.sdf");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector("div#installsuccess h1")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector("div#installsuccess a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			// Assert
			Driver.Navigate().GoToUrl(installUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		[Description("Used to verify the upgrade in the release zip file before Creating a new Codeplex release")]
		[Explicit]
		public void Can_Upgrade_SqlServerCe_From_152_From_Release_Zip_File()
		{
			// Arrange
			string upgradeUrl = "http://roadkillupgrade.local/";
			LoginUrl = string.Format("{0}/user/login", upgradeUrl);
			Driver.Navigate().GoToUrl(upgradeUrl);

			// Act
			Driver.FindElement(By.CssSelector("input[value=Upgrade]")).Click();
			LoginAsAdmin();
			CreatePageWithTitleAndTags("New page", "new page");
			Driver.Navigate().GoToUrl(upgradeUrl);

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("This is 1.5.2 homepage"));

			Driver.Navigate().GoToUrl(upgradeUrl + "wiki/2/new-page");
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("New page"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}
	}
}
