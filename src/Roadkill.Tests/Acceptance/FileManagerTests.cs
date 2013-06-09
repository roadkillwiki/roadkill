using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class FileManagerTests : AcceptanceTestBase
	{
		[SetUp]
		public void Setup()
		{
			// Re-create the attachments directory
			string sitePath = AcceptanceTestsSetup.GetSitePath();
			string attachmentsPath = Path.Combine(sitePath, "App_Data", "Attachments");
			if (Directory.Exists(attachmentsPath))
				Directory.Delete(attachmentsPath, true);

			Directory.CreateDirectory(attachmentsPath);
		}
		
		[TearDown]
		public void TearDown()
		{
			// Remove everything from the attachments directory
			string sitePath = AcceptanceTestsSetup.GetSitePath();
			string attachmentsPath = Path.Combine(sitePath, "App_Data", "Attachments");
			if (Directory.Exists(attachmentsPath))
				Directory.Delete(attachmentsPath, true);

			Directory.CreateDirectory(attachmentsPath);

			// Recreate the emptyfile.txt file that VS publish needs
			string emptyFilePath = Path.Combine(attachmentsPath, "emptyfile.txt");
			File.WriteAllText(emptyFilePath, "");
		}

		[Test]
		public void Editor_Login_Should_Only_Show_Upload_And_New_Folder_Buttons()
		{
			// Arrange
			LoginAsEditor();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".fileupload-buttonbar")).Displayed, Is.True, ".fileupload-buttonbar");
			Assert.That(Driver.FindElement(By.CssSelector("#addfolderbtn")).Displayed, Is.True, "#addfolderbtn");
			Assert.That(Driver.FindElements(By.CssSelector("#deletefilebtn")).Count(), Is.EqualTo(0), "#deletefilebtn");
			Assert.That(Driver.FindElements(By.CssSelector("#deletefolderbtn")).Count(), Is.EqualTo(0), "#deletefolderbtn");
		}

		[Test]
		public void Admin_Login_Should_Show_All_Buttons()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".fileupload-buttonbar")).Displayed, Is.True, ".fileupload-buttonbar");
			Assert.That(Driver.FindElement(By.CssSelector("#addfolderbtn")).Displayed, Is.True, "#addfolderbtn");
			Assert.That(Driver.FindElement(By.CssSelector("#deletefilebtn")).Displayed, Is.True, "#deletefilebtn");
			Assert.That(Driver.FindElement(By.CssSelector("#deletefolderbtn")).Displayed, Is.True, "#deletefolderbtn");
		}

		[Test]
		public void NewFolder_Displays_In_Table()
		{
			// Arrange
			LoginAsEditor();
			string folderName = "myfolder";

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();
			Driver.FindElement(By.CssSelector("#addfolderbtn")).Click();
			Driver.FindElement(By.CssSelector("#newfolderinput")).SendKeys(folderName);
			Driver.FindElement(By.CssSelector("#newfolderinput")).SendKeys(Keys.Return);

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("table#files tr")).Count(), Is.EqualTo(2));
			Assert.That(Driver.FindElements(By.CssSelector("tr[data-itemtype='folder'] td"))[1].Text, Is.EqualTo(folderName), "tr[data-itemtype='folder']");
		}

		[Test]
		public void Upload_File_Shows_Toast_And_Displays_In_Table()
		{
			// Arrange
			LoginAsEditor();
			string sitePath = AcceptanceTestsSetup.GetSitePath();
			string file = Path.Combine(sitePath, "Themes", "Mediawiki", "logo.png");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();
			MakeFileInputVisible();
			Driver.FindElement(By.CssSelector("#fileupload")).SendKeys(file);

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".toast-success")).Displayed, Is.True, ".toast-success");
			Assert.That(Driver.FindElements(By.CssSelector("table#files td.file")).Count(), Is.EqualTo(1));
			Assert.That(Driver.FindElement(By.CssSelector("table#files td.file")).Text, Is.EqualTo("logo.png"));
		}

		// [X] Delete file
		// [X] Delete folder
		// [X] Navigate 2 deep folders
		// [X] Navigate 2 deep folders + use nav bar
		// [X] Upload + select in page editor

		private void MakeFileInputVisible()
		{
			// Remove the <input type="file">'s parent, so it Selenium can target it.
			IJavaScriptExecutor js = Driver as IJavaScriptExecutor;
			js.ExecuteScript("$('#fileupload').unwrap();");
		}
	}
}
