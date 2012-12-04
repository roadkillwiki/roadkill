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
	[Explicit]
	public class PageTests : AcceptanceTestBase
	{
		[Test]
		public void No_MainPage_Set()
		{
			// Arrange + Act
			Driver.Navigate().GoToUrl(BaseUrl);
			
			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Is.EqualTo("You have no mainpage set"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Is.EqualTo("To set a main page, create a page and assign the tag 'homepage' to it."));
		}

		[Test]
		public void No_Extra_Menu_Options_For_Anonymous_Users()
		{
			// Arrange
			Logout();

			// Act
			IEnumerable<IWebElement> leftmenuItems = Driver.FindElements(By.CssSelector("div#leftmenu li"));

			// Assert
			Assert.That(leftmenuItems.Count(), Is.EqualTo(3));
		}

		private void Logout()
		{
			Driver.Navigate().GoToUrl(LogoutUrl);
		}

		[Test]
		public void Login_As_Admin_Shows_All_Left_Menu_Options()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			IEnumerable<IWebElement> leftmenuItems = Driver.FindElements(By.CssSelector("div#leftmenu li"));

			// Assert
			Assert.That(leftmenuItems.Count(), Is.EqualTo(5));
		}

		[Test]
		public void Login_As_Editor_Shows_Extra_Menu_Option()
		{
			// Arrange
			LoginAsEditor();

			// Act
			IEnumerable<IWebElement> leftmenuItems = Driver.FindElements(By.CssSelector("div#leftmenu li"));

			// Assert
			Assert.That(leftmenuItems.Count(), Is.EqualTo(4));
		}

		[Test]
		public void NewPage_With_Homepage_Tag_Shows_As_Homepage()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			CreatePageWithTags("Homepage");

			// Assert
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("My title"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		public void AllTagsPage_Displays_Correct_Tags_For_All_Users()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTags("Tag1", "Tag2");

			// Act	
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.FindElement(By.CssSelector("a[href='/pages/alltags']")).Click();
			
			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#tagcloud a")).Count, Is.EqualTo(2));
			Assert.That(Driver.FindElements(By.CssSelector("#tagcloud a"))[0].Text, Is.EqualTo("Tag1"));
			Assert.That(Driver.FindElements(By.CssSelector("#tagcloud a"))[1].Text, Is.EqualTo("Tag2"));
		}

		[Test]
		[RequiresBrowserWithJavascript]
		public void EditIcon_Exists_For_Editors()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#toolbar i.icon-pencil")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector("#pageedit-button")).Count, Is.EqualTo(1));
		}

		[Test]
		[RequiresBrowserWithJavascript]
		public void EditIcon_Exists_For_Admins()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#toolbar i.icon-pencil")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector("#pageedit-button")).Count, Is.EqualTo(1));
		}

		[Test]
		[RequiresBrowserWithJavascript]
		public void Properties_Icon_Exists_For_Editors()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#toolbar i.icon-book")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector("#pageinfo-button")).Count, Is.EqualTo(1));

			Driver.FindElement(By.CssSelector("#pageinfo-button")).Click();

			Assert.That(Driver.FindElement(By.CssSelector("#pageinformation")).GetCssValue("display"), Is.EqualTo("block"));
		}

		[Test]
		[RequiresBrowserWithJavascript]
		public void Properties_Icon_Exists_For_Admin()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#toolbar i.icon-book")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector("#pageinfo-button")).Count, Is.EqualTo(1));

			Driver.FindElement(By.CssSelector("#pageinfo-button")).Click();

			Assert.That(Driver.FindElement(By.CssSelector("#pageinformation")).GetCssValue("display"), Is.EqualTo("block"));
		}

		[Test]
		public void View_History_Link_Exists_For_All_Users()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#viewhistory")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElement(By.CssSelector("#viewhistory a")).Text, Is.EqualTo("View History"));
		}

		[Test]
		public void All_Pages_Has_Edit_But_No_Delete_Button_For_Editor()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(0));
		}

		[Test]
		public void All_Pages_Has_Edit_And_Delete_Button_For_Admin()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(1));
		}

		[Test]
		public void All_Pages_Has_No_Buttons_For_Anonymous()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTags("Homepage");
			Logout();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/alltags']")).Click();
			Driver.FindElement(By.CssSelector("#tagcloud a")).Click();	

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(0));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(0));
		}

		[Test]
		public void TagPage_Has_Edit_And_Delete_Button_For_Admin()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/alltags']")).Click();
			Driver.FindElement(By.CssSelector("#tagcloud a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(0));
		}

		[Test]
		public void TagPage_Has_Edit_But_No_Delete_Button_For_Editor()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/alltags']")).Click();
			Driver.FindElement(By.CssSelector("#tagcloud a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(0));
		}

		[Test]
		public void TagPage_Has_No_Buttons_For_Anonymous()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");
			Logout();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/alltags']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(0));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(0));
		}

		[Test]
		public void ByUserPage_Has_Edit_And_Delete_Button_For_Admin()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();
			Driver.FindElement(By.CssSelector("#viewhistory a")).Click();
			Driver.FindElement(By.CssSelector("#historytable .editedby a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(1));
		}

		[Test]
		public void ByUserPage_Has_Edit_But_No_Delete_Button_For_Editor()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();
			Driver.FindElement(By.CssSelector("#viewhistory a")).Click();
			Driver.FindElement(By.CssSelector("#historytable .editedby a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(0));
		}

		[Test]
		public void ByUserPage_Has_No_Buttons_For_Anonymous()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");
			Logout();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();
			Driver.FindElement(By.CssSelector("#viewhistory a")).Click();
			Driver.FindElement(By.CssSelector("#historytable .editedby a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(0));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(0));
		}

		///

		[Test]
		public void HistoryPage_Has_Revert_For_Admin()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTags("Homepage");
			Driver.FindElement(By.CssSelector("#pageedit-button")).Click();
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();
			Driver.FindElement(By.CssSelector("#viewhistory a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .revert a")).Count, Is.EqualTo(1));
		}

		[Test]
		public void HistoryPage_Has_Revert_For_Editor()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");
			Driver.FindElement(By.CssSelector("#pageedit-button")).Click();
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();


			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();
			Driver.FindElement(By.CssSelector("#viewhistory a")).Click();
			

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .revert a")).Count, Is.EqualTo(1));
		}

		[Test]
		public void HistoryPage_Has_No_Revert_For_Anonymous()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");
			Driver.FindElement(By.CssSelector("#pageedit-button")).Click();
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();
			Logout();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector(".table td a")).Click();
			Driver.FindElement(By.CssSelector("#viewhistory a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .revert a")).Count, Is.EqualTo(0));
		}

		private void CreatePageWithTags(params string[] tags)
		{
			Driver.FindElement(By.CssSelector("a[href='/pages/new']")).Click();
			Driver.FindElement(By.Name("Title")).SendKeys("My title");
			//Driver.FindElement(By.Name("RawTags")).SendKeys("Tag1,Tag2");

			foreach (string tag in tags)
			{
				Driver.FindElement(By.Name("TagsEntry")).SendKeys(tag);
				Driver.FindElement(By.Name("TagsEntry")).SendKeys(Keys.Space);
			}

			Driver.FindElement(By.Name("Content")).SendKeys("Some content goes here");
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();
		}

		private void LoginAsAdmin()
		{
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
		}

		private void LoginAsEditor()
		{
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(EDITOR_PASSWORD);
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
		}
	}
}
