using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Security;
using System.IO;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class UpgradeControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceBase _userService;
		private SettingsService _settingsService;
		private UpgradeController _upgradeController;
		private ConfigReaderWriterStub _configReaderWriter;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");
			_context = _container.UserContext;
			_repository = _container.Repository;
			_settingsService = _container.SettingsService;
			_userService = new FormsAuthUserService(_applicationSettings, _repository);
			_configReaderWriter = new ConfigReaderWriterStub();

			_upgradeController = new UpgradeController(_applicationSettings, _repository, _userService, _context, _settingsService, _configReaderWriter);
		}

		[Test]
		public void Index_Should_Redirect_If_Upgrade_Is_Not_Required()
		{
			// Arrange
			_applicationSettings.UpgradeRequired = false;
			
			// Act
			ActionResult result = _upgradeController.Index();

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToAction");
		}

		// Testing the actual upgrade action is easier via Selenium.
	}
}
