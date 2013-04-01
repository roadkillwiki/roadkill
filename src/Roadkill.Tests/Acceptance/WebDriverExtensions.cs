using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Roadkill.Tests.Acceptance
{
	public static class WebDriverExtensions
	{
		public static IWebElement WaitForElementDisplayed(this IWebDriver driver, By by, int timeoutInSeconds = 10)
		{
			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
			if (wait.Until<bool>(x => x.FindElement(by).Displayed))
			{
				return driver.FindElement(by);
			}
			else
			{
				return null;
			}
		}

		public static bool IsElementDisplayed(this IWebDriver driver, By by, int timeoutInSeconds = 10)
		{
			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
			return wait.Until<bool>(x => x.FindElement(by).Displayed);
		}
	}
}
