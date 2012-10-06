using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Controllers;

namespace Roadkill.Tests.Controllers
{
	[TestFixture]
	public class UserControllerTests
	{
		private User _testUser;

		[SetUp]
		public void Setup()
		{
			Mock<User> mockUser = new Mock<User>();
			mockUser.SetupProperty<string>(x => x.Email, "test@localhost");
			mockUser.SetupProperty<string>(x => x.Username, "testuser");
			_testUser = mockUser.Object;

			Mock<UserManager> mock = new Mock<UserManager>();
			UserManager.Initialize(mock.Object);
			mock.Setup(x => x.GetUser(_testUser.Email)).Returns(mockUser.Object);
			mock.Setup(x => x.Authenticate(_testUser.Email, "")).Returns(true);
			mock.Setup(x => x.GetLoggedInUserName(It.IsAny<HttpContextBase>())).Returns(_testUser.Username);		
		}

		[Test]
		public void Logon_Should_Redirect_And_SetUsername_ForContext()
		{
			// Arrange
			UserController userController = new UserController();
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Login(_testUser.Email, "", "");
			
			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
			Assert.That(userController.GetCurrentUser(userController.HttpContext), Is.EqualTo(_testUser.Username));
		}
	}
}
