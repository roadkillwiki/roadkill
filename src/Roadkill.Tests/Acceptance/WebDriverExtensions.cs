using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace OpenQA.Selenium
{
	/// <summary>
	/// A set of CSS and form based extension methods for <see cref="IWebDriver"/>.
	/// </summary>
	public static class WebDriverExtensions
	{
		/// <summary>
		/// Clicks a button that has the given value.
		/// </summary>
		/// <param name="buttonValue">The button's value (input[value=])</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void ClickButtonWithValue(this IWebDriver webdriver, string buttonValue)
		{
			webdriver.FindElement(By.CssSelector("input[value='" + buttonValue + "']")).Click();
		}

		/// <summary>
		/// Clicks a button with the id ending with the provided value.
		/// </summary>
		/// <param name="idEndsWith">A CSS id.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void ClickButtonWithId(this IWebDriver webdriver, string idEndsWith)
		{
			webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']")).Click();
		}

		/// <summary>
		/// Selects an item from a drop down box using the given CSS id and the itemIndex.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="itemIndex">A zero-based index that determines which drop down box to target from the CSS selector (assuming
		/// the CSS selector returns more than one drop down box).</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void SelectItemInList(this IWebDriver webdriver, string selector, int itemIndex)
		{
			SelectElement element = new SelectElement(webdriver.FindElement(By.CssSelector(selector)));
			element.SelectByIndex(itemIndex);
		}

		/// <summary>
		/// Selects an item from the nth drop down box (based on the elementIndex argument), using the given CSS id and the itemIndex.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="itemIndex">A zero-based index that determines which drop down box to target from the CSS selector (assuming
		/// the CSS selector returns more than one drop down box).</param>
		/// <param name="elementIndex">The element in the drop down list to select.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void SelectItemInList(this IWebDriver webdriver, string selector, int itemIndex, int elementIndex)
		{
			SelectElement element = new SelectElement(webdriver.FindElements(By.CssSelector(selector))[elementIndex]);
			element.SelectByIndex(itemIndex);
		}

		/// <summary>
		/// Returns the number of elements that match the given CSS selector.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>The number of elements found.</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static int ElementCount(this IWebDriver webdriver, string selector)
		{
			return webdriver.FindElements(By.CssSelector(selector)).Count;
		}

		/// <summary>
		/// Gets the selected index from a drop down box using the given CSS selector.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>The index of the selected item in the drop down box</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static int SelectedIndex(this IWebDriver webdriver, string selector)
		{
			SelectElement element = new SelectElement(webdriver.FindElement(By.CssSelector(selector)));

			for (int i = 0; i < element.Options.Count; i++)
			{
				if (element.Options[i].Selected)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Gets the selected index from the nth drop down box (based on the elementIndex argument), using the given CSS selector.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="itemIndex">A zero-based index that determines which drop down box to target from the CSS selector (assuming
		/// the CSS selector returns more than one drop down box).</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>The index of the selected item in the drop down box</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static int SelectedIndex(this IWebDriver webdriver, string selector, int itemIndex)
		{
			SelectElement element = new SelectElement(webdriver.FindElements(By.CssSelector(selector))[itemIndex]);

			for (int i = 0; i < element.Options.Count; i++)
			{
				if (element.Options[i].Selected)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Gets the selected value from a drop down box using the given CSS selector.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>The value of the selected item.</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static string SelectedItemValue(this IWebDriver webdriver, string selector)
		{
			SelectElement element = (SelectElement)webdriver.FindElement(By.CssSelector(selector));
			return element.SelectedOption.GetAttribute("value");
		}

		/// <summary>
		/// Clicks a link with the text provided. This is case sensitive and searches using an Xpath contains() search.
		/// </summary>
		/// <param name="linkContainsText">The link text to search for.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void ClickLinkWithText(this IWebDriver webdriver, string linkContainsText)
		{
			webdriver.FindElement(By.XPath("//a[contains(text(),'" + linkContainsText + "')]")).Click();
		}

		/// <summary>
		/// Clicks a link with the id ending with the provided value.
		/// </summary>
		/// <param name="idEndsWith">A CSS id.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void ClickLinkWithId(this IWebDriver webdriver, string idEndsWith)
		{
			webdriver.FindElement(By.CssSelector("a[id$='" + idEndsWith + "']")).Click();
		}

		/// <summary>
		/// Clicks an element using the given CSS selector.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void Click(this IWebDriver webdriver, string selector)
		{
			webdriver.FindElement(By.CssSelector(selector)).Click();
		}

		/// <summary>
		/// Clicks an element using the given CSS selector.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="itemIndex">A zero-based index that determines which element to target from the CSS selector (assuming
		/// the CSS selector returns more than one element).</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void Click(this IWebDriver webdriver, string selector, int itemIndex)
		{
			webdriver.FindElements(By.CssSelector(selector))[itemIndex].Click();
		}

		/// <summary>
		/// Gets an input element with the id ending with the provided value.
		/// </summary>
		/// <param name="idEndsWith">A CSS id.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>An <see cref="IWebElement"/> for the item matched.</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static IWebElement InputWithId(this IWebDriver webdriver, string idEndsWith)
		{
			return webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']"));
		}

		/// <summary>
		/// Gets an element's value with the id ending with the provided value.
		/// </summary>
		/// <param name="idEndsWith">A CSS id.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>The element's value.</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static string ElementValueWithId(this IWebDriver webdriver, string idEndsWith)
		{
			return webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']")).GetAttribute("value");
		}

		/// <summary>
		/// Gets an element's value using the given CSS selector.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>The element's value.</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static string ElementValue(this IWebDriver webdriver, string selector)
		{
			return webdriver.FindElement(By.CssSelector(selector)).GetAttribute("value");
		}

		/// <summary>
		/// Gets an element's value using the given CSS selector.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="itemIndex">A zero-based index that determines which element to target from the CSS selector (assuming
		/// the CSS selector returns more than one element).</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>The element's value.</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static string ElementValue(this IWebDriver webdriver, string selector, int itemIndex)
		{
			return webdriver.FindElements(By.CssSelector(selector))[itemIndex].GetAttribute("value");
		}

		/// <summary>
		/// Gets an element's text using the given CSS selector.
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="itemIndex">A zero-based index that determines which element to target from the CSS selector (assuming
		/// the CSS selector returns more than one element).</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>The element's text.</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static string ElementText(this IWebDriver webdriver, string selector, int itemIndex)
		{
			return webdriver.FindElements(By.CssSelector(selector))[itemIndex].Text;
		}

		/// <summary>
		/// Return true if the checkbox with the id ending with the provided value is checked/selected.
		/// </summary>
		/// <param name="idEndsWith">A CSS id.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <returns>True if the checkbox is checked.</returns>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static bool IsCheckboxChecked(this IWebDriver webdriver, string idEndsWith)
		{
			return webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']")).Selected;
		}

		/// <summary>
		/// Clicks the checkbox with the id ending with the provided value.
		/// </summary>
		/// <param name="idEndsWith">A CSS id.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void ClickCheckbox(this IWebDriver webdriver, string idEndsWith)
		{
			webdriver.FindElement(By.CssSelector("input[id$='" + idEndsWith + "']")).Click();
		}

		/// <summary>
		/// Sets an element's (an input field) value to the provided text by using SendKeys().
		/// </summary>
		/// <param name="value">The text to type.</param>
		/// <param name="element">A <see cref="IWebElement"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void SetValue(this IWebElement element, string value)
		{
			element.SendKeys(value);
		}

		/// <summary>
		/// Sets an element's (an input field) value to the provided text, using the given CSS selector and using SendKeys().
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="value">The text to type.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void SetValue(this IWebDriver webdriver, string selector, string value)
		{
			webdriver.FindElement(By.CssSelector(selector)).Clear();
			webdriver.FindElement(By.CssSelector(selector)).SendKeys(value);
		}

		/// <summary>
		/// Sets an element's (an input field) value to the provided text, using the given CSS selector and using SendKeys().
		/// </summary>
		/// <param name="selector">A valid CSS selector.</param>
		/// <param name="value">The text to type.</param>
		/// <param name="itemIndex">A zero-based index that determines which element to target from the CSS selector (assuming
		/// the CSS selector returns more than one element).</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void SetValue(this IWebDriver webdriver, string selector, string value, int itemIndex)
		{
			webdriver.FindElements(By.CssSelector(selector))[itemIndex].Clear();
			webdriver.FindElements(By.CssSelector(selector))[itemIndex].SendKeys(value);
		}

		/// <summary>
		/// Sets the textbox with the given CSS id to the provided value.
		/// </summary>
		/// <param name="idEndsWith">A CSS id.</param>
		/// <param name="value">The text to type.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void FillTextBox(this IWebDriver webdriver, string idEndsWith, string value)
		{
			webdriver.SetValue("input[id$='" + idEndsWith + "']", value);
		}

		/// <summary>
		/// Sets the textarea with the given CSS id to the provided value.
		/// </summary>
		/// <param name="value">The text to set the value to.</param>
		/// <param name="idEndsWith">A CSS id.</param>
		/// <param name="webdriver">A <see cref="IWebDriver"/> instance.</param>
		/// <exception cref="OpenQA.Selenium.NoSuchElementException">No element was found.</exception>
		public static void FillTextArea(this IWebDriver webdriver, string idEndsWith, string value)
		{
			webdriver.SetValue("textarea[id$='" + idEndsWith + "']", value);
		}

		public static IWebElement WaitForElementDisplayed(this IWebDriver driver, By by, int timeoutInSeconds = 10)
		{
			try
			{
				WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
				if (wait.Until<bool>(x => x.FindElement(by).Displayed))
				{
					return driver.FindElement(by);
				}
				else
				{
					ITakesScreenshot screenshotDriver = driver as ITakesScreenshot;

					if (screenshotDriver != null)
					{
						string filename = string.Format("webdriverwait-failure{0}.png", DateTime.Now.Ticks);
						string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
						Screenshot screenshot = screenshotDriver.GetScreenshot();
						screenshot.SaveAsFile(fullPath, ImageFormat.Png);

						Console.WriteLine("Took screenshot: {0} ", fullPath);
					}

					return null;
				}
			}
			catch (WebDriverException e)
			{
				Assert.Fail("Unable to find element '{0}' on '{1}' - {2}", by.ToString(), driver.Url, e.Message);
				return null;
			}
		}

		public static bool IsElementDisplayed(this IWebDriver driver, By by, int timeoutInSeconds = 10)
		{
			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));

			try
			{
				bool result = wait.Until<bool>(x => x.FindElement(by).Displayed);
				return result;
			}
			catch (WebDriverException e)
			{
				Assert.Fail("Unable to find element '{0}' on '{1}' - {2}", by.ToString(), driver.Url, e.Message);
				return false;
			}
		}
	}
}
