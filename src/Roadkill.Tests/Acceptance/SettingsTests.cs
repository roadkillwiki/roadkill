using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Roadkill.Core;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class SettingsTests : AcceptanceTestBase
	{
		[Test]
		public void Configuration_Page_Shows_All_Settings()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();

			// Assert
			Assert.That(Driver.ElementValue("#SiteName"), Is.EqualTo("Acceptance Tests"));
			Assert.That(Driver.ElementValue("#SiteUrl"), Is.EqualTo("http://localhost:9876"));
			Assert.That(Driver.ElementValue("#ConnectionString"), Is.StringStarting(SqlExpressSetup.ConnectionString));
			Assert.That(Driver.ElementValue("#RecaptchaPrivateKey"), Is.EqualTo("recaptcha-private-key"));
			Assert.That(Driver.ElementValue("#RecaptchaPublicKey"), Is.EqualTo("recaptcha-public-key"));
			Assert.That(Driver.ElementValue("#EditorRoleName"), Is.EqualTo("Editor"));
			Assert.That(Driver.ElementValue("#AdminRoleName"), Is.EqualTo("Admin"));
			Assert.That(Driver.ElementValue("#AttachmentsFolder"), Is.EqualTo(@"~/App_Data/Attachments"));
			Assert.That(Driver.ElementValue("#AllowedFileTypes"), Is.EqualTo("jpg,png,gif,zip,xml,pdf"));

			Assert.False(Driver.IsCheckboxChecked("UseWindowsAuth"));
			Assert.True(Driver.IsCheckboxChecked("AllowUserSignup"));
			Assert.False(Driver.IsCheckboxChecked("IsRecaptchaEnabled"));
			Assert.False(Driver.IsCheckboxChecked("UseObjectCache"));
			Assert.False(Driver.IsCheckboxChecked("UseBrowserCache"));

			Assert.That(Driver.FindElements(By.CssSelector("#DataStoreTypeName option")).Count, Is.EqualTo(DataStoreType.AllTypes.Count()));
			SelectElement element = new SelectElement(Driver.FindElement(By.CssSelector("#DataStoreTypeName")));
			Assert.That(element.SelectedOption.GetAttribute("value"), Is.EqualTo(DataStoreType.ByName("SqlServer2012").Name));
			Assert.That(Driver.SelectedIndex("#MarkupType"), Is.EqualTo(0));
			Assert.That(Driver.SelectedIndex("#Theme"), Is.EqualTo(3));
			Assert.False(Driver.IsCheckboxChecked("OverwriteExistingFiles"));
			Assert.That(Driver.ElementValue("#HeadContent"), Is.EqualTo(""));
			Assert.That(Driver.ElementValue("#MenuMarkup"), Is.EqualTo("* %mainpage%\r\n* %categories%\r\n* %allpages%\r\n* %newpage%\r\n* %managefiles%\r\n* %sitesettings%\r\n\r\n"));
		}

		[Test]
		public void Users_Page_Shows_All_Admin_Users_And_Buttons_And_No_Delete_For_Current_User()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#admins-table tbody tr")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElement(By.CssSelector("#admins-table tbody tr>td")).Text, Is.EqualTo("admin"));
			Assert.That(Driver.FindElement(By.CssSelector("#admins-table tbody tr>td+td")).Text, Is.EqualTo("admin@localhost"));

			Assert.That(Driver.FindElement(By.CssSelector("#admins-table tbody tr>td+td+td a")).Text, Is.EqualTo("Edit"));
			Assert.That(Driver.FindElements(By.CssSelector("#admins-table tbody tr>td+td+td+td a")).Count, Is.EqualTo(0));
		}

		[Test]
		public void Users_Page_Shows_All_Editor_Users_And_Both_Buttons()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#editors-table tbody tr")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElement(By.CssSelector("#editors-table tbody tr>td")).Text, Is.EqualTo("editor"));
			Assert.That(Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td")).Text, Is.EqualTo("editor@localhost"));

			Assert.That(Driver.FindElements(By.CssSelector("#editors-table tbody tr>td")).Count, Is.EqualTo(4));
			Assert.That(Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td+td a")).Text, Is.EqualTo("Edit"));
			Assert.That(Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td+td+td a")).Text, Is.EqualTo("Delete"));
		}

		[Test]
		public void Edit_User_On_Users_Page_Shows_CorrectDetails()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement']")).Click();
			Driver.FindElement(By.CssSelector("#admins-table tbody tr>td+td+td a")).Click();

			// Assert
			Assert.That(Driver.ElementValue("#NewUsername"), Is.EqualTo("admin"));
			Assert.That(Driver.ElementValue("#NewEmail"), Is.EqualTo("admin@localhost"));
			Assert.That(Driver.ElementValue("#Password"), Is.EqualTo(""));
			Assert.That(Driver.ElementValue("#PasswordConfirmation"), Is.EqualTo(""));
		}

		[Test]
		public void Edit_User_On_Users_Page_Saves_User()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement']")).Click();
			Driver.FindElement(By.CssSelector("#admins-table tbody tr>td+td+td a")).Click();
			Driver.FindElement(By.CssSelector("#NewUsername")).Clear();
			Driver.FindElement(By.CssSelector("#NewUsername")).SendKeys("admin2");
			Driver.FindElement(By.CssSelector("#NewEmail")).Clear();
			Driver.FindElement(By.CssSelector("#NewEmail")).SendKeys("admin2@localhost");
			Driver.FindElement(By.CssSelector("input[value='Save']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#admins-table tbody tr")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElement(By.CssSelector("#admins-table tbody tr>td")).Text, Is.EqualTo("admin2"));
			Assert.That(Driver.FindElement(By.CssSelector("#admins-table tbody tr>td+td")).Text, Is.EqualTo("admin2@localhost"));
			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas")).Text, Contains.Substring("Logged in as admin2"));
		}

		[Test]
		public void Can_Change_Password_On_Users_Page()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement']")).Click();
			Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td+td a")).Click();
			Driver.FindElement(By.CssSelector("#Password")).Clear();
			Driver.FindElement(By.CssSelector("#Password")).SendKeys("newpassword");
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).Clear();
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).SendKeys("newpassword");
			Driver.FindElement(By.CssSelector("input[value='Save']")).Click();

			Logout();

			// Assert
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(EDITOR_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys("newpassword");
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();

			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas")).Text, Contains.Substring("Logged in as editor"));
		}

		[Test]
		public void Add_Admin_On_Users_Page_Shows_In_Table_Of_Admins_With_Both_Buttons()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement/AddAdmin']")).Click();
			Driver.FindElement(By.CssSelector("#NewUsername")).Clear();
			Driver.FindElement(By.CssSelector("#NewUsername")).SendKeys("anotheradmin");
			Driver.FindElement(By.CssSelector("#NewEmail")).Clear();
			Driver.FindElement(By.CssSelector("#NewEmail")).SendKeys("anotheradmin@localhost");
			Driver.FindElement(By.CssSelector("#Password")).Clear();
			Driver.FindElement(By.CssSelector("#Password")).SendKeys("password");
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).Clear();
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).SendKeys("password");
			Driver.FindElement(By.CssSelector("input[value='Save']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#admins-table tbody tr")).Count, Is.EqualTo(2));
			Assert.That(Driver.FindElement(By.CssSelector("#admins-table tbody tr+tr>td")).Text, Is.EqualTo("anotheradmin"));
			Assert.That(Driver.FindElement(By.CssSelector("#admins-table tbody tr+tr>td+td")).Text, Is.EqualTo("anotheradmin@localhost"));
			Assert.That(Driver.FindElement(By.CssSelector("#admins-table tbody tr+tr>td+td+td a")).Text, Is.EqualTo("Edit"));
			Assert.That(Driver.FindElement(By.CssSelector("#admins-table tbody tr+tr>td+td+td+td a")).Text, Is.EqualTo("Delete"));
		}

		[Test]
		public void Add_Editor_On_Users_Page_Shows_In_Table_Of_Editor_With_Both_Buttons()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement/AddEditor']")).Click();
			Driver.FindElement(By.CssSelector("#NewUsername")).Clear();
			Driver.FindElement(By.CssSelector("#NewUsername")).SendKeys("anothereditor");
			Driver.FindElement(By.CssSelector("#NewEmail")).Clear();
			Driver.FindElement(By.CssSelector("#NewEmail")).SendKeys("anothereditor@localhost");
			Driver.FindElement(By.CssSelector("#Password")).Clear();
			Driver.FindElement(By.CssSelector("#Password")).SendKeys("password");
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).Clear();
			Driver.FindElement(By.CssSelector("#PasswordConfirmation")).SendKeys("password");
			Driver.FindElement(By.CssSelector("input[value='Save']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#editors-table tbody tr")).Count, Is.EqualTo(2));
			Assert.That(Driver.FindElement(By.CssSelector("#editors-table tbody tr>td")).Text, Is.EqualTo("anothereditor"));
			Assert.That(Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td")).Text, Is.EqualTo("anothereditor@localhost"));
			Assert.That(Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td+td a")).Text, Is.EqualTo("Edit"));
			Assert.That(Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td+td+td a")).Text, Is.EqualTo("Delete"));
		}

		[Test]
		public void Delete_User_On_Users_Page_Removes_From_User_Table()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement']")).Click();
			Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td+td+td a")).Click();
			Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td+td+td a")).Click(); // confirm

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#editors-table tbody tr")).Count, Is.EqualTo(0));
		}

		[Test]
		public void Delete_User_On_Users_Page_Requires_Confirm_Click()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("a[href='/SiteSettings/UserManagement']")).Click();
			Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td+td+td a")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#editors-table tbody tr>td+td+td+td a")).Text, Is.EqualTo("Confirm"));
			Assert.That(Driver.FindElements(By.CssSelector("#editors-table tbody tr")).Count, Is.EqualTo(1));
		}
	}
}
