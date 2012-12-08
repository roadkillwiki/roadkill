using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Roadkill.Tests.Acceptance
{
	public abstract class AcceptanceTestBase
	{
		protected static readonly string ADMIN_EMAIL = "admin@localhost";
		protected static readonly string ADMIN_PASSWORD = "password";

		protected static readonly string EDITOR_EMAIL = "editor@localhost";
		protected static readonly string EDITOR_PASSWORD = "password";
		protected string SitePath;

		protected IWebDriver Driver;
		protected string LoginUrl;
		protected string BaseUrl;
		protected string LogoutUrl;

		[SetUp]
		public void BeforeEachTextFixture()
		{
			CopyDb();
			BaseUrl = "http://localhost:9876";
			LoginUrl = BaseUrl + "/user/login";
			LogoutUrl = BaseUrl + "/user/logout";
			Driver = AcceptanceTestsSetup.Driver;
		}

		private void CopyDb()
		{
			SitePath = AcceptanceTestsSetup.GetSitePath();
			string libFolder = Path.Combine(SitePath, "..", "lib");
			libFolder = new DirectoryInfo(libFolder).FullName;

			string testsDBPath = Path.Combine(libFolder, "Empty-databases", "roadkill-acceptancetests.sdf");
			File.Copy(testsDBPath, Path.Combine(SitePath, "App_Data", "roadkill-acceptancetests.sdf"), true);
		}

		protected void CreatePageWithTags(params string[] tags)
		{
			CreatePageWithTitleAndTags("My title", tags);
		}

		protected void CreatePageWithTitleAndTags(string title, params string[] tags)
		{
			Driver.FindElement(By.CssSelector("a[href='/pages/new']")).Click();
			Driver.FindElement(By.Name("Title")).SendKeys(title);
			//Driver.FindElement(By.Name("RawTags")).SendKeys("Tag1,Tag2");

			foreach (string tag in tags)
			{
				Driver.FindElement(By.Name("TagsEntry")).SendKeys(tag);
				Driver.FindElement(By.Name("TagsEntry")).SendKeys(Keys.Space);
			}

			Driver.FindElement(By.Name("Content")).SendKeys("Some content goes here");
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();
		}

		protected void LoginAsAdmin()
		{
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
		}

		protected void LoginAsEditor()
		{
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(EDITOR_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
		}

		protected void Logout()
		{
			Driver.Navigate().GoToUrl(LogoutUrl);
		}
	}
}
