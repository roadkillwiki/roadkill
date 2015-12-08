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
		public static IWebDriver Driver { get; private set; }

		[SetUp]
		public void Setup()
		{
			TestHelpers.CopyWebConfig();
			TestHelpers.CopyConnectionStringsConfig();
			TestHelpers.CopyRoadkillConfig();
			Driver = LaunchChrome();
		}

		[TearDown]
		public void TearDown()
		{
			Driver.Quit();
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
