using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Roadkill.Tests.Acceptance
{
	/// <summary>
	/// Nunit runs this once at the start of the test run when a test has the 'Roadkill.Tests.Acceptance' namespace.
	/// It's separate from AcceptanceTestBase so it isn't seen by nunit(dotcover) as a test.
	/// </summary>
	[SetUpFixture]
	public class AcceptanceTestsSetup
	{
		private IISExpress _iisExpress;
		public static IWebDriver Driver { get; private set; }

		[SetUp]
		public void Setup()
		{
			_iisExpress = new IISExpress();
			_iisExpress.Start();

			ConfigFileManager.CopyWebConfig();
			ConfigFileManager.CopyConnectionStringsConfig();
			ConfigFileManager.CopyRoadkillConfig();
			Driver = LaunchChrome();
		}

		[TearDown]
		public void TearDown()
		{
			Driver.Quit();

			if (_iisExpress != null)
			{
				_iisExpress.Dispose();
			}
		}

		private ChromeDriver LaunchChrome()
		{
			// Disable the remember password popups, and make sure it's full screen so that Bootstrap elements aren't hidden
			ChromeOptions options = new ChromeOptions();
			options.AddArgument("--incognito");
			options.AddArgument("--start-maximized");
			ChromeDriver chromeDriver = new ChromeDriver(options);
			chromeDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));

			return chromeDriver;
		}
	}
}
