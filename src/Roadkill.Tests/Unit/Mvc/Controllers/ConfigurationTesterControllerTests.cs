using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Localization;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using System.Runtime.Caching;
using System.Threading;
using Roadkill.Tests.Unit.StubsAndMocks;
using MvcContrib.TestHelper;
using Roadkill.Core.DI;
using StructureMap;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class ConfigurationTesterControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private UserServiceMock _userService;
		private SettingsService _settingsService;
		private ConfigReaderWriterStub _configReaderWriter;
		private ActiveDirectoryProviderMock _activeDirectoryProviderMock;

		private ConfigurationTesterController _configTesterController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.Installed = false;

			_context = _container.UserContext;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_configReaderWriter = new ConfigReaderWriterStub();
			_activeDirectoryProviderMock = new ActiveDirectoryProviderMock();

			_configTesterController = new ConfigurationTesterController(_applicationSettings, _context, _configReaderWriter, _activeDirectoryProviderMock);
		}

		[Test]
		public void TestWebConfig_Should_Return_JsonResult_And_TestResult_Model_Without_Errors()
		{
			// Arrange
			_configReaderWriter.TestWebConfigResult = "";

			// Act
			ActionResult result = _configTesterController.TestWebConfig();

			// Assert
			JsonResult jsonResult = result.AssertResultIs<JsonResult>();

			TestResult testResult = jsonResult.Data as TestResult;
			Assert.That(testResult, Is.Not.Null);
			Assert.That(testResult.ErrorMessage, Is.EqualTo(""));
			Assert.That(testResult.Success, Is.True);
		}

		[Test]
		public void TestWebConfig_Should_Return_Empty_Content_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _configTesterController.TestWebConfig();

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult, Is.Not.Null);
			Assert.That(contentResult.Content, Is.Empty);
		}

		[Test]
		public void TestAttachments_Should_Return_Empty_Content_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _configTesterController.TestAttachments(AppDomain.CurrentDomain.BaseDirectory);

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult, Is.Not.Null);
			Assert.That(contentResult.Content, Is.Empty);
		}

		[Test]
		public void TestDatabaseConnection_Should_Return_Empty_Content_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _configTesterController.TestDatabaseConnection("connectionstring", "sqlite");

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult, Is.Not.Null);
			Assert.That(contentResult.Content, Is.Empty);
		}

		[Test]
		public void TestLdap_Should_Return_JsonResult_And_TestResult_Model_Without_Errors()
		{
			// Arrange
			_activeDirectoryProviderMock.LdapConnectionResult = "";

			// Act
			ActionResult result = _configTesterController.TestLdap("connectionstring", "username", "password", "groupname");

			// Assert
			JsonResult jsonResult = result.AssertResultIs<JsonResult>();

			TestResult testResult = jsonResult.Data as TestResult;
			Assert.That(testResult, Is.Not.Null);
			Assert.That(testResult.ErrorMessage, Is.EqualTo(""));
			Assert.That(testResult.Success, Is.True);
		}

		[Test]
		public void TestLdap_Should_Return_Empty_Content_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _configTesterController.TestLdap("connectionstring", "username", "password", "groupname");

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult, Is.Not.Null);
			Assert.That(contentResult.Content, Is.Empty);
		}

		[Test]
		public void CopySqlite_Should_Return_Empty_Content_When_Installed_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _configTesterController.CopySqlite();

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult, Is.Not.Null);
			Assert.That(contentResult.Content, Is.Empty);
		}

		[Test]
		public void CopySqlite_Should_Return_JsonResult_And_TestResult_Model_Without_Errors()
		{
			// Arrange
			_activeDirectoryProviderMock.LdapConnectionResult = "";
			_applicationSettings.SQLiteBinariesPath = Path.Combine(Settings.WEB_PATH, "App_Data", "Internal", "SQLiteBinaries");
			_configTesterController.SetFakeControllerContext();

			// Act
			ActionResult result = _configTesterController.CopySqlite();

			// Assert
			JsonResult jsonResult = result.AssertResultIs<JsonResult>();

			TestResult testResult = jsonResult.Data as TestResult;
			Assert.That(testResult, Is.Not.Null);
			Assert.That(testResult.ErrorMessage, Is.EqualTo(""), testResult.ErrorMessage);
			Assert.That(testResult.Success, Is.True);
		}


		[Test]
		public void TestAttachments_Should_Allow_Get_And_Return_Json_Result_And_TestResult_With_No_Errors()
		{
			// Arrange
			string directory = AppDomain.CurrentDomain.BaseDirectory;

			// Act
			JsonResult result = _configTesterController.TestAttachments(directory) as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.AllowGet));

			TestResult data = result.Data as TestResult;
			Assert.That(data, Is.Not.Null);
			Assert.That(data.Success, Is.True);
			Assert.That(data.ErrorMessage, Is.Null.Or.Empty);
		}

		[Test]
		public void TestAttachments_Should_Return_TestResult_With_Errors_For_UnWritable_Folder()
		{
			// Arrange
			string directory = "c:\ads8ads9f8d7asf98ad7f";

			// Act
			JsonResult result = _configTesterController.TestAttachments(directory) as JsonResult;

			// Assert
			TestResult data = result.Data as TestResult;
			Assert.That(data.ErrorMessage, Is.Not.Null);
			Assert.That(data.Success, Is.False);
		}

		[Test]
		public void TestDatabaseConnection_Should_Allow_Get_And_Return_Json_Result_And_TestResult_With_No_Errors()
		{
			// Arrange
			string sqlCeDbPath = Path.Combine(Settings.LIB_FOLDER, "Empty-databases", "roadkill.sdf");
			string sqlCeDbDestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testdatabase.sdf");
			File.Copy(sqlCeDbPath, sqlCeDbDestPath, true);

			string connectionString = @"Data Source=|DataDirectory|\testdatabase.sdf";
			DependencyManager manager = new DependencyManager(new ApplicationSettings());
			manager.Configure();

			// Act
			JsonResult result = _configTesterController.TestDatabaseConnection(connectionString, "SqlServerCE") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.AllowGet));

			TestResult data = result.Data as TestResult;
			Assert.That(data, Is.Not.Null);
			Assert.That(data.Success, Is.True, data.ErrorMessage);
			Assert.That(data.ErrorMessage, Is.Null.Or.Empty);
		}

		[Test]
		public void TestDatabaseConnection_Should_Return_TestResult_With_Errors_For_Invalid_ConnectionString()
		{
			// Arrange
			string connectionString = "invalid connection string";
			DependencyManager manager = new DependencyManager(new ApplicationSettings());
			manager.Configure();

			// Act
			JsonResult result = _configTesterController.TestDatabaseConnection(connectionString, "SqlServerCE") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");

			TestResult data = result.Data as TestResult;
			Assert.That(data, Is.Not.Null);
			Assert.That(data.Success, Is.False, data.ErrorMessage);
			Assert.That(data.ErrorMessage, Is.Not.Null.Or.Empty);
		}
	}
}