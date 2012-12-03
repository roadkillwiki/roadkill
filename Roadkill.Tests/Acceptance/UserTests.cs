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
using System.Threading;
using OpenQA.Selenium.Firefox;

namespace Roadkill.Tests.Acceptance
{
	/// <summary>
	/// Web tests using a headless browser (non-javascript interaction)
	/// </summary>
	[TestFixture]
	[Category("Acceptance")]
	public class UserTests : AcceptanceTestsBase
	{
		[Test]
		public void Reset_Password_Sends_Email()
		{
			// Arrange
			foreach (string file in Directory.GetFiles(@"C:\inetpub\temp\smtp\", "*.eml"))
			{
				File.Delete(file);
			}
			
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);

			// Act
			Driver.FindElement(By.CssSelector("a[href='/user/resetpassword']")).Click();
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.CssSelector("input[value='Reset password']")).Click();
			
			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#content h1"))[0].Text, Contains.Substring("Your password reset request was sent."));
			Assert.That(Driver.FindElement(By.CssSelector("#content p")).Text, Contains.Substring("Thank you, an email has been sent to admin@localhost with details on how to reset your password."));
			Assert.That(Directory.GetFiles(@"C:\inetpub\temp\smtp\", "*.eml").Count(), Is.EqualTo(1));
		}

		[Test]
		public void Profile_Shows_Correct_Information()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/user/profile']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#Firstname")).GetAttribute("value"), Is.EqualTo("Chris"));
			Assert.That(Driver.FindElement(By.CssSelector("#Lastname")).GetAttribute("value"), Is.EqualTo("Admin"));
			Assert.That(Driver.FindElement(By.CssSelector("#NewEmail")).GetAttribute("value"), Is.EqualTo("admin@localhost"));
			Assert.That(Driver.FindElement(By.CssSelector("#NewUsername")).GetAttribute("value"), Is.EqualTo("admin"));
			Assert.That(Driver.FindElement(By.CssSelector("#Password")).GetAttribute("value"), Is.EqualTo(""));
			Assert.That(Driver.FindElement(By.CssSelector("#PasswordConfirmation")).GetAttribute("value"), Is.EqualTo(""));
		}

		[Test]
		public void Save_Profile_Shows_Correct_Information()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			Driver.FindElement(By.CssSelector("a[href='/user/profile']")).Click();	

			// Act
			Driver.FindElement(By.CssSelector("#Firstname")).Clear();
			Driver.FindElement(By.CssSelector("#Firstname")).SendKeys("NewFirstName");
			Driver.FindElement(By.CssSelector("#Lastname")).Clear();
			Driver.FindElement(By.CssSelector("#Lastname")).SendKeys("NewLastName");
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();
			Driver.FindElement(By.CssSelector("a[href='/user/profile']")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#Firstname")).GetAttribute("value"), Is.EqualTo("NewFirstName"));
			Assert.That(Driver.FindElement(By.CssSelector("#Lastname")).GetAttribute("value"), Is.EqualTo("NewLastName"));
			Assert.That(Driver.FindElement(By.CssSelector("#NewEmail")).GetAttribute("value"), Is.EqualTo("admin@localhost"));
			Assert.That(Driver.FindElement(By.CssSelector("#NewUsername")).GetAttribute("value"), Is.EqualTo("admin"));
			Assert.That(Driver.FindElement(By.CssSelector("#Password")).GetAttribute("value"), Is.EqualTo(""));
			Assert.That(Driver.FindElement(By.CssSelector("#PasswordConfirmation")).GetAttribute("value"), Is.EqualTo(""));
		}

		[Test]
		public void Save_Profile_Password_Changes_Password()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(EDITOR_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			Driver.FindElement(By.CssSelector("a[href='/user/profile']")).Click();	

			// Act
			Driver.FindElement(By.CssSelector("#Password")).Clear();
			Driver.FindElement(By.CssSelector("#Password")).SendKeys("newpassword");
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).Clear();
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).SendKeys("newpassword");
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();
			Driver.Navigate().GoToUrl(LogoutUrl);

			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys("newpassword");
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			
			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas a")).Text, Is.EqualTo("Logged in as editor"));
		}

		[Test]
		public void All_Pages_Has_Edit_Delete_For_Admin()
		{
			//
			// Arrange
			//
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			// 1st new page
			Driver.FindElement(By.CssSelector("a[href='/pages/new']")).Click();
			Driver.FindElement(By.CssSelector("#Title")).SendKeys("My title1");

			if (Driver is SimpleBrowserDriver)
			{
				Driver.FindElement(By.Name("RawTags")).SendKeys("Homepage");
			}
			else
			{
				Driver.FindElement(By.Name("TagsEntry")).SendKeys("Homepage");
				Driver.FindElement(By.Name("TagsEntry")).SendKeys(Keys.Space);
			}
			Driver.FindElement(By.Name("Content")).SendKeys("Some content goes here1");
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();

			// 2nd new page
			Driver.FindElement(By.CssSelector("a[href='/pages/new']")).Click();
			Driver.FindElement(By.Name("Title")).SendKeys("My title2");
			if (Driver is SimpleBrowserDriver)
			{
				Driver.FindElement(By.Name("RawTags")).SendKeys("Tag2");
			}
			else
			{
				Driver.FindElement(By.Name("TagsEntry")).SendKeys("Tag2");
				Driver.FindElement(By.Name("TagsEntry")).SendKeys(Keys.Space);
			}
			Driver.FindElement(By.Name("Content")).SendKeys("Some content goes here2");
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();

			//
			// Act
			//
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table>tbody>tr")).Count, Is.EqualTo(2));
		}

		[Test]
		public void All_Pages_Has_Edit_NoDelete_For_Editor()
		{

		}

		[Test]
		public void All_Pages_Has_No_Buttons_For_Anonymous()
		{

		}

		///////


		[Test]
		public void TagPage_Has_Edit_Delete_For_Admin()
		{

		}

		[Test]
		public void TagPage_Has_Edit_NoDelete_For_Editor()
		{

		}

		[Test]
		public void TagPage_Has_No_Buttons_For_Anonymous()
		{

		}
		
		///////


		[Test]
		public void ByUserPage_Has_Edit_Delete_For_Admin()
		{

		}

		[Test]
		public void ByUserPage_Has_Edit_NoDelete_For_Editor()
		{

		}

		[Test]
		public void ByUserPage_Has_No_Buttons_For_Anonymous()
		{

		}

		///////


		[Test]
		public void HistoryPage_Has_Revert_For_Admin()
		{

		}

		[Test]
		public void HistoryPage_Has_Revert_For_Editor()
		{

		}

		[Test]
		public void HistoryPage_Has_No_Revert_For_Anonymous()
		{

		}
	}
}
