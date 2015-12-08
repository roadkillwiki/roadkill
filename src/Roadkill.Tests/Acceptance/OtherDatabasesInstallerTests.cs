using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class OtherDatabasesInstallerTests : InstallerTests
	{
		[Test]
		[Explicit("Requires MySQL 5 installed on the machine the acceptance tests are running first.")]
		public void MySQL_All_Steps_With_Minimum_Required()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
			select.SelectByValue("MySQL");

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"server=localhost;database=roadkill;uid=root;pwd=Passw0rd;");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector(".alert strong")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector(".continue a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		[Explicit("Requires Postgres 9 server installed on the machine the acceptance tests are running first.")]
		public void Postgres_All_Steps_With_Minimum_Required()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
			select.SelectByValue("Postgres");

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"server=localhost;database=roadkill;uid=postgres;pwd=Passw0rd;");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector(".alert strong")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector(".continue a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}
		
		[Test]
		[Explicit("Requires SQL Server Express 2012 (but it uses the Lightspeed SQL Server 2005 driver) installed on the machine the acceptance tests are running first, using LocalDB.")]
		public void SQLServer2005Driver_All_Steps_With_Minimum_Required()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
			select.SelectByValue("SqlServer2008");

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"Server=(LocalDB)\v11.0;Integrated Security=true;");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector(".alert strong")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector(".continue a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		[Explicit(@"This is really a helper test, it installs onto .\SQLEXPRESS, database 'roadkill' using integrated security")]
		public void SQLServerExpress_All_Steps_With_Minimum_Required()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("button[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#bottom-buttons > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DatabaseName")));
			select.SelectByValue("SqlServer2008");

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"Server=.\SQLEXPRESS;Integrated Security=true;database=roadkill");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue button")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector(".alert strong")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector(".continue a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}
	}
}
