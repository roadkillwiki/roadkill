using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using Roadkill.Core.Logging;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class LogViewerTests : AcceptanceTestBase
	{
		private int _errorCount = 105;

		[SetUp]
		public void TestFixtureTearDown()
		{
			string logDir = Path.Combine(AcceptanceTestsSetup.GetSitePath(), "App_Data", "Logs");
			foreach (string logFile in Directory.EnumerateFiles(logDir, LogReader.LOG_FILE_SEARCHPATH))
			{
				File.Delete(logFile);
			}
		}

		[Test]
		[Ignore("TODO - xmls not being correctly generated")]
		public void Shows_All_Errors_In_List_By_Default()
		{
			// Arrange
			LoginAsAdmin();

			// Generate a lot of errors
			for (int i = 0; i < _errorCount; i++)
			{
				Driver.Navigate().GoToUrl(BaseUrl + "/help/showerror");
			}

			// Act
			Driver.Navigate().GoToUrl(BaseUrl);
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/logviewer']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.ClassName("entry-container")).Count, Is.GreaterThanOrEqualTo(_errorCount));
		}

		[Test]
		[Ignore("TODO - xmls not being correctly generated")]
		public void Details_Link_Shows_Full_Error_Message()
		{
			// Arrange
			LoginAsAdmin();
			Driver.Navigate().GoToUrl(BaseUrl + "/help/showerror");

			// Act
			Driver.Navigate().GoToUrl(BaseUrl);
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/logviewer']")).Click();
			Driver.FindElement(By.LinkText("Details")).Click();

			// Assert
			IWebElement element = Driver.FindElement(By.CssSelector(".entry-container pre.entrymessage"));
			Assert.That(element.Text, Is.StringStarting("Error caught on help.showerror"));
		}
	}
}
