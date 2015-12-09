using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Localization;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;
using Roadkill.Tests.Unit.StubsAndMocks.Mvc;

namespace Roadkill.Tests.Unit.Mvc.Controllers
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
		public void activate_should_redirect_when_windows_authentication_is_enabled()
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
		public void activate_should_return_view_when_key_is_valid()
		{
			// Arrange

			// Act	
			ActionResult result = _userController.Activate(UserServiceMock.ACTIVATIONKEY);

			// Assert
			result.AssertResultIs<ViewResult>();
		}

		[Test]
		public void activate_should_have_model_error_when_key_is_invalid()
		{
			// Arrange

			// Act	
			ActionResult result = _userController.Activate("badkey");

			// Assert
			result.AssertResultIs<ViewResult>();
			Assert.That(_userController.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void activate_should_return_redirectresult_when_key_is_empty()
		{
			// Arrange + Act	
			ActionResult result = _userController.Activate("");

			// Assert
			RedirectToRouteResult redirectResult = result.AssertResultIs<RedirectToRouteResult>();
			redirectResult.AssertActionRouteIs("Index");
			redirectResult.AssertControllerRouteIs("Home");
		}

		[Test]
		public void loggedinas_should_return_partialview()
		{
			// Arrange + Act	
			ActionResult result = _userController.LoggedInAs();

			// Assert
			PartialViewResult partialViewResult = result.AssertResultIs<PartialViewResult>();
			partialViewResult.AssertPartialViewRendered(); // check the view matches the action name
		}

		[Test]
		public void login_get_should_return_view()
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
		public void login_get_should_display_blank_view_when_returnurl_is_from_filemanager()
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
		public void login_get_should_display_blank_view_when_returnurl_is_from_help()
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
		public void login_get_should_redirect_when_windows_authentication_is_enabled()
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
		public void login_post_should_redirect_when_windows_authentication_is_enabled()
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
		public void login_post_should_redirect_when_authentication_is_successful()
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
		public void login_post_should_redirect_to_fromurl_when_authentication_is_successful()
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
		public void login_post_with_wrong_email_and_password_should_have_model_error()
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
		public void login_post_should_display_blank_view_when_returnurl_is_from_filemanager_and_authentication_is_unsuccessful()
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
		public void login_post_should_display_blank_view_when_returnurl_is_from_help_and_authentication_is_unsuccessful()
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
		public void logout_should_have_redirectresult()
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
		public void signup_post_when_loggedin_should_return_redirectresult()
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
		public void signup_post_with_windowsauth_enabled_should_return_redirectresult()
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
		public void signup_post_with_signups_disabled_should_return_redirectresult()
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
		public void signup_post_should_send_email()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			
			Core.Configuration.SiteSettings siteSettings = _settingsService.GetSiteSettings();
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
		public void signup_post_should_not_send_email_with_invalid_modelstate()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			
			Core.Configuration.SiteSettings siteSettings = _settingsService.GetSiteSettings();
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
		public void signup_post_should_set_modelstate_error_from_securityexception()
		{
			// Arrange
			Core.Configuration.SiteSettings siteSettings = _settingsService.GetSiteSettings();
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
		public void signup_post_should_set_modelstate_error_from_bad_recaptcha()
		{
			// Arrange
			Core.Configuration.SiteSettings siteSettings = _settingsService.GetSiteSettings();
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
		public void resetpassword_get_should_return_viewresult_and_viewname()
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
		public void resetpassword_get_with_windows_auth_enabled_should_return_redirectresult()
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
		public void resetpassword_post_with_windows_auth_enabled_should_return_redirectresult()
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
		public void resetpassword_post_should_not_send_email_with_invalid_modelstate()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			Core.Configuration.SiteSettings siteSettings = _settingsService.GetSiteSettings();

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
		public void resetpassword_post_should_have_resetpasswordsent_view_and_should_send_resetpassword_email()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			Core.Configuration.SiteSettings siteSettings = _settingsService.GetSiteSettings();
			
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
		public void resetpassword_post_should_set_modelstate_error_when_email_is_empty()
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
		public void completeresetpassword_get_should_have_correct_model_and_actionresult()
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
		public void completeresetpassword_get_should_return_completeresetpasswordinvalid_view_when_user_is_null()
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
		public void completeresetpassword_get_with_windowsauth_enabled_should_redirect()
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
		public void completeresetpassword_post_with_windows_auth_enabled_should_return_redirectresult()
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
		public void completeresetpassword_post_should_change_password()
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
		public void completeresetpassword_post_should_return_completeresetpasswordinvalid_when_key_is_invalid()
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
		public void completeresetpassword_post_should_have_invalid_modelstate_when_passwords_dont_match()
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
		public void resendconfirmation_post_with_invalid_email_should_show_signup_view()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			Core.Configuration.SiteSettings siteSettings = _settingsService.GetSiteSettings();

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
		public void resendconfirmation_post_should_sendemail_and_show_signupcomplete_view_and_set_tempdata()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = false;
			Core.Configuration.SiteSettings siteSettings = _settingsService.GetSiteSettings();

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
		public void profile_get_should_redirect_if_no_logged_in_user()
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
		public void profile_get_should_return_correct_actionresult_and_model()
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
		public void profile_post_should_redirect_if_summary_has_no_id()
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
		public void profile_post_should_return_403_when_updated_id_is_not_logged_in_user()
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
		public void profile_post_should_update_user()
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
		public void profile_post_should_update_password_if_changed()
		{
			// Arrange
			string email = "profiletest@test.com";
			string newPassword = "newpassword";
			_userService.AddUser(email, "profiletest", "password", false, true);
			User newUser = _userService.Users.First(x => x.Email == email);
			newUser.IsActivated = true;

			string existingHash = newUser.Password;

			Guid userId = newUser.Id;
			_userContext.CurrentUser = userId.ToString();

			UserViewModel model = new UserViewModel(newUser);
			model.Password = newPassword;

			// Act	
			ActionResult result = _userController.Profile(model);

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			viewResult.AssertViewRendered();

			User user = _userService.GetUser(email);
			Assert.That(user.Password, Is.Not.EqualTo(existingHash));
			Assert.That(user.Password, Is.Not.Empty.Or.Null);
			Assert.That(user.Password.Length, Is.GreaterThan(10));
		}

		[Test]
		public void signup_get_should_return_view()
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
		public void signup_get_should_redirect_when_logged_in()
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
		public void signup_get_should_redirect_when_signups_are_disabled()
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
		public void signup_get_should_redirect_when_windows_auth_is_enabled()
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
