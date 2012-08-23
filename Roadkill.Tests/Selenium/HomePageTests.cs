using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Plasma.Core;
using NUnit.Framework;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using System.Configuration;

namespace Roadkill.Tests.Selenium
{
	/// <summary>
	/// These tests are run via Saucelab.com. The API key and username come from Appharbor's
	/// configuration settings - to avoid going over the 200 minutes per month, these are kept private.
	/// 
	/// You can always run these tests on your desktop machine by changing the _useSaucelabs variable to false.
	/// </summary>
	public class HomePageTests
	{
		private static IWebDriver _webDriver;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			if (SeleniumSettings.UseSaucelabs && SeleniumSettings.HasValidSaucelabsKey == false)
			{
				Assert.Ignore("Saucelabs tests require an access key configuration setting");
			}
			else
			{
				_webDriver = GetWebDriver();
			}
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_webDriver.Close();
		}

		[Test]
		public void Homepage_HasLeftMenu()
		{
			_webDriver.Navigate().GoToUrl(SeleniumSettings.GetUrl("/"));

			Assert.That(_webDriver.FindElement(By.CssSelector("#leftmenu ul li>a")).Text, Is.EqualTo("Main Page"));
			Assert.That(_webDriver.FindElement(By.CssSelector("#leftmenu ul li+li>a")).Text, Is.EqualTo("Categories"));
			Assert.That(_webDriver.FindElement(By.CssSelector("#leftmenu ul li+li+li>a")).Text, Is.EqualTo("All pages"));
		}

		private static IWebDriver GetWebDriver()
		{
			if (SeleniumSettings.UseSaucelabs)
			{
				DesiredCapabilities capabillities = DesiredCapabilities.Firefox();
				capabillities.SetCapability(CapabilityType.Version, "10");
				capabillities.SetCapability(CapabilityType.IsJavaScriptEnabled, true);
				capabillities.SetCapability(CapabilityType.Platform, new Platform(PlatformType.Windows));
				capabillities.SetCapability("name", "Roadkill homepage tests");
				capabillities.SetCapability("username", ConfigurationManager.AppSettings["Saucelabs_Username"]);
				capabillities.SetCapability("accessKey", ConfigurationManager.AppSettings["Saucelabs_AccessKey"]);

				return new RemoteWebDriver(new Uri("http://ondemand.saucelabs.com:80/wd/hub"), capabillities);
			}
			else
			{
				return new FirefoxDriver();
			}
		}
	}
}
