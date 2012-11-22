using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Domain;

namespace Roadkill.Tests.Unit.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class UserControllerTests
	{
		public static string AdminEmail = "admin@localhost";
		public static string AdminUsername = "admin";
		public static string AdminPassword = "password";

		private IConfigurationContainer _config;
		private IRepository _repository;
		private Mock<UserManager> _userManager;

		[TestFixtureSetUp]
		public void TestsSetup()
		{
			_config = new RoadkillSettings();
			_repository = null;
			_userManager = new Mock<UserManager>(_config, _repository, null);
			_userManager.Setup(u => u.Authenticate(AdminEmail, AdminPassword)).Returns(true);
		}

		[Test]
		public void Logon_Should_Redirect_And_SetUsername_ForContext()
		{
			// Arrange
			UserController userController = new UserController(_config, _userManager.Object);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Login(AdminEmail, AdminPassword, "");
			
			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
			Assert.That(userController.ModelState.Count, Is.EqualTo(0));
		}
	}
}
