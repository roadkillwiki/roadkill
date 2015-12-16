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
		public void configuration_page_shows_all_settings()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();

			// Assert
			Assert.That(Driver.ElementValue("#SiteName"), Is.EqualTo("Acceptance Tests"));
			Assert.That(Driver.ElementValue("#SiteUrl"), Is.EqualTo(TestConstants.WEB_BASEURL));
			Assert.That(Driver.ElementValue("#ConnectionString"), Is.StringStarting(TestConstants.CONNECTION_STRING));
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

			Assert.That(Driver.FindElements(By.CssSelector("#DatabaseName option")).Count, Is.EqualTo(4));
			SelectElement element = new SelectElement(Driver.FindElement(By.CssSelector("#DatabaseName")));
			Assert.That(element.SelectedOption.GetAttribute("value"), Is.EqualTo("SqlServer2008"));
			Assert.That(Driver.SelectedIndex("#MarkupType"), Is.EqualTo(0));
			Assert.That(Driver.SelectedIndex("#Theme"), Is.EqualTo(3));
			Assert.False(Driver.IsCheckboxChecked("OverwriteExistingFiles"));
			Assert.That(Driver.ElementValue("#HeadContent"), Is.EqualTo(""));
			Assert.That(Driver.ElementValue("#MenuMarkup"), Is.EqualTo("* %mainpage%\r\n* %categories%\r\n* %allpages%\r\n* %newpage%\r\n* %managefiles%\r\n* %sitesettings%\r\n\r\n"));
		}

		[Test]
		public void users_page_shows_all_admin_users_and_buttons_and_no_delete_for_current_user()
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
		public void users_page_shows_all_editor_users_and_both_buttons()
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
		public void edit_user_on_users_page_shows_correctdetails()
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
		public void edit_user_on_users_page_saves_user()
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
		public void can_change_password_on_users_page()
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
		public void add_admin_on_users_page_shows_in_table_of_admins_with_both_buttons()
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
		public void add_editor_on_users_page_shows_in_table_of_editor_with_both_buttons()
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
		public void delete_user_on_users_page_removes_from_user_table()
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
		public void delete_user_on_users_page_requires_confirm_click()
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
