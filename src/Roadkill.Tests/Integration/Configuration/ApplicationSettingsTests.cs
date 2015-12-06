using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Moq;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Tests.Unit;
using Roadkill.Tests.Unit.StubsAndMocks.Mvc;

namespace Roadkill.Tests.Integration.Configuration
{
	[TestFixture]
	[Category("Unit")]
	public class ApplicationSettingsTests
	{
		[Test]
		public void Should_Have_Default_Values_Set_In_Constructor()
		{
			// Arrange + Act
			ApplicationSettings appSettings = new ApplicationSettings();

			// Assert
			Assert.That(appSettings.AppDataPath,               Is.EqualTo(GetFullPath(@"App_Data")), "AppDataPath");
			Assert.That(appSettings.AppDataInternalPath,       Is.EqualTo(GetFullPath(@"App_Data\Internal")), "AppDataInternalPath");
			Assert.That(appSettings.CustomTokensPath,          Is.EqualTo(GetFullPath(@"App_Data\customvariables.xml")), "CustomTokensPath");
			Assert.That(appSettings.EmailTemplateFolder,       Is.EqualTo(GetFullPath(@"App_Data\EmailTemplates")), "EmailTemplateFolder");
			Assert.That(appSettings.HtmlElementWhiteListPath,  Is.EqualTo(GetFullPath(@"App_Data\Internal\htmlwhitelist.xml")), "HtmlElementWhiteListPath");
			Assert.That(appSettings.SearchIndexPath,           Is.EqualTo(GetFullPath(@"App_Data\Internal\Search")), "SearchIndexPath");
			Assert.That(appSettings.SQLiteBinariesPath,        Is.EqualTo(GetFullPath(@"App_Data\Internal\SQLiteBinaries")), "SQLiteBinariesPath");
			Assert.That(appSettings.PluginsBinPath,            Is.EqualTo(GetFullPath(@"bin\Plugins")), "PluginsBinPath");
			Assert.That(appSettings.PluginsPath,               Is.EqualTo(GetFullPath(@"Plugins")), "PluginsPath");

			Assert.That(appSettings.MinimumPasswordLength, Is.EqualTo(6), "MinimumPasswordLength");
			Assert.That(appSettings.DatabaseName, Is.EqualTo(RepositoryFactory.SqlServer2008), "DataStoreType");
			Assert.That(appSettings.AttachmentsRoutePath, Is.EqualTo("Attachments"), "AttachmentsRoutePath");
			Assert.That(appSettings.AttachmentsFolder, Is.EqualTo("~/App_Data/Attachments"), "AttachmentsFolder");	
		}

		private string GetFullPath(string path)
		{
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			return Path.Combine(baseDir, path);
		}

		[Test]
		public void AttachmentsDirectoryPath_Should_Map_AttachmentsFolder_And_End_With_Slash()
		{
			// Arrange
			string attachmentsFolder = @"~/myfolder";

			MvcMockContainer container = new MvcMockContainer();
			HttpContextBase httpContext = MvcMockHelpers.FakeHttpContext(container);
			container.ServerUtility.Setup(x => x.MapPath(attachmentsFolder)).Returns(@"c:\inetpub\myfolder");

			ApplicationSettings appSettings = new ApplicationSettings(httpContext);
			appSettings.AttachmentsFolder = attachmentsFolder;

			// Act
			string actualPath = appSettings.AttachmentsDirectoryPath;

			// Assert
			Assert.That(actualPath, Is.EqualTo(@"c:\inetpub\myfolder\"));
		}

		[Test]
		public void AttachmentsRoutePath_Should_Use_AttachmentsRoutePath_And_Prepend_ApplicationPath()
		{
			// Arrange
			MvcMockContainer container = new MvcMockContainer();
			HttpContextBase httpContext = MvcMockHelpers.FakeHttpContext(container);
			container.Request.Setup(x => x.ApplicationPath).Returns("/wiki");

			ApplicationSettings appSettings = new ApplicationSettings(httpContext);
			appSettings.AttachmentsRoutePath = "Folder1/Folder2";

			// Act
			string actualUrlPath = appSettings.AttachmentsUrlPath;
			
			// Assert
			Assert.That(actualUrlPath, Is.EqualTo(@"/wiki/Folder1/Folder2"));
		}
	}
}
