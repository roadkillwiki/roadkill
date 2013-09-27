using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Security;

namespace Roadkill.Tests.Unit.Mvc.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class UpgradeControllerTests
	{
		private ApplicationSettings _settings;
		private IUserContext _context;
		private RepositoryMock _repository;

		private UserManagerBase _userManager;
		private SettingsManager _settingsManager;
		private UpgradeController _controller;

		[SetUp]
		public void Setup()
		{
			_context = new Mock<IUserContext>().Object;
			_settings = new ApplicationSettings();
			_settings.Installed = true;

			_repository = new RepositoryMock();
			_repository.SiteSettings = new SiteSettings();
			_repository.SiteSettings.MarkupType = "Creole";
			_userManager = new FormsAuthUserManager(_settings, _repository);

			_controller = new UpgradeController(_settings, _repository, _userManager, _context, _settingsManager);
		}

		[Test]
		public void Index_Should_Redirect_If_Upgrade_Is_Not_Required()
		{
			// Arrange
			_settings.UpgradeRequired = false;
			
			// Act
			ActionResult result = _controller.Index();

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToAction");
		}

		// Testing the actual upgrade action is easier via Selenium.
	}
}
