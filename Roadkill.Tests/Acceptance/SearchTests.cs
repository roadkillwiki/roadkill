using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core;
using Roadkill.Tests.Core;
using NUnit.Framework;
using System.IO;

using Plasma.Core;
using Plasma.WebDriver;
using OpenQA.Selenium;
using System.Diagnostics;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using SimpleBrowser.WebDriver;

namespace Roadkill.Tests.Acceptance
{
	/// <summary>
	/// Web tests using a headless browser (non-javascript interaction)
	/// </summary>
	[TestFixture]
	[Category("Acceptance")]
	public class SearchTests
	{
		private SimpleBrowserDriver _driver;

		[TestFixtureSetUp]
		public void Setup()
		{
			_driver = new SimpleBrowserDriver();
			_driver.Navigate().GoToUrl(Settings.HeadlessUrl);
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			_driver.Dispose();
		}
	}
}
