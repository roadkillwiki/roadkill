using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Database;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using System.IO;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class UserControllerTests
	{
		public static string AdminEmail = "admin@localhost";
		public static string AdminUsername = "admin";
		public static string AdminPassword = "password";

		private ApplicationSettings _applicationSettings;
		private RepositoryMock _repository;
		private UserManagerMock _userManager;
		private IUserContext _userContext;
		private SettingsManager _settingsManager;

		[SetUp]
		public void Setup()
		{
			_applicationSettings = new ApplicationSettings();
			_repository = new RepositoryMock();
			_settingsManager = new SettingsManager(_applicationSettings, _repository);

			_userManager = new UserManagerMock(_applicationSettings, _repository);
			_userManager.AddUser(AdminEmail, AdminUsername, AdminPassword, true, true);
			_userManager.Users[0].IsActivated = true;
			_userManager.Users[0].Firstname = "Firstname";
			_userManager.Users[0].Lastname = "LastnameNotSurname";

			_userContext = new UserContext(_userManager);
		}

		[Test]
		public void Activate_With_Valid_Key_Returns_View()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Activate("activatekey");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());
		}

		[Test]
		public void Activate_With_Invalid_Key_Should_Have_Model_Error()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Activate("badkey");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());
			Assert.That(userController.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Activate_With_Empty_Key_Returns_RedirectResult()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Activate("");

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void CompleteResetPassword_Has_Correct_Model_And_ActionResult()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();
			_userManager.ResetPassword(AdminEmail);

			// Act	
			ActionResult result = userController.CompleteResetPassword("resetkey");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());

			UserSummary summary = result.ModelFromActionResult<UserSummary>();
			User expectedUser = _userManager.Users[0];

			Assert.That(summary.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(summary.NewEmail, Is.EqualTo(expectedUser.Email));
			Assert.That(summary.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(summary.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(summary.Lastname, Is.EqualTo(expectedUser.Lastname));
		}

		[Test]
		public void CompleteResetPassword_With_WindowsAuth_Enabled_Redirects()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.CompleteResetPassword("resetkey");

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Login_Should_Redirect()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Login(AdminEmail, AdminPassword, "");

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
			Assert.That(userController.ModelState.Count, Is.EqualTo(0));
		}

		[Test]
		public void Login_With_Wrong_Email_And_Password_Should_Have_Model_Error()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Login("wrongemail", "wrongpassword", "");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());
			Assert.That(userController.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Logout_Should_Have_RedirectResult()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Logout();

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Profile_Should_Return_Correct_ActionResult_And_Model()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act
			userController.Login(AdminEmail, AdminPassword, "");
			//_userContext.CurrentUser = _userManager.Users[0].Id.ToString(); // base controller's OnActionExecuted normally sets this.
			ActionResult result = userController.Profile();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());

			UserSummary summary = result.ModelFromActionResult<UserSummary>();
			User expectedUser = _userManager.Users[0];

			Assert.That(summary.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(summary.NewEmail, Is.EqualTo(expectedUser.Email));
			Assert.That(summary.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(summary.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(summary.Lastname, Is.EqualTo(expectedUser.Lastname));
		}

		[Test]
		public void Profile_Post_Should_Update_User()
		{

			_applicationSettings.UseWindowsAuthentication = false;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			//UserSummary summary = _userManager.get


			// Act
			userController.Login(AdminEmail, AdminPassword, "");
			//ActionResult result = userController.Profile(

		}

		[Test]
		[Ignore]
		public void ResetPassword_Should_Return_ViewResult()
		{

		}

		[Test]
		public void ResetPassword_WithWindows_Auth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Logout();

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		[Ignore]
		public void ResetPassword_Post_Should_()
		{

		}

		[Test]
		[Ignore]
		public void ResendConfirmation_Post()
		{

		}

		[Test]
		public void Signup_When_LoggedIn_Should_Return_RedirectResult()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			userController.Login(AdminEmail, AdminPassword, "");
			ActionResult result = userController.Signup(null, null);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Signup_With_WindowsAuth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			_repository.SiteSettings.AllowUserSignup = true;
			_applicationSettings.UseWindowsAuthentication = true;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Signup(null, null);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Signup_With_Signups_Disabled_Should_Return_RedirectResult()
		{
			// Arrange
			_repository.SiteSettings.AllowUserSignup = false;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Signup(null, null);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Signup_Should_Send_Email()
		{
			// Arrange
			string binFolder = AppDomain.CurrentDomain.BaseDirectory;
			File.WriteAllText(Path.Combine(binFolder, "Signup.txt"), "{EMAIL}");
			File.WriteAllText(Path.Combine(binFolder, "Signup.html"), "{EMAIL}");
			_applicationSettings.EmailTemplateFolder = binFolder;
			_applicationSettings.UseWindowsAuthentication = false;
			
			SiteSettings siteSettings = _settingsManager.GetSiteSettings();
			siteSettings.AllowUserSignup = true;

			FakeSignupEmail signupEmail = new FakeSignupEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, signupEmail, null);
			userController.SetFakeControllerContext();

			UserSummary summary = new UserSummary();
			summary.NewEmail = "blah@localhost";
			summary.Password = "password";
			summary.PasswordConfirmation = "password";

			// Act	
			ViewResult result = userController.Signup(summary, null) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ViewName, Is.EqualTo("SignupComplete"));
			Assert.That(signupEmail.IsSent, Is.True);
			Assert.That(signupEmail.Summary, Is.EqualTo(summary));
		}

		[Test]
		public void Signup_Should_Not_Send_Email_With_Invalid_ModelState()
		{
			// Arrange
			string binFolder = AppDomain.CurrentDomain.BaseDirectory;
			File.WriteAllText(Path.Combine(binFolder, "Signup.txt"), "{EMAIL}");
			File.WriteAllText(Path.Combine(binFolder, "Signup.html"), "{EMAIL}");
			_applicationSettings.EmailTemplateFolder = binFolder;
			_applicationSettings.UseWindowsAuthentication = false;
			
			SiteSettings siteSettings = _settingsManager.GetSiteSettings();
			siteSettings.AllowUserSignup = true;

			FakeSignupEmail signupEmail = new FakeSignupEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsManager, signupEmail, null);
			userController.SetFakeControllerContext();
			userController.ModelState.AddModelError("key", "this is used to force ModelState.IsValid to false");

			UserSummary summary = new UserSummary();

			// Act
			ViewResult result = userController.Signup(summary, null) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ViewName, Is.Not.EqualTo("Signup"));
			Assert.That(userController.ModelState.Count, Is.EqualTo(1));
			Assert.That(signupEmail.IsSent, Is.False);
		}

		// TODO:
		// Reset password POST (should send email)
		// Resend password confirm POST
		// Profile POST
	}
}
