using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Database;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using System.IO;
using System.Linq;
using MvcContrib.TestHelper;

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
		private SettingsService _settingsService;

		[SetUp]
		public void Setup()
		{
			_applicationSettings = new ApplicationSettings();
			_repository = new RepositoryMock();
			_settingsService = new SettingsService(_applicationSettings, _repository);

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
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Activate(UserManagerMock.ACTIVATIONKEY);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());
		}

		[Test]
		public void Activate_With_Invalid_Key_Should_Have_Model_Error()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
		public void Login_Should_Redirect()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
		public void Signup_When_LoggedIn_Should_Return_RedirectResult()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
			
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			siteSettings.AllowUserSignup = true;

			FakeSignupEmail signupEmail = new FakeSignupEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, signupEmail, null);
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
			
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			siteSettings.AllowUserSignup = true;

			FakeSignupEmail signupEmail = new FakeSignupEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, signupEmail, null);
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

		[Test]
		public void ResetPassword_Get_Should_Return_ViewResult_And_ViewName()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ViewResult result = userController.ResetPassword() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			result.AssertViewRendered(); // this checks the view name matches the method
		}

		[Test]
		public void ResetPassword_WithWindows_Auth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
		public void ResetPassword_Should_Not_Send_Email_With_Invalid_ModelState()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();

			FakeResetPasswordEmail resetEmail = new FakeResetPasswordEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, resetEmail);
			userController.SetFakeControllerContext();

			// Act	
			ViewResult result = userController.ResetPassword("fake email") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			result.AssertViewRendered();
			Assert.That(userController.ModelState.Count, Is.EqualTo(1));
			Assert.That(resetEmail.IsSent, Is.EqualTo(false));
		}

		[Test]
		public void ResetPassword_Post_Should_Have_ResetPasswordSent_View_And_Should_Send_ResetPassword_Email()
		{
			// Arrange
			string binFolder = AppDomain.CurrentDomain.BaseDirectory;
			File.WriteAllText(Path.Combine(binFolder, "ResetPassowrd.txt"), "{EMAIL}");
			File.WriteAllText(Path.Combine(binFolder, "ResetPassword.html"), "{EMAIL}");
			_applicationSettings.EmailTemplateFolder = binFolder;
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			
			string email = "test@test.com";
			_userManager.AddUser(email, "test", "test", false, true);
			_userManager.Users.First(x => x.Email == email).IsActivated = true;

			FakeResetPasswordEmail resetEmail = new FakeResetPasswordEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, resetEmail);
			userController.SetFakeControllerContext();

			// Act	
			ViewResult result = userController.ResetPassword(email) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ViewName, Is.EqualTo("ResetPasswordSent"));
			Assert.That(resetEmail.IsSent, Is.True);
			Assert.That(resetEmail.Summary.ExistingEmail, Is.EqualTo(email));
			Assert.That(resetEmail.Summary.PasswordResetKey, Is.EqualTo(UserManagerMock.RESETKEY));
		}

		[Test]
		public void CompleteResetPassword_Should_Have_Correct_Model_And_ActionResult()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
		public void CompleteResetPassword_With_WindowsAuth_Enabled_Should_Redirect()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
		public void ResendConfirmation_With_Invalid_Email_Should_Show_Signup_View()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();

			FakeResetPasswordEmail resetEmail = new FakeResetPasswordEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, resetEmail);
			userController.SetFakeControllerContext();

			// Act	
			ViewResult result = userController.ResendConfirmation("doesnt exist") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ViewName, Is.EqualTo("Signup"));
		}

		[Test]
		public void ResendConfirmation_Should_SendEmail_And_Show_SignupComplete_View_And_Set_TempData()
		{
			// Arrange
			string binFolder = AppDomain.CurrentDomain.BaseDirectory;
			File.WriteAllText(Path.Combine(binFolder, "Signup.txt"), "{EMAIL}");
			File.WriteAllText(Path.Combine(binFolder, "Signup.html"), "{EMAIL}");
			_applicationSettings.EmailTemplateFolder = binFolder;
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();

			string email = "test@test.com";
			_userManager.AddUser(email, "test", "password", false, true);
			UserSummary summary = _userManager.GetUser("test@test.com", false).ToSummary();

			FakeSignupEmail signupEmail = new FakeSignupEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, signupEmail, null);
			userController.SetFakeControllerContext();

			// Act	
			ViewResult result = userController.ResendConfirmation(email) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ViewName, Is.EqualTo("SignupComplete"));
			Assert.That(result.TempData["resend"], Is.EqualTo(true));
			Assert.That(signupEmail.IsSent, Is.EqualTo(true));
		}

		[Test]
		public void Profile_Get_Should_Return_Correct_ActionResult_And_Model()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
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
		public void Profile_Post_Should_Redirect_If_Summary_Has_No_Id()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();

			UserSummary summary = new UserSummary();
			
			// Act	
			ActionResult result = userController.Profile(summary);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Login"));
		}

		[Test]
		public void Profile_Post_Should_Return_403_When_Updated_Id_Is_Not_Logged_In_User()
		{
			// Arrange
			string loggedInEmail = "profiletest.new@test.com";
			string secondUserEmail = "seconduser@test.com";
			string secondUserNewEmail = "seconduser.new@test.com";

			_userManager.AddUser(loggedInEmail, "profiletest", "password", false, true);
			_userManager.Users.First(x => x.Email == loggedInEmail).IsActivated = true;
			Guid firstUserId = _userManager.GetUser(loggedInEmail).Id;

			_userManager.AddUser(secondUserEmail, "seconduser", "password", false, true);
			_userManager.Users.First(x => x.Email == secondUserEmail).IsActivated = true;
			Guid secondUserId = _userManager.GetUser(secondUserEmail).Id;

			_userContext.CurrentUser = firstUserId.ToString();

			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();

			UserSummary summary = new UserSummary(); // try to change the other user's email
			summary.Id = secondUserId;
			summary.ExistingEmail = secondUserEmail;
			summary.NewEmail = secondUserNewEmail;
			summary.Firstname = "test";
			summary.Lastname = "user";
			summary.ExistingUsername = "profiletest";
			summary.NewUsername = "newprofiletest";

			// Act	
			ActionResult result = userController.Profile(summary);

			// Assert
			Assert.That(result, Is.TypeOf<HttpStatusCodeResult>());

			HttpStatusCodeResult redirectResult = result as HttpStatusCodeResult;
			Assert.That(redirectResult.StatusCode, Is.EqualTo(403));
		}

		[Test]
		public void Profile_Post_Should_Update_User()
		{
			// Arrange
			string email = "profiletest@test.com";
			string newEmail = "profiletest.new@test.com";
			_userManager.AddUser(email, "profiletest", "password", false, true);
			_userManager.Users.First(x => x.Email == email).IsActivated = true;
			Guid userId = _userManager.GetUser(email).Id;

			_userContext.CurrentUser = userId.ToString();

			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();

			UserSummary summary = new UserSummary();
			summary.Id = userId;
			summary.ExistingEmail = email;
			summary.NewEmail = newEmail;
			summary.Firstname = "test";
			summary.Lastname = "user";
			summary.ExistingUsername = "profiletest";
			summary.NewUsername = "newprofiletest";

			// Act	
			ViewResult result = userController.Profile(summary) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			result.AssertViewRendered();

			User user = _userManager.GetUser(newEmail);
			Assert.That(user.Email, Is.EqualTo(summary.NewEmail));
			Assert.That(user.Username, Is.EqualTo(summary.NewUsername));
			Assert.That(user.Firstname, Is.EqualTo(summary.Firstname));
			Assert.That(user.Lastname, Is.EqualTo(summary.Lastname));
		}

		[Test]
		public void Profile_Post_Should_Update_Password_If_Changed()
		{
			// Arrange
			string email = "profiletest@test.com";
			string newPassword = "newpassword";
			string hashedPassword = User.HashPassword(newPassword, "");

			_userManager.AddUser(email, "profiletest", "password", false, true);
			_userManager.Users.First(x => x.Email == "profiletest@test.com").IsActivated = true;
			Guid userId = _userManager.GetUser(email).Id;
			_userContext.CurrentUser = userId.ToString();

			UserController userController = new UserController(_applicationSettings, _userManager, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();

			UserSummary summary = _userManager.GetUser(email).ToSummary(); // use the same summary, as profile() updates everything.
			summary.Password = newPassword;

			// Act	
			ViewResult result = userController.Profile(summary) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			result.AssertViewRendered();

			User user = _userManager.GetUser(email);
			Assert.That(user.Password, Is.EqualTo(hashedPassword));
		}
	}
}
