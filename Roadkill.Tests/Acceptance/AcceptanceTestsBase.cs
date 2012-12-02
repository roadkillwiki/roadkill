using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using SimpleBrowser.WebDriver;

namespace Roadkill.Tests.Acceptance
{
	[Category("Acceptance")]
	public class AcceptanceTestsBase
	{
		protected static readonly string ADMIN_EMAIL = "admin@localhost";
		protected static readonly string ADMIN_PASSWORD = "password";

		protected static readonly string EDITOR_EMAIL = "editor@localhost";
		protected static readonly string EDITOR_PASSWORD = "password";

		protected IWebDriver Driver;
		protected string LoginUrl;
		protected string BaseUrl;
		protected string LogoutUrl;
		private Process _iisProcess;

		[TestFixtureSetUp]
		public void Setup()
		{
			string sitePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Roadkill.Site");
			sitePath = new DirectoryInfo(sitePath).FullName;

			CopyWebConfigAndDb(sitePath);
			LaunchIisExpress(sitePath);

			BaseUrl = "http://localhost:9876";
			LoginUrl = BaseUrl + "/user/login";
			LogoutUrl = BaseUrl + "/user/logout";

			Driver = new SimpleBrowserDriver();
			Driver = new FirefoxDriver();
			Driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			Driver.Dispose();

			if (_iisProcess != null)
			{
				_iisProcess.CloseMainWindow();
				_iisProcess.Dispose();
			}
		}

		private void CopyWebConfigAndDb(string sitePath)
		{
			string libFolder = Path.Combine(sitePath, "..", "lib");
			libFolder = new DirectoryInfo(libFolder).FullName;

			string testsWebConfigPath = Path.Combine(libFolder, "Configs", "web.acceptancetests.config");
			string testsDBPath = Path.Combine(libFolder, "Empty-databases", "roadkill-acceptancetests.sdf");

			// Be a good neighbour and backup the web.config
			string siteWebConfig = Path.Combine(sitePath, "web.config");
			File.Copy(siteWebConfig, siteWebConfig +".bak", true);
			File.Copy(testsWebConfigPath, siteWebConfig, true);

			File.Copy(testsDBPath, Path.Combine(sitePath, "App_Data", "roadkill-acceptancetests.sdf"), true);
		}

		private void LaunchIisExpress(string sitePath)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.Arguments = string.Format("/path:\"{0}\" /port:{1}", sitePath, 9876);

			string programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			if (Environment.Is64BitOperatingSystem)
				programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

			startInfo.FileName = string.Format(@"{0}\IIS Express\iisexpress.exe", programfiles);

			if (!File.Exists(startInfo.FileName))
			{
				throw new FileNotFoundException("IIS Express is not installed and is required for the acceptance tests\n " +
					"Download it from http://www.microsoft.com/en-gb/download/details.aspx?id=1038");
			}

			try
			{
				_iisProcess = Process.Start(startInfo);
			}
			catch
			{
				_iisProcess.CloseMainWindow();
				_iisProcess.Dispose();
			}
		}
	}
}
