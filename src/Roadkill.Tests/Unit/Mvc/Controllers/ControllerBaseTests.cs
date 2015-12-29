using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;
using Roadkill.Tests.Unit.StubsAndMocks.Mvc;

namespace Roadkill.Tests.Unit.Mvc.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class ControllerBaseTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private UserServiceMock _userService;
		private SettingsService _settingsService;

		private ConfigReaderWriterStub _configReaderWriter;

		private ControllerBaseStub _controller;
		private DatabaseTesterMock _databaseTester;
		private InstallationService _installationService;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;

			_controller = new ControllerBaseStub(_applicationSettings, _userService, _context, _settingsService);
			_controller.SetFakeControllerContext("~/");

			// InstallController
			_configReaderWriter = new ConfigReaderWriterStub();
			_databaseTester = _container.DatabaseTester;
			_installationService = _container.InstallationService;
		}

		[Test]
		public void should_redirect_when_installed_is_false()
		{
			// Arrange
			_applicationSettings.Installed = false;
			ActionExecutingContext filterContext = new ActionExecutingContext();
			filterContext.Controller = _controller;

			// Act
			_controller.CallOnActionExecuting(filterContext);
			RedirectResult result = filterContext.Result as RedirectResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectResult");
			Assert.That(result.Url, Is.EqualTo("/install"));
		}

		[Test]
		public void should_not_redirect_when_installed_is_false_and_controller_is_installercontroller()
		{
			// Arrange
			_applicationSettings.Installed = false;
			InstallControllerStub installController = new InstallControllerStub(_applicationSettings, _configReaderWriter, _installationService, _databaseTester);
			ActionExecutingContext filterContext = new ActionExecutingContext();
			filterContext.Controller = installController;

			// Act
			installController.CallOnActionExecuting(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Null);
		}

		[Test]
		public void should_set_loggedin_user_and_viewbag_data()
		{
			// Arrange
			_applicationSettings.Installed = true;
			_userService.LoggedInUserId = "mrblah";

			ActionExecutingContext filterContext = new ActionExecutingContext();
			filterContext.Controller = _controller;

			// Act
			_controller.CallOnActionExecuting(filterContext);

			// Assert
			Assert.That(_context.CurrentUsername, Is.EqualTo("mrblah"));
			Assert.That(_controller.ViewBag.Context, Is.EqualTo(_context));
			Assert.That(_controller.ViewBag.Config, Is.EqualTo(_applicationSettings));
		}
	}


	//
	// These stubs let the tests call OnActionExecuting (without resorting to a tangle of Moq setups)
	//

	public class ControllerBaseStub : Roadkill.Core.Mvc.Controllers.ControllerBase
	{
		public ControllerBaseStub(ApplicationSettings settings, UserServiceBase userService, IUserContext context, 
			SettingsService settingsService) : base(settings, userService, context, settingsService)
		{

		}

		public void CallOnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
		}
	}

	internal class InstallControllerStub : InstallController
	{
		public InstallControllerStub(ApplicationSettings settings, ConfigReaderWriter configReaderWriter, IInstallationService installationService, IDatabaseTester databaseTester)
			: base(settings, configReaderWriter, installationService, databaseTester)
		{

		}

		public void CallOnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
		}
	}
}