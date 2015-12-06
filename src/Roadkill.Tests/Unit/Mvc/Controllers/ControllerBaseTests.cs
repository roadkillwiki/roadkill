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

		private RepositoryMock _repository;
		private PageService _pageService;
		private PageHistoryService _historyService;
		private PluginFactoryMock _pluginFactory;
		private SearchServiceMock _searchService;
		private ConfigReaderWriterStub _configReaderWriter;

		private ControllerBaseStub _controller;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;

			_controller = new ControllerBaseStub(_applicationSettings, _userService, _context, _settingsService);
			MvcMockContainer container = _controller.SetFakeControllerContext("~/");

			// Used by InstallController
			_repository = _container.Repository;
			_pluginFactory = _container.PluginFactory;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;
			_searchService = _container.SearchService;
			_configReaderWriter = new ConfigReaderWriterStub();	
		}

		[Test]
		public void Should_Redirect_When_Installed_Is_False()
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
		public void Should_Not_Redirect_When_Installed_Is_False_And_Controller_Is_InstallerController()
		{
			// Arrange
			_applicationSettings.Installed = false;
			InstallControllerStub installController = new InstallControllerStub(_applicationSettings, _configReaderWriter, new RepositoryFactoryMock()); // use a concrete implementation
			 
			ActionExecutingContext filterContext = new ActionExecutingContext();
			filterContext.Controller = installController;

			// Act
			installController.CallOnActionExecuting(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Null);
		}

		[Test]
		public void Should_Set_LoggedIn_User_And_ViewBag_Data()
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
		public InstallControllerStub(ApplicationSettings settings, ConfigReaderWriter configReaderWriter, IRepositoryFactory repositoryFactory)
			: base(settings, configReaderWriter, repositoryFactory)
		{

		}

		public void CallOnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
		}
	}
}