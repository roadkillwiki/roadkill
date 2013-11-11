using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Integration.Configuration
{
	[TestFixture]
	[Category("Unit")]
	public class ApplicationSettingsTests
	{
		[Test]
		public void Should_Have_Default_Values_Set_In_Constructor()
		{
			// Arrange
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;

			// Act
			ApplicationSettings appSettings = new ApplicationSettings();

			// Assert
			Assert.That(appSettings.AppDataPath, Is.EqualTo(baseDir +@"\App_Data"), "AppDataPath");
			Assert.That(appSettings.AppDataInternalPath, Is.EqualTo(baseDir + @"\App_Data\Internal"), "AppDataInternalPath");
			Assert.That(appSettings.CustomTokensPath, Is.EqualTo(baseDir + @"\App_Data\customvariables.xml"), "CustomTokensPath");
			Assert.That(appSettings.EmailTemplateFolder, Is.EqualTo(baseDir + @"\App_Data\EmailTemplates"), "EmailTemplateFolder");
			Assert.That(appSettings.HtmlElementWhiteListPath, Is.EqualTo(baseDir + @"\App_Data\Internal\htmlwhitelist.xml"), "HtmlElementWhiteListPath");
			Assert.That(appSettings.MinimumPasswordLength, Is.EqualTo(6), "MinimumPasswordLength");
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.SqlServer2008), "DataStoreType");
			Assert.That(appSettings.AttachmentsRoutePath, Is.EqualTo("Attachments"), "AttachmentsRoutePath");
			Assert.That(appSettings.AttachmentsFolder, Is.EqualTo("~/App_Data/Attachments"), "AttachmentsFolder");
			Assert.That(appSettings.SearchIndexPath, Is.EqualTo(baseDir + @"\App_Data\Internal\Search"), "SearchIndexPath");
			Assert.That(appSettings.SQLiteBinariesPath, Is.EqualTo(baseDir + @"\App_Data\Internal\SQLiteBinaries"), "SQLiteBinariesPath");
			Assert.That(appSettings.SpecialPagePluginsPath, Is.EqualTo(baseDir + @"\Plugins\SpecialPages"), "SpecialPagePluginsPath");
			Assert.That(appSettings.SpecialPagePluginsBinPath, Is.EqualTo(baseDir + @"\bin\Plugins\SpecialPages"), "SpecialPagePluginsBinPath");
			Assert.That(appSettings.TextPluginsPath, Is.EqualTo(baseDir + @"\Plugins\Text"), "TextPluginsPath");
			Assert.That(appSettings.TextPluginsBinPath, Is.EqualTo(baseDir + @"\bin\Plugins\Text"), "TextPluginsBinPath");
			Assert.That(appSettings.UserServicePluginsPath, Is.EqualTo(baseDir + @"\Plugins\UserService"), "UserServicePluginsPath");
		}

		[Test]
		public void Attachment_Paths_Should()
		{
			// AttachmentsFolder
			// AttachmentsDirectoryPath
			// AttachmentsUrlPath
			// AttachmentsRoutePath

			// RandomPage plugin

			// Arrange
			ApplicationSettings appSettings = new ApplicationSettings();

			// Act


			// Assert
		}
	}
}
