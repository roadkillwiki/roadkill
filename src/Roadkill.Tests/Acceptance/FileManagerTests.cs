using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
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
			string sitePath = Settings.WEB_PATH;
			string attachmentsPath = Path.Combine(sitePath, "App_Data", "Attachments");
			if (Directory.Exists(attachmentsPath))
				Directory.Delete(attachmentsPath, true);

			Directory.CreateDirectory(attachmentsPath);
		}
		
		[TearDown]
		public void TearDown()
		{
			// Remove everything from the attachments directory
			string sitePath = Settings.WEB_PATH;
			string attachmentsPath = Path.Combine(sitePath, "App_Data", "Attachments");
			if (Directory.Exists(attachmentsPath))
				Directory.Delete(attachmentsPath, true);

			Directory.CreateDirectory(attachmentsPath);

			// Recreate the emptyfile.txt file that VS publish needs
			string emptyFilePath = Path.Combine(attachmentsPath, "emptyfile.txt");
			File.WriteAllText(emptyFilePath, "");
		}

		[Test]
		public void File_Table_Should_List_Folders_Then_Files()
		{
			// Arrange
			string sitePath = Settings.WEB_PATH;
			string fileSource = Path.Combine(sitePath, "Themes", "Mediawiki", "logo.png");
			string fileDest = Path.Combine(sitePath, "App_Data", "Attachments", "logo.png");
			File.Copy(fileSource, fileDest);

			string folder1Path = Path.Combine(sitePath, "App_Data", "Attachments", "folder1");
			Directory.CreateDirectory(folder1Path);

			string folder2Path = Path.Combine(sitePath, "App_Data", "Attachments", "folder2");
			Directory.CreateDirectory(folder2Path);

			LoginAsEditor();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("table#files tbody tr")).Count(), Is.EqualTo(3));
			Assert.That(Driver.FindElement(By.CssSelector("table#files tbody tr td+td")).Text, Is.EqualTo("folder1"));
			Assert.That(Driver.FindElement(By.CssSelector("table#files tbody tr+tr td+td")).Text, Is.EqualTo("folder2"));
			Assert.That(Driver.FindElement(By.CssSelector("table#files tbody tr+tr+tr td+td")).Text, Is.EqualTo("logo.png"));
		}

		[Test]
		public void Editor_Login_Should_Only_Show_Upload_And_New_Folder_Buttons()
		{
			// Arrange
			LoginAsEditor();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#fileupload-buttonbar")).Displayed, Is.True, "#fileupload-buttonbar");
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
			Assert.That(Driver.FindElement(By.CssSelector("#fileupload-buttonbar")).Displayed, Is.True, ".fileupload-buttonbar");
			Assert.That(Driver.FindElement(By.CssSelector("#addfolderbtn")).Displayed, Is.True, "#addfolderbtn");
			Assert.That(Driver.FindElement(By.CssSelector("#deletefilebtn")).Displayed, Is.True, "#deletefilebtn");
			Assert.That(Driver.FindElement(By.CssSelector("#deletefolderbtn")).Displayed, Is.True, "#deletefolderbtn");
		}

		[Test]
		[Explicit("Failing on Teamcity, but works locally")]
		public void NewFolder_Should_Display_In_Table()
		{
			// Arrange
			LoginAsEditor();
			string folderName = "myfolder";

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#addfolderbtn"), 5);
			Driver.FindElement(By.CssSelector("#addfolderbtn")).Click();

			// #newfolderinput
			IWebElement inputBox = Driver.FindElement(By.CssSelector("table#files input"));
			inputBox.SendKeys(folderName);
			inputBox.SendKeys(Keys.Return);
			WaitForAjaxToComplete();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("table#files tr")).Count(), Is.EqualTo(2));
			Assert.That(Driver.FindElements(By.CssSelector("tr[data-itemtype='folder'] td"))[1].Text, Is.EqualTo(folderName), "tr[data-itemtype='folder']");
		}

		[Test]
		public void Upload_File_Should_Show_Toast_And_Displays_In_Table()
		{
			// Arrange
			LoginAsEditor();
			string sitePath = Settings.WEB_PATH;
			string file = Path.Combine(sitePath, "Themes", "Mediawiki", "logo.png");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();
			MakeFileInputVisible();
			Driver.FindElement(By.CssSelector("#fileupload")).SendKeys(file);
			WaitForAjaxToComplete();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".toast-success")).Displayed, Is.True, ".toast-success");
			Assert.That(Driver.FindElements(By.CssSelector("table#files td.file")).Count(), Is.EqualTo(1));
			Assert.That(Driver.FindElement(By.CssSelector("table#files td.file")).Text, Is.EqualTo("logo.png"));
		}

		[Test]
		public void Delete_File_Should_Show_Toast_And_Not_Show_File_In_Table()
		{
			// Arrange
			string sitePath = Settings.WEB_PATH;
			string fileSource = Path.Combine(sitePath, "Themes", "Mediawiki", "logo.png");
			string fileDest = Path.Combine(sitePath, "App_Data", "Attachments", "logo.png");
			File.Copy(fileSource, fileDest);

			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();
			Driver.FindElement(By.CssSelector("td.file")).Click();
			Driver.FindElement(By.CssSelector("#deletefilebtn")).Click();
			Driver.FindElement(By.CssSelector(".bootbox button.btn-primary")).Click();
			WaitForAjaxToComplete();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".toast-info")).Displayed, Is.True, ".toast-info");
			Assert.That(Driver.FindElements(By.CssSelector("table#files td.file")).Count(), Is.EqualTo(0));
		}

		[Test]
		public void Delete_Folder_Should_Show_Toast_And_Not_Show_Folder_In_Table()
		{
			// Arrange
			string sitePath = Settings.WEB_PATH;
			string folderPath = Path.Combine(sitePath, "App_Data", "Attachments", "RandomFolder");
			Directory.CreateDirectory(folderPath);

			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();
			Driver.FindElement(By.CssSelector("table#files tbody tr td+td")).Click();
			Driver.FindElement(By.CssSelector("#deletefolderbtn")).Click();
			Driver.FindElement(By.CssSelector(".bootbox button.btn-primary")).Click();
			WaitForAjaxToComplete();

			// Assert
			Driver.WaitForElementDisplayed(By.CssSelector(".toast-info"));
			Assert.That(Driver.FindElement(By.CssSelector(".toast-info")).Displayed, Is.True, ".toast-info");
			Assert.That(Driver.FindElements(By.CssSelector("table#files tbody tr")).Count(), Is.EqualTo(0));
		}

		[Test]
		public void Navigate_SubFolders_Should_Work_With_Double_Click()
		{
			// Arrange
			string sitePath = Settings.WEB_PATH;
			string folderPath = Path.Combine(sitePath, "App_Data", "Attachments", "folder");
			Directory.CreateDirectory(folderPath);
			
			string subfolderPath = Path.Combine(folderPath, "subfolder");
			Directory.CreateDirectory(subfolderPath);

			LoginAsEditor();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();

			IWebElement td = Driver.FindElement(By.CssSelector("table#files tbody tr td+td"));
			Actions action = new Actions(Driver);
			action.DoubleClick(td).Perform();
			WaitForAjaxToComplete();

			td = Driver.FindElement(By.CssSelector("table#files tbody tr td+td"));
			action = new Actions(Driver);
			action.DoubleClick(td).Perform();
			WaitForAjaxToComplete();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("table#files tbody tr")).Count(), Is.EqualTo(0));
			Assert.That(Driver.FindElements(By.CssSelector("#path-navigator li"))[0].Text, Is.EqualTo("/"));
			Assert.That(Driver.FindElements(By.CssSelector("#path-navigator li"))[1].Text, Is.EqualTo("folder"));
			Assert.That(Driver.FindElements(By.CssSelector("#path-navigator li"))[2].Text, Is.EqualTo("subfolder"));
		}

		[Test]
		[Explicit("This test has timing issues")]
		public void Navigate_Folders_With_Crumb_Trail_Should_Update_Table_And_Crumb_Trail()
		{
			// Arrange
			string sitePath = Settings.WEB_PATH;
			string folderPath = Path.Combine(sitePath, "App_Data", "Attachments", "folder");
			Directory.CreateDirectory(folderPath);

			string subfolderPath = Path.Combine(folderPath, "subfolder");
			Directory.CreateDirectory(subfolderPath);

			LoginAsEditor();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/filemanager']")).Click();

			IWebElement td = Driver.FindElement(By.CssSelector("table#files tbody tr td+td"));
			Actions action = new Actions(Driver);
			action.DoubleClick(td).Perform(); // go to 1st folder
			WaitForAjaxToComplete();

			td = Driver.FindElement(By.CssSelector("table#files tbody tr td+td"));
			action = new Actions(Driver);
			action.DoubleClick(td).Perform(); // go to 2nd folder
			WaitForAjaxToComplete();
			Thread.Sleep(500);

			Driver.FindElements(By.CssSelector("#path-navigator li"))[1].Click();
			WaitForAjaxToComplete();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("table#files tbody tr")).Count(), Is.EqualTo(1));
			Assert.That(Driver.FindElement(By.CssSelector("table#files tbody tr td+td")).Text, Is.EqualTo("subfolder"));

			Driver.FindElements(By.CssSelector("#path-navigator li"))[1].Click(); // use the current node, just to be sure it does nothing
			Driver.FindElements(By.CssSelector("#path-navigator li"))[0].Click();
			WaitForAjaxToComplete();

			Assert.That(Driver.FindElements(By.CssSelector("table#files tbody tr")).Count(), Is.EqualTo(1));
			Assert.That(Driver.FindElement(By.CssSelector("table#files tbody tr td+td")).Text, Is.EqualTo("folder"));
		}

		[Test]
		public void Select_File_In_Page_Editor_Should_Add_Markup()
		{
			// Arrange
			string sitePath = Settings.WEB_PATH;
			string fileSource = Path.Combine(sitePath, "Themes", "Mediawiki", "logo.png");
			string fileDest = Path.Combine(sitePath, "App_Data", "Attachments", "logo.png");
			File.Copy(fileSource, fileDest);

			LoginAsEditor();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/new']")).Click();
			Driver.FindElement(By.CssSelector(".wysiwyg-picture")).Click();
			Driver.SwitchTo().Frame(0); // the iframe modal
			Driver.FindElement(By.CssSelector("table#files tbody tr td+td")).Click();
			Driver.SwitchTo().DefaultContent();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("textarea#Content")).GetAttribute("value"),
						Is.EqualTo("{{/logo.png|Image title}}"));
		}

		public void WaitForAjaxToComplete()
		{
			// Borrowed from http://stackoverflow.com/a/7203819/21574
			int tries = 0;
			while (tries < 5)
			{
				var ajaxIsComplete = (bool)(Driver as IJavaScriptExecutor).ExecuteScript("return jQuery.active == 0");
				if (ajaxIsComplete)
					break;
				
				Thread.Sleep(100);
				tries++;
			}
		}

		private void MakeFileInputVisible()
		{
			// Remove the <input type="file">'s parent, so it Selenium can target it.
			IJavaScriptExecutor js = Driver as IJavaScriptExecutor;
			js.ExecuteScript("$('#fileupload').unwrap();");
		}
	}
}
