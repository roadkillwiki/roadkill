using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Reflection;
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
		protected bool IsWindowsAuthTests;

		[SetUp]
		public void Setup()
		{
			string url = ConfigurationManager.AppSettings["url"];
			if (string.IsNullOrEmpty(url))
				url = "http://localhost:9876";

			CopyDb();
			BaseUrl = url;
			LoginUrl = BaseUrl + "/user/login";
			LogoutUrl = BaseUrl + "/user/logout";
			Driver = AcceptanceTestsSetup.Driver;

			IsWindowsAuthTests = (ConfigurationManager.AppSettings["useWindowsAuth"] == "true");
		}

		protected void CopyDb()
		{
			SitePath = AcceptanceTestsSetup.GetSitePath();

			string testsDBPath = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "roadkill-acceptancetests.sdf");
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
			if (IsWindowsAuthTests)
				return;

			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
		}

		protected void LoginAsEditor()
		{
			if (IsWindowsAuthTests)
				return;

			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(EDITOR_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
		}

		protected void Logout()
		{
			if (IsWindowsAuthTests)
				return;

			Driver.Navigate().GoToUrl(LogoutUrl);
		}
	}
}
