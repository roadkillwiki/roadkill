using System.Configuration;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using Roadkill.Core;
using System.Linq;
using OpenQA.Selenium.Support.UI;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class UpgradeToVersion16Tests : AcceptanceTestBase
	{
		[SetUp]
		public void Setup()
		{
			// Copy the legacy database
			string sitePath = AcceptanceTestsSetup.GetSitePath();
			string version16DbFileSource = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "roadkill-pre16-acceptancetests.sdf");
			string version16DbFileDest = string.Format("{0}/App_Data/roadkill-pre16-acceptancetests.sdf", sitePath);
			System.IO.File.Copy(version16DbFileSource, version16DbFileDest, true);

			// Update the connection string
			string webConfigPath = Path.Combine(sitePath, "web.config");
			ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
			fileMap.ExeConfigFilename = webConfigPath;
			System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
			config.ConnectionStrings.ConnectionStrings["Roadkill"].ConnectionString = @"Data Source=|DataDirectory|\roadkill-pre16-acceptancetests.sdf;";
			config.Save();
		}

		[TearDown]
		public void TearDown()
		{
			AcceptanceTestsSetup.CopyWebConfig();
		}

		[Test]
		[Ignore("Needs Schema.Upgrade implementations")]
		public void Version15_Can_Login_As_Admin_And_Settings_Page_Contains_Settings()
		{
			// Arrange
			Driver.Navigate().GoToUrl(LogoutUrl);
			Driver.Navigate().GoToUrl(LoginUrl);
			Driver.FindElement(By.Name("email")).SendKeys(ADMIN_EMAIL);
			Driver.FindElement(By.Name("password")).SendKeys(ADMIN_PASSWORD);

			// Act
			Driver.FindElement(By.CssSelector("input[value=Login]")).Click();
			Driver.FindElement(By.CssSelector("a[href='/settings']")).Click();
			Driver.FindElement(By.CssSelector("input[value=Save]")).Click();

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector("#loggedinas a")).Text, Is.EqualTo("Logged in as admin"));

			Assert.That(Driver.ElementValue("#SiteName"), Is.EqualTo("Acceptance Tests"));
			Assert.That(Driver.ElementValue("#SiteUrl"), Is.EqualTo("http://localhost:57025"));
			Assert.That(Driver.ElementValue("#ConnectionString"), Is.StringStarting(@"Data Source=|DataDirectory|\roadkill-pre16-acceptancetests.sdf"));
			Assert.That(Driver.ElementValue("#RecaptchaPrivateKey"), Is.EqualTo("Recaptcha Private Key"));
			Assert.That(Driver.ElementValue("#RecaptchaPublicKey"), Is.EqualTo("Recaptcha Public Key"));
			Assert.That(Driver.ElementValue("#EditorRoleName"), Is.EqualTo("Editor"));
			Assert.That(Driver.ElementValue("#AdminRoleName"), Is.EqualTo("Admin"));
			Assert.That(Driver.ElementValue("#AttachmentsFolder"), Is.EqualTo(@"C:\temp\roadkillattachments"));
			Assert.That(Driver.ElementValue("#AllowedExtensions"), Is.EqualTo("jpg,png,gif,zip,xml,pdf"));

			Assert.False(Driver.IsCheckboxChecked("UseWindowsAuth"));
			Assert.True(Driver.IsCheckboxChecked("AllowUserSignup"));
			Assert.False(Driver.IsCheckboxChecked("EnableRecaptcha"));
			Assert.True(Driver.IsCheckboxChecked("UseObjectCache"));
			Assert.True(Driver.IsCheckboxChecked("UseBrowserCache"));

			DataStoreType sqlCeType = DataStoreType.ByName("SqlServerCe");
			Assert.That(Driver.FindElements(By.CssSelector("#DataStoreTypeName option")).Count, Is.EqualTo(DataStoreType.AllTypes.Count()));

			SelectElement element = new SelectElement(Driver.FindElement(By.CssSelector("#DataStoreTypeName")));
			Assert.That(element.SelectedOption.GetAttribute("value"), Is.EqualTo(DataStoreType.ByName("SqlServerCe").Name));
			Assert.That(Driver.SelectedIndex("#MarkupType"), Is.EqualTo(0));
			Assert.That(Driver.SelectedIndex("#Theme"), Is.EqualTo(1));
		}
	}
}
