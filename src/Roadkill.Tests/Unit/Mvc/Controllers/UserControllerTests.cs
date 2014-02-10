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
using Roadkill.Core.Localization;
using Roadkill.Tests.Unit.StubsAndMocks;

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
		private MvcMockContainer _mvcMockContainer;
		private EmailClientMock _emailClientMock;

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
			_emailClientMock = _container.EmailClient;

			_userService.AddUser(AdminEmail, AdminUsername, AdminPassword, true, true);
			_userService.Users[0].IsActivated = true;
			_userService.Users[0].Firstname = "Firstname";
			_userService.Users[0].Lastname = "LastnameNotSurname";

			_userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, null);
			_mvcMockContainer = _userController.SetFakeControllerContext();
		}

		[Test]
		public void Activate_Should_Redirect_When_Windows_Authentication_Is_Enabled()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.Activate(UserServiceMock.ACTIVATIONKEY);

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Activate_Should_Return_View_When_Key_Is_Valid()
		{
			// Arrange

			// Act	
			ActionResult result = _userController.Activate(UserServiceMock.ACTIVATIONKEY);

			// Assert
			result.AssertResultIs<ViewResult>();
		}

		[Test]
		public void Activate_Should_Have_Model_Error_When_Key_Is_Invalid()
		{
			// Arrange

			// Act	
			ActionResult result = _userController.Activate("badkey");

			// Assert
			result.AssertResultIs<ViewResult>();
			Assert.That(_userController.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Activate_Should_Return_RedirectResult_When_Key_Is_Empty()
		{
			// Arrange + Act	
			ActionResult result = _userController.Activate("");

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void LoggedInAs_Should_Return_PartialView()
		{
			// Arrange + Act	
			ActionResult result = _userController.LoggedInAs();

			// Assert
			PartialViewResult partialViewResult = result.AssertResultIs<PartialViewResult>();
			partialViewResult.AssertPartialViewRendered(); // check the view matches the action name
		}

		[Test]
		public void Login_GET_Should_Return_View()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;

			// Act	
			ActionResult result = _userController.Login();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();
		}

		[Test]
		public void Login_GET_Should_Display_Blank_View_When_ReturnUrl_Is_From_FileManager()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			_userController.Request.QueryString.Add("ReturnUrl", "/filemanager/select");

			// Act	
			ActionResult result = _userController.Login();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("BlankLogin"));
		}

		[Test]
		public void Login_GET_Should_Display_Blank_View_When_ReturnUrl_Is_From_Help()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			_mvcMockContainer.Request.Object.QueryString.Add("ReturnUrl", "/help");

			// Act	
			ActionResult result = _userController.Login();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("BlankLogin"));
		}

		[Test]
		public void Login_GET_Should_Redirect_When_Windows_Authentication_Is_Enabled()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.Login();

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Login_POST_Should_Redirect_When_Windows_Authentication_Is_Enabled()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.Login(AdminEmail, AdminPassword, "");

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void Login_POST_Should_Redirect_When_Authentication_Is_Successful()
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
		public void Login_POST_Should_Redirect_To_FromUrl_When_Authentication_Is_Successful()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;

			// Act	
			ActionResult result = _userController.Login(AdminEmail, AdminPassword, "http://www.google.com");

			// Assert
			RedirectResult redirectResult = result.AssertResultIs<RedirectResult>();
			Assert.That(redirectResult.Url, Is.EqualTo("http://www.google.com"));
		}

		[Test]
		public void Login_POST_With_Wrong_Email_And_Password_Should_Have_Model_Error()
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
		public void Login_POST_Should_Display_Blank_View_When_ReturnUrl_Is_From_FileManager_And_Authentication_Is_Unsuccessful()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			_userController.Request.QueryString.Add("ReturnUrl", "/filemanager/select");

			// Act	
			ActionResult result = _userController.Login("wrongemail", "wrongpassword", "");

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("BlankLogin"));
		}

		[Test]
		public void Login_POST_Should_Display_Blank_View_When_ReturnUrl_Is_From_Help_And_Authentication_Is_Unsuccessful()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			_mvcMockContainer.Request.Object.QueryString.Add("ReturnUrl", "/help");

			// Act	
			ActionResult result = _userController.Login("wrongemail", "wrongpassword", "");

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("BlankLogin"));
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
			_applicationSettings.UseWindowsAuthentication = false;
			
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			siteSettings.AllowUserSignup = true;

			SignupEmailStub signupEmail = new SignupEmailStub(_applicationSettings, _repository, _emailClientMock);
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
			_applicationSettings.UseWindowsAuthentication = false;
			
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			siteSettings.AllowUserSignup = true;

			SignupEmailStub signupEmail = new SignupEmailStub(_applicationSettings, _repository, _emailClientMock);
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

			SignupEmailStub signupEmail = new SignupEmailStub(_applicationSettings, _repository, _emailClientMock); // change the signup email
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
		public void Signup_POST_Should_Set_ModelState_Error_From_Bad_Recaptcha()
		{
			// Arrange
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			siteSettings.AllowUserSignup = true;

			UserViewModel model = new UserViewModel();
			model.NewEmail = "blah@localhost";
			model.Password = "password";
			model.PasswordConfirmation = "password";

			// Act
			ActionResult result = _userController.Signup(model, false);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.Not.EqualTo("Signup"));
			Assert.That(_userController.ModelState.Count, Is.EqualTo(1));
			Assert.That(_userController.ModelState["General"].Errors[0].ErrorMessage, Is.EqualTo(SiteStrings.Signup_Error_Recaptcha));
		}

		[Test]
		public void ResetPassword_GET_Should_Return_ViewResult_And_ViewName()
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
		public void ResetPassword_GET_With_Windows_Auth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.ResetPassword();

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void ResetPassword_POST_With_Windows_Auth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.ResetPassword("someemail");

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void ResetPassword_POST_Should_Not_Send_Email_With_Invalid_ModelState()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();

			ResetPasswordEmailStub resetEmail = new ResetPasswordEmailStub(_applicationSettings, _repository, _emailClientMock);
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
		public void ResetPassword_POST_Should_Have_ResetPasswordSent_View_And_Should_Send_ResetPassword_Email()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			
			string email = "test@test.com";
			_userService.AddUser(email, "test", "test", false, true);
			_userService.Users.First(x => x.Email == email).IsActivated = true;

			ResetPasswordEmailStub resetEmail = new ResetPasswordEmailStub(_applicationSettings, _repository, _emailClientMock);
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
		public void ResetPassword_POST_Should_Set_ModelState_Error_When_Email_Is_Empty()
		{
			// Arrange

			// Act
			ActionResult result = _userController.ResetPassword("");

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.Not.EqualTo("Signup"));
			Assert.That(_userController.ModelState.Count, Is.EqualTo(1));
			Assert.That(_userController.ModelState["General"].Errors[0].ErrorMessage, Is.EqualTo(SiteStrings.ResetPassword_Error_MissingEmail));
		}

		[Test]
		public void CompleteResetPassword_GET_Should_Have_Correct_Model_And_ActionResult()
		{
			// Arrange
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, null);
			userController.SetFakeControllerContext();
			_userService.ResetPassword(AdminEmail);

			// Act	
			ActionResult result = userController.CompleteResetPassword(UserServiceMock.RESETKEY);

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
		public void CompleteResetPassword_GET_Should_Return_CompleteResetPasswordInvalid_View_When_User_Is_Null()
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
		public void CompleteResetPassword_GET_With_WindowsAuth_Enabled_Should_Redirect()
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
		public void CompleteResetPassword_POST_With_Windows_Auth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			_applicationSettings.UseWindowsAuthentication = true;

			// Act	
			ActionResult result = _userController.CompleteResetPassword("key", model);

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void CompleteResetPassword_POST_Should_Change_Password()
		{
			// Arrange
			_userService.AddUser("email@localhost", "username", "OldPassword", false, true);
			User newUser = _userService.GetUser("email@localhost", false);
			newUser.IsActivated = true;
			newUser.PasswordResetKey = UserServiceMock.RESETKEY;

			UserViewModel model = new UserViewModel();
			model.Password = "NewPassword";
			model.PasswordConfirmation = "NewPassword";

			// Act	
			ActionResult result = _userController.CompleteResetPassword(UserServiceMock.RESETKEY, model);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("CompleteResetPasswordSuccessful"));
			Assert.That(_userService.Authenticate("email@localhost", "NewPassword"), Is.True);
		}

		[Test]
		public void CompleteResetPassword_POST_Should_Return_CompleteResetPasswordInvalid_When_Key_Is_Invalid()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			model.Password = "NewPassword";
			model.PasswordConfirmation = "NewPassword";

			// Act	
			ActionResult result = _userController.CompleteResetPassword("a key that doesn't exist", model);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("CompleteResetPasswordInvalid"));
		}

		[Test]
		public void CompleteResetPassword_POST_Should_Have_Invalid_ModelState_When_Passwords_Dont_Match()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			model.Password = "NewPassword";
			model.PasswordConfirmation = "DoesntMatch";

			// Act	
			ActionResult result = _userController.CompleteResetPassword(UserServiceMock.RESETKEY, model);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.Not.EqualTo("Signup"));
			Assert.That(_userController.ModelState.Count, Is.EqualTo(1));
			Assert.That(_userController.ModelState["Passwords"].Errors[0].ErrorMessage, Is.EqualTo(SiteStrings.ResetPassword_Error));
		}

		[Test]
		public void ResendConfirmation_POST_With_Invalid_Email_Should_Show_Signup_View()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();

			ResetPasswordEmailStub resetEmail = new ResetPasswordEmailStub(_applicationSettings, _repository, _emailClientMock);
			UserController userController = new UserController(_applicationSettings, _userService, _userContext, _settingsService, null, resetEmail);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.ResendConfirmation("doesnt exist");

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("Signup"));
		}

		[Test]
		public void ResendConfirmation_POST_Should_SendEmail_And_Show_SignupComplete_View_And_Set_TempData()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			SiteSettings siteSettings = _settingsService.GetSiteSettings();

			string email = "test@test.com";
			_userService.AddUser(email, "test", "password", false, true);
			UserViewModel model = new UserViewModel(_userService.GetUser("test@test.com", false));

			SignupEmailStub signupEmail = new SignupEmailStub(_applicationSettings, _repository, _emailClientMock);
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
		public void Profile_GET_Should_Redirect_If_No_Logged_In_User()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;

			// Act
			ActionResult result = _userController.Profile();

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Login");
		}

		[Test]
		public void Profile_GET_Should_Return_Correct_ActionResult_And_Model()
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
		public void Profile_POST_Should_Redirect_If_Summary_Has_No_Id()
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
		public void Profile_POST_Should_Return_403_When_Updated_Id_Is_Not_Logged_In_User()
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
		public void Profile_POST_Should_Update_User()
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
		public void Profile_POST_Should_Update_Password_If_Changed()
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
