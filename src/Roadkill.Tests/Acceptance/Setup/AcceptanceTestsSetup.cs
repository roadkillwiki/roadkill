using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;

namespace Roadkill.Tests.Acceptance
{
	/// <summary>
	/// Separate from the TestBase so it isn't run by nunit as a test.
	/// </summary>
	[SetUpFixture]
	public class AcceptanceTestsSetup
	{
		public static IWebDriver Driver { get; private set; }
		public static Process IisProcess { get; private set; }

		[SetUp]
		public void BeforeAllTests()
		{
			CopyDbBinaries();
			CopyWebConfig();
			LaunchIisExpress();

			//Driver = new FirefoxDriver();
			Driver = new ChromeDriver();
			//Driver = new PhantomJSDriver();

			try
			{
				Driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));
			}
			catch (NotImplementedException) { }
		}

		[TearDown]
		public void AfterAllTests()
		{
			Driver.Quit();

			if (IisProcess != null && !IisProcess.HasExited)
			{
				IisProcess.CloseMainWindow();
				IisProcess.Dispose();
				Console.WriteLine("Killed IISExpress");
			}
		}

		public static string GetSitePath()
		{
			string sitePath = Path.Combine(GlobalSetup.ROOT_FOLDER, "src", "Roadkill.Site");
			sitePath = new DirectoryInfo(sitePath).FullName;

			return sitePath;
		}

		private void CopyWebConfig()
		{
			try
			{
				string sitePath = GetSitePath();
				string siteWebConfig = Path.Combine(sitePath, "web.config");

				string testsWebConfigPath = Path.Combine(GlobalSetup.LIB_FOLDER, "Configs", "web.acceptancetests.config");
				Console.WriteLine("Original web.config path: {0}", siteWebConfig);
				Console.WriteLine("Acceptance tests web.config path: {0}", testsWebConfigPath);

				// Be a good neighbour and backup the web.config
				File.Copy(siteWebConfig, siteWebConfig + ".bak", true);
				Console.WriteLine("Backed up web.config to {0}.bak", siteWebConfig);

				File.Copy(testsWebConfigPath, siteWebConfig, true);
				Console.WriteLine("Copied web.config from '{0}' to '{1}'", testsWebConfigPath, siteWebConfig);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void CopyDbBinaries()
		{
			string sitePath = GetSitePath();

			string sqliteFileSource = string.Format("{0}/App_Data/SQLiteBinaries/x86/System.Data.SQLite.dll", sitePath);
			string sqliteFileDest = string.Format("{0}/bin/System.Data.SQLite.dll", sitePath);
			string sqliteLinqFileSource = string.Format("{0}/App_Data/SQLiteBinaries/x86/System.Data.SQLite.Linq.dll", sitePath);
			string sqliteFileLinqDest = string.Format("{0}/bin/System.Data.SQLite.Linq.dll", sitePath);

			string sqlCeLinqFileSource = string.Format("{0}/App_Data/SQLiteBinaries/x86/System.Data.SQLite.Linq.dll", sitePath);
			string sqlCeFileLinqDest = string.Format("{0}/bin/System.Data.SQLite.Linq.dll", sitePath);

			if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
			{
				sqliteFileSource = string.Format("{0}/App_Data/SQLiteBinaries/x64/System.Data.SQLite.dll", sitePath);
				sqliteLinqFileSource = string.Format("{0}/App_Data/SQLiteBinaries/x64/System.Data.SQLite.Linq.dll", sitePath);
			}

			if (!System.IO.File.Exists(sqliteFileDest))
				System.IO.File.Copy(sqliteFileSource, sqliteFileDest);

			if (!System.IO.File.Exists(sqliteFileLinqDest))
				System.IO.File.Copy(sqliteLinqFileSource, sqliteFileLinqDest);
		}

		private void LaunchIisExpress()
		{
			string sitePath = GetSitePath();
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.Arguments = string.Format("/path:\"{0}\" /port:{1}", sitePath, 9876);

			string programfiles = programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			string searchPath1 = string.Format(@"{0}\IIS Express\iisexpress.exe", programfiles);
			string searchPath2 = "";
			startInfo.FileName = string.Format(@"{0}\IIS Express\iisexpress.exe", programfiles);

			if (!File.Exists(startInfo.FileName))
			{
				programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
				searchPath2 = string.Format(@"{0}\IIS Express\iisexpress.exe", programfiles);
				startInfo.FileName = string.Format(@"{0}\IIS Express\iisexpress.exe", programfiles);
			}

			if (!File.Exists(startInfo.FileName))
			{
				throw new FileNotFoundException(string.Format("IIS Express is not installed in '{0}' or '{1}' and is required for the acceptance tests\n " +
					"Download it from http://www.microsoft.com/en-gb/download/details.aspx?id=1038",
					searchPath1, searchPath2));
			}

			try
			{
				Console.WriteLine("Launching IIS Express: {0} {1}", startInfo.FileName, startInfo.Arguments);
				IisProcess = Process.Start(startInfo);
			}
			catch
			{
				IisProcess.CloseMainWindow();
				IisProcess.Dispose();
			}
		}
	}
}
