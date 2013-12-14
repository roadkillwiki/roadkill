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

		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private IUserContext _userContext;
		private SettingsService _settingsService;
		private UserController _userController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");
			_repository = _container.Repository;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_userContext = _container.UserContext;

			_userService.AddUser(AdminEmail, AdminUsername, AdminPassword, true, true);
			_userService.Users[0].IsActivated = true;
			_userService.Users[0].Firstname = "Firstname";
			_userService.Users[0].Lastname = "LastnameNotSurname";

			_userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, null);
			_userController.SetFakeControllerContext();
		}

		[Test]
		public void Activate_With_Valid_Key_Returns_View()
		{
			// Arrange

			// Act	
			ActionResult result = _userController.Activate(UserServiceMock.ACTIVATIONKEY);

			// Assert
			result.AssertResultIs<ViewResult>();
		}

		[Test]
		public void Activate_With_Invalid_Key_Should_Have_Model_Error()
		{
			// Arrange

			// Act	
			ActionResult result = _userController.Activate("badkey");

			// Assert
			result.AssertResultIs<ViewResult>();
			Assert.That(_userController.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Activate_With_Empty_Key_Returns_RedirectResult()
		{
			// Arrange

			// Act	
			ActionResult result = _userController.Activate("");

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Login_Should_Redirect()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;

			// Act	
			ActionResult result = _userController.Login(AdminEmail, AdminPassword, "");

			// Assert
			result.AssertResultIs<RedirectToRouteResult>();
			Assert.That(_userController.ModelState.Count, Is.EqualTo(0));
		}

		[Test]
		public void Login_With_Wrong_Email_And_Password_Should_Have_Model_Error()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;

			// Act	
			ActionResult result = _userController.Login("wrongemail", "wrongpassword", "");

			// Assert
			result.AssertResultIs<ViewResult>();
			Assert.That(_userController.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Logout_Should_Have_RedirectResult()
		{
			// Arrange

			// Act	
			ActionResult result = _userController.Logout();

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Signup_POST_When_LoggedIn_Should_Return_RedirectResult()
		{
			// Arrange

			// Act	
			_userController.Login(AdminEmail, AdminPassword, "");
			ActionResult result = _userController.Signup(null, null);

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Signup_POST_With_WindowsAuth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			_repository.SiteSettings.AllowUserSignup = true;
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.Signup(null, null);

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Signup_POST_With_Signups_Disabled_Should_Return_RedirectResult()
		{
			// Arrange
			_repository.SiteSettings.AllowUserSignup = false;

			// Act	
			ActionResult result = _userController.Signup(null, null);

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Signup_POST_Should_Send_Email()
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
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, signupEmail, null);
			userController.SetFakeControllerContext();

			UserViewModel model = new UserViewModel();
			model.NewEmail = "blah@localhost";
			model.Password = "password";
			model.PasswordConfirmation = "password";

			// Act	
			ActionResult result = userController.Signup(model, null);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("SignupComplete"));
			Assert.That(signupEmail.IsSent, Is.True);
			Assert.That(signupEmail.ViewModel, Is.EqualTo(model));
		}

		[Test]
		public void Signup_POST_Should_Not_Send_Email_With_Invalid_ModelState()
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
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, signupEmail, null);
			userController.SetFakeControllerContext();
			userController.ModelState.AddModelError("key", "this is used to force ModelState.IsValid to false");

			UserViewModel model = new UserViewModel();

			// Act
			ActionResult result = userController.Signup(model, null);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.Not.EqualTo("Signup"));
			Assert.That(userController.ModelState.Count, Is.EqualTo(1));
			Assert.That(signupEmail.IsSent, Is.False);
		}

		[Test]
		public void Signup_POST_Should_Set_ModelState_Error_From_SecurityException()
		{
			// Arrange
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			siteSettings.AllowUserSignup = true;

			FakeSignupEmail signupEmail = new FakeSignupEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, signupEmail, null);
			userController.SetFakeControllerContext();
			
			_userService.ThrowSecurityExceptionOnSignup = true;

			UserViewModel model = new UserViewModel();

			// Act
			ActionResult result = userController.Signup(model, null);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.Not.EqualTo("Signup"));
			Assert.That(userController.ModelState.Count, Is.EqualTo(1));
			Assert.That(userController.ModelState["General"].Errors[0].ErrorMessage, Is.EqualTo("ThrowSecurityExceptionOnSignup"));
		}

		[Test]
		public void ResetPassword_Get_Should_Return_ViewResult_And_ViewName()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;

			// Act	
			ActionResult result = _userController.ResetPassword();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered(); // this checks the view name matches the method
		}

		[Test]
		public void ResetPassword_WithWindows_Auth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.Logout();

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void ResetPassword_Should_Not_Send_Email_With_Invalid_ModelState()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();

			FakeResetPasswordEmail resetEmail = new FakeResetPasswordEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, resetEmail);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.ResetPassword("fake email");

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();
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
			_userService.AddUser(email, "test", "test", false, true);
			_userService.Users.First(x => x.Email == email).IsActivated = true;

			FakeResetPasswordEmail resetEmail = new FakeResetPasswordEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, resetEmail);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.ResetPassword(email);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("ResetPasswordSent"));
			Assert.That(resetEmail.IsSent, Is.True);
			Assert.That(resetEmail.Model.ExistingEmail, Is.EqualTo(email));
			Assert.That(resetEmail.Model.PasswordResetKey, Is.EqualTo(UserServiceMock.RESETKEY));
		}

		[Test]
		public void CompleteResetPassword_Should_Have_Correct_Model_And_ActionResult()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();
			_userService.ResetPassword(AdminEmail);

			// Act	
			ActionResult result = userController.CompleteResetPassword("resetkey");

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();

			UserViewModel model = viewResult.ModelFromActionResult<UserViewModel>();
			User expectedUser = _userService.Users[0];

			Assert.That(model.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(model.NewEmail, Is.EqualTo(expectedUser.Email));
			Assert.That(model.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(model.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(model.Lastname, Is.EqualTo(expectedUser.Lastname));
		}

		[Test]
		public void CompleteResetPassword_Should_Return_CompleteResetPasswordInvalid_View_When_User_Is_Null()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.CompleteResetPassword("invalidresetkey");

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("CompleteResetPasswordInvalid"));
		}

		[Test]
		public void CompleteResetPassword_With_WindowsAuth_Enabled_Should_Redirect()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.CompleteResetPassword("resetkey");

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void ResendConfirmation_With_Invalid_Email_Should_Show_Signup_View()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();

			FakeResetPasswordEmail resetEmail = new FakeResetPasswordEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, resetEmail);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.ResendConfirmation("doesnt exist");

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("Signup"));
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
			_userService.AddUser(email, "test", "password", false, true);
			UserViewModel model = new UserViewModel(_userService.GetUser("test@test.com", false));

			FakeSignupEmail signupEmail = new FakeSignupEmail(_applicationSettings, siteSettings);
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, signupEmail, null);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.ResendConfirmation(email);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("SignupComplete"));
			Assert.That(viewResult.TempData["resend"], Is.EqualTo(true));
			Assert.That(signupEmail.IsSent, Is.EqualTo(true));
		}

		[Test]
		public void Profile_Get_Should_Return_Correct_ActionResult_And_Model()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;

			// Act
			_userController.Login(AdminEmail, AdminPassword, "");
			//_userContext.CurrentUser = _userManager.Users[0].Id.ToString(); // the base controller's OnActionExecuted normally sets this.
			ActionResult result = _userController.Profile();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();

			UserViewModel model = viewResult.ModelFromActionResult<UserViewModel>();
			User expectedUser = _userService.Users[0];

			Assert.That(model.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(model.NewEmail, Is.EqualTo(expectedUser.Email));
			Assert.That(model.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(model.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(model.Lastname, Is.EqualTo(expectedUser.Lastname));
		}

		[Test]
		public void Profile_Post_Should_Redirect_If_Summary_Has_No_Id()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			
			// Act	
			ActionResult result = _userController.Profile(model);

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Login");
		}

		[Test]
		public void Profile_Post_Should_Return_403_When_Updated_Id_Is_Not_Logged_In_User()
		{
			// Arrange
			string loggedInEmail = "profiletest.new@test.com";
			string secondUserEmail = "seconduser@test.com";
			string secondUserNewEmail = "seconduser.new@test.com";

			_userService.AddUser(loggedInEmail, "profiletest", "password", false, true);
			_userService.Users.First(x => x.Email == loggedInEmail).IsActivated = true;
			Guid firstUserId = _userService.GetUser(loggedInEmail).Id;

			_userService.AddUser(secondUserEmail, "seconduser", "password", false, true);
			_userService.Users.First(x => x.Email == secondUserEmail).IsActivated = true;
			Guid secondUserId = _userService.GetUser(secondUserEmail).Id;

			_userContext.CurrentUser = firstUserId.ToString();

			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();

			UserViewModel model = new UserViewModel(); // try to change the other user's email
			model.Id = secondUserId;
			model.ExistingEmail = secondUserEmail;
			model.NewEmail = secondUserNewEmail;
			model.Firstname = "test";
			model.Lastname = "user";
			model.ExistingUsername = "profiletest";
			model.NewUsername = "newprofiletest";

			// Act	
			ActionResult result = userController.Profile(model);

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
			_userService.AddUser(email, "profiletest", "password", false, true);
			_userService.Users.First(x => x.Email == email).IsActivated = true;
			Guid userId = _userService.GetUser(email).Id;

			_userContext.CurrentUser = userId.ToString();

			UserViewModel model = new UserViewModel();
			model.Id = userId;
			model.ExistingEmail = email;
			model.NewEmail = newEmail;
			model.Firstname = "test";
			model.Lastname = "user";
			model.ExistingUsername = "profiletest";
			model.NewUsername = "newprofiletest";

			// Act	
			ActionResult result = _userController.Profile(model) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			result.AssertViewRendered();

			User user = _userService.GetUser(newEmail);
			Assert.That(user.Email, Is.EqualTo(model.NewEmail));
			Assert.That(user.Username, Is.EqualTo(model.NewUsername));
			Assert.That(user.Firstname, Is.EqualTo(model.Firstname));
			Assert.That(user.Lastname, Is.EqualTo(model.Lastname));
		}

		[Test]
		public void Profile_Post_Should_Update_Password_If_Changed()
		{
			// Arrange
			string email = "profiletest@test.com";
			string newPassword = "newpassword";
			string hashedPassword = User.HashPassword(newPassword, "");

			_userService.AddUser(email, "profiletest", "password", false, true);
			_userService.Users.First(x => x.Email == "profiletest@test.com").IsActivated = true;
			Guid userId = _userService.GetUser(email).Id;
			_userContext.CurrentUser = userId.ToString();

			UserViewModel model = new UserViewModel(_userService.GetUser(email)); // use the same model, as profile() updates everything.
			model.Password = newPassword;

			// Act	
			ActionResult result = _userController.Profile(model);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			User user = _userService.GetUser(email);
			Assert.That(user.Password, Is.EqualTo(hashedPassword));
		}

		[Test]
		public void Signup_GET_Should_Return_View()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			_repository.SiteSettings.AllowUserSignup = true;
			_userContext.CurrentUser = "";
			_applicationSettings.UseWindowsAuthentication = false;

			// Act	
			ActionResult result = _userController.Signup();

			// Assert
			result.AssertResultIs<ViewResult>();
		}

		[Test]
		public void Signup_GET_Should_Redirect_When_Logged_In()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			_repository.SiteSettings.AllowUserSignup = true;
			_userContext.CurrentUser = Guid.NewGuid().ToString();

			// Act	
			ActionResult result = _userController.Signup();

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Signup_GET_Should_Redirect_When_Signups_Are_Disabled()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			_repository.SiteSettings.AllowUserSignup = false;

			// Act	
			ActionResult result = _userController.Signup();

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Signup_GET_Should_Redirect_When_Windows_Auth_Is_Enabled()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.Signup();

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}
	}
}
