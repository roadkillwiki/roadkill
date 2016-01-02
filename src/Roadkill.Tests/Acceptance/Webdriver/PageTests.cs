using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class PageTests : AcceptanceTestBase
	{
		[Test]
		public void installation_page_should_not_display_for_home_page_when_installed_is_true()
		{
			// Arrange

			// Act
			Driver.Navigate().GoToUrl($"{BaseUrl}/Install");

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("div#installer-container")).Count, Is.EqualTo(0));
		}

		[Test]
		public void no_mainpage_set()
		{
			// Arrange + Act
			Driver.Navigate().GoToUrl(BaseUrl);
			
			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Is.EqualTo("You have no mainpage set"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Is.EqualTo("To set a main page, create a page and assign the tag 'homepage' to it."));
		}

		[Test]
		public void no_extra_menu_options_for_anonymous_users()
		{
			// Arrange
			Logout();

			// Act
			IEnumerable<IWebElement> leftmenuItems = Driver.FindElements(By.CssSelector("ul.nav li"));

			// Assert
			Assert.That(leftmenuItems.Count(), Is.EqualTo(3));
		}

		[Test]
		public void login_as_admin_shows_all_left_menu_options()
		{
			// Arrange
			LoginAsAdmin();

			// Act
			IEnumerable<IWebElement> leftmenuItems = Driver.FindElements(By.CssSelector("ul.nav li"));

			// Assert
			Assert.That(leftmenuItems.Count(), Is.EqualTo(6));
		}

		[Test]
		public void login_as_editor_shows_extra_menu_option()
		{
			// Arrange
			LoginAsEditor();

			// Act
			IEnumerable<IWebElement> leftmenuItems = Driver.FindElements(By.CssSelector("ul.nav li"));

			// Assert
			Assert.That(leftmenuItems.Count(), Is.EqualTo(5));
		}

		[Test]
		public void newpage_with_homepage_tag_shows_as_homepage()
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
		public void alltagspage_displays_correct_tags_for_all_users()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTags("Tag1-áéíóöü++", "Tag2-ÁÉÍÓÖÜÚ$"); //őű Ű

			// Act	
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.FindElement(By.CssSelector("a[href='/pages/alltags']")).Click();
			
			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#tagcloud a")).Count, Is.EqualTo(2));
			Assert.That(Driver.FindElements(By.CssSelector("#tagcloud a"))[0].Text, Is.EqualTo("Tag1-áéíóöü++"));
			Assert.That(Driver.FindElements(By.CssSelector("#tagcloud a"))[1].Text, Is.EqualTo("Tag2-ÁÉÍÓÖÜÚ$"));
		}

		[Test]
		public void editicon_exists_for_editors()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector("td.pagename a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#pageedit-button")).Count, Is.EqualTo(1));
		}

		[Test]
		public void editicon_exists_for_admins()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector("td.pagename a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#pageedit-button")).Count, Is.EqualTo(1));
		}

		[Test]
		public void properties_icon_exists_for_editors()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector("td.pagename a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#pageinfo-button")).Count, Is.EqualTo(1));

			Driver.FindElement(By.CssSelector("#pageinfo-button")).Click();

			Assert.That(Driver.FindElement(By.CssSelector("#pageinformation")).GetCssValue("display"), Is.EqualTo("block"));
		}

		[Test]
		public void properties_icon_exists_for_admin()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector("td.pagename a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#pageinfo-button")).Count, Is.EqualTo(1));

			Driver.FindElement(By.CssSelector("#pageinfo-button")).Click();

			Assert.That(Driver.FindElement(By.CssSelector("#pageinformation")).GetCssValue("display"), Is.EqualTo("block"));
		}

		[Test]
		public void view_history_link_exists_for_all_users()
		{
			// Arrange
			LoginAsEditor();
			CreatePageWithTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector("td.pagename a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector("#viewhistory")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElement(By.CssSelector("#viewhistory a")).Text, Is.EqualTo("View History"));
		}

		[Test]
		public void all_pages_has_edit_but_no_delete_button_for_editor()
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
		public void all_pages_has_edit_and_delete_button_for_admin()
		{
			// Arrange
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage");

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .edit a")).Count, Is.EqualTo(1));
			Assert.That(Driver.FindElements(By.CssSelector(".table .delete a")).Count, Is.EqualTo(1));
		}

		[Test]
		public void all_pages_has_no_buttons_for_anonymous()
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
		public void tagpage_has_edit_and_delete_button_for_admin()
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
		public void tagpage_has_edit_but_no_delete_button_for_editor()
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
		public void tagpage_has_no_buttons_for_anonymous()
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
		public void byuserpage_has_edit_and_delete_button_for_admin()
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
		public void byuserpage_has_edit_but_no_delete_button_for_editor()
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
		public void byuserpage_has_no_buttons_for_anonymous()
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

		[Test]
		public void historypage_has_revert_for_admin()
		{
			// Arrange
			LoginAsAdmin();
			Driver.Navigate().GoToUrl(BaseUrl + "/SiteSettings/tools/updatesearchindex");

			CreatePageWithTags("Homepage");
			Driver.FindElement(By.CssSelector("#pageedit-button")).Click();
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector("td.pagename a")).Click();
			Driver.FindElement(By.CssSelector("#viewhistory a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .revert a")).Count, Is.EqualTo(1));
		}

		[Test]
		public void historypage_has_revert_for_editor()
		{
			// Arrange
			LoginAsAdmin();
			Driver.Navigate().GoToUrl(BaseUrl + "/SiteSettings/tools/updatesearchindex");
			Logout();

			LoginAsEditor();
			CreatePageWithTags("Homepage");
			Driver.FindElement(By.CssSelector("#pageedit-button")).Click();
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();


			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector("td.pagename a")).Click();
			Driver.FindElement(By.CssSelector("#viewhistory a")).Click();
			

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .revert a")).Count, Is.EqualTo(1));
		}

		[Test]
		public void historypage_has_no_revert_for_anonymous()
		{
			// Arrange
			LoginAsAdmin();
			Driver.Navigate().GoToUrl(BaseUrl + "/SiteSettings/tools/updatesearchindex");
			Logout();

			LoginAsEditor();
			CreatePageWithTags("Homepage");
			Driver.FindElement(By.CssSelector("#pageedit-button")).Click();
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();
			Logout();

			// Act
			Driver.FindElement(By.CssSelector("a[href='/pages/allpages']")).Click();
			Driver.FindElement(By.CssSelector("td.pagename a")).Click();
			Driver.FindElement(By.CssSelector("#viewhistory a")).Click();

			// Assert
			Assert.That(Driver.FindElements(By.CssSelector(".table .revert a")).Count, Is.EqualTo(0));
		}
	}
}
