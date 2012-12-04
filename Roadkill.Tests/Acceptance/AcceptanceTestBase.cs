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
			string sitePath = AcceptanceTestsSetup.GetSitePath();
			string libFolder = Path.Combine(sitePath, "..", "lib");
			libFolder = new DirectoryInfo(libFolder).FullName;

			string testsDBPath = Path.Combine(libFolder, "Empty-databases", "roadkill-acceptancetests.sdf");
			File.Copy(testsDBPath, Path.Combine(sitePath, "App_Data", "roadkill-acceptancetests.sdf"), true);
		}
	}
}
