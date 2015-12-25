using System;
using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class ConfigurationTesterControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private UserServiceMock _userService;
		private ConfigReaderWriterStub _configReaderWriter;
		private ActiveDirectoryProviderMock _activeDirectoryProviderMock;

		private ConfigurationTesterController _configTesterController;
		private DatabaseTesterMock _databaseTester;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.Installed = false;

			_context = _container.UserContext;
			_userService = _container.UserService;
			_configReaderWriter = new ConfigReaderWriterStub();
			_activeDirectoryProviderMock = new ActiveDirectoryProviderMock();

			_databaseTester = _container.DatabaseTester;

			_configTesterController = new ConfigurationTesterController(_applicationSettings, _context, _configReaderWriter, _activeDirectoryProviderMock, _userService, _databaseTester);
		}

		[Test]
		public void testwebconfig_should_return_jsonresult_and_testresult_model_without_errors()
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
		public void testwebconfig_should_return_empty_content_when_installed_is_true()
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
		public void testattachments_should_return_empty_content_when_installed_is_true()
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
		public void testdatabaseconnection_should_return_empty_content_when_installed_is_true()
		{
			// Arrange
			_applicationSettings.Installed = true;

			// Act
			ActionResult result = _configTesterController.TestDatabaseConnection("connectionstring", "SqlServer2008");

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult, Is.Not.Null);
			Assert.That(contentResult.Content, Is.Empty);
		}

		[Test]
		public void testldap_should_return_jsonresult_and_testresult_model_without_errors()
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
		public void testldap_should_return_empty_content_when_installed_is_true()
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
		public void testattachments_should_allow_get_and_return_json_result_and_testresult_with_no_errors()
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
		public void testattachments_should_return_testresult_with_errors_for_unwritable_folder()
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
		public void testdatabaseconnection_should_allow_get_and_return_json_result_and_testresult_with_no_errors()
		{
			// Arrange
			string connectionString = "Server=(local);Integrated Security=true;Connect Timeout=5;database=Roadkill";
			_databaseTester.IsConnectionValid = true;

			// Act
			JsonResult result = _configTesterController.TestDatabaseConnection(connectionString, "SqlServer2008") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.AllowGet));

			TestResult data = result.Data as TestResult;
			Assert.That(data, Is.Not.Null);
			Assert.That(data.Success, Is.True, data.ErrorMessage);
			Assert.That(data.ErrorMessage, Is.Null.Or.Empty);
		}

		[Test]
		public void testdatabaseconnection_should_return_testresult_with_errors_for_invalid_connectionstring()
		{
			// Arrange
			string connectionString = "invalid connection string";
			_databaseTester.IsConnectionValid = false;

			// Act
			JsonResult result = _configTesterController.TestDatabaseConnection(connectionString, "SqlServer2008") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");

			TestResult data = result.Data as TestResult;
			Assert.That(data, Is.Not.Null);
			Assert.That(data.Success, Is.False, data.ErrorMessage);
			Assert.That(data.ErrorMessage, Is.Not.Null.Or.Empty);
		}
	}
}