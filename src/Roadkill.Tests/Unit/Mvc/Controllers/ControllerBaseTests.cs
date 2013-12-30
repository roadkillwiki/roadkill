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
using ControllerBase = Roadkill.Core.Mvc.Controllers.ControllerBase;
using System.Web.Routing;
using Roadkill.Core.Mvc;
using Roadkill.Core.Security.Windows;

namespace Roadkill.Tests.Unit
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
			InstallControllerStub installController = new InstallControllerStub(_applicationSettings, _userService, _pageService, 
																				_searchService, _repository, _settingsService, _context, 
																				_configReaderWriter); // use a concrete implementation
			 
			ActionExecutingContext filterContext = new ActionExecutingContext();
			filterContext.Controller = installController;

			// Act
			installController.CallOnActionExecuting(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Null);
		}

		[Test]
		public void Should_Redirect_When_UpgradeRequired_Is_True()
		{
			// Arrange
			_applicationSettings.Installed = true;
			_applicationSettings.UpgradeRequired = true;
			ActionExecutingContext filterContext = new ActionExecutingContext();
			filterContext.Controller = _controller;

			// Act
			_controller.CallOnActionExecuting(filterContext);
			RedirectResult result = filterContext.Result as RedirectResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectResult");
			Assert.That(result.Url, Is.EqualTo("/upgrade"));
		}

		[Test]
		public void Should_Not_Redirect_When_UpgradeRequired_Is_True_Is_UpgradeController()
		{
			// Arrange
			_applicationSettings.Installed = true;
			_applicationSettings.UpgradeRequired = true;

			UpgradeControllerStub upgradeController = new UpgradeControllerStub(_applicationSettings, _userService, _repository, _settingsService, _context, _configReaderWriter);
			ActionExecutingContext filterContext = new ActionExecutingContext();
			filterContext.Controller = upgradeController;

			// Act
			upgradeController.CallOnActionExecuting(filterContext);

			// Assert
			Assert.That(filterContext.Result, Is.Null);
		}

		[Test]
		public void Should_Set_LoggedIn_User_And_ViewBag_Data()
		{
			// Arrange
			_applicationSettings.Installed = true;
			_applicationSettings.UpgradeRequired = false;
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
		public InstallControllerStub(ApplicationSettings settings, UserServiceBase userService,
			PageService pageService, SearchService searchService, IRepository respository,
			SettingsService settingsService, IUserContext context, ConfigReaderWriter configReaderWriter)
			: base(settings, userService, pageService, searchService, respository, settingsService, context, configReaderWriter)
		{

		}

		public void CallOnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
		}
	}

	internal class UpgradeControllerStub : UpgradeController
	{
		public UpgradeControllerStub(ApplicationSettings settings, UserServiceBase userService, IRepository respository,
			SettingsService settingsService, IUserContext context, ConfigReaderWriter configReaderWriter)
			: base(settings, respository, userService, context, settingsService, configReaderWriter)
		{

		}

		public void CallOnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
		}
	}
}