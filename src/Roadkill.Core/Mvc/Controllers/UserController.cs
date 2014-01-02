using System;
using System.Web.Mvc;
using System.Web.Security;
using Roadkill.Core.Localization;
using Roadkill.Core.Configuration;
using RoadkillUser = Roadkill.Core.Database.User;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Email;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// All actions related to user based tasks.
	/// </summary>
	public class UserController : ControllerBase
	{
		private SignupEmail _signupEmail;
		private ResetPasswordEmail _resetPasswordEmail;
		
		public UserController(ApplicationSettings settings, UserServiceBase userManager,
			IUserContext context, SettingsService settingsService, 
			SignupEmail signupEmail, ResetPasswordEmail resetPasswordEmail)
			: base(settings, userManager, context, settingsService) 
		{
			_signupEmail = signupEmail;
			_resetPasswordEmail = resetPasswordEmail;
		}

		/// <summary>
		/// Activates a newly registered account.
		/// </summary>
		/// <returns></returns>
		public ActionResult Activate(string id)
		{
			if (ApplicationSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			if (string.IsNullOrEmpty(id))
				return RedirectToAction("Index", "Home");

			if (!UserService.ActivateUser(id))
			{
				ModelState.AddModelError("General", SiteStrings.Activate_Error);
			}

			return View();
		}

		/// <summary>
		/// Displays the password text boxes for a password reset request.
		/// </summary>
		public ActionResult CompleteResetPassword(string id)
		{
			if (ApplicationSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			RoadkillUser user = UserService.GetUserByResetKey(id);
			
			if (user == null)
			{
				return View("CompleteResetPasswordInvalid");
			}
			else
			{
				UserViewModel model = new UserViewModel(user);
				return View(model);
			}
		}

		/// <summary>
		/// Updates the password for a user based for a reset key.
		/// </summary>
		[HttpPost]
		public ActionResult CompleteResetPassword(string id, UserViewModel model)
		{
			if (ApplicationSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			// Don't use ModelState.isvalid as the UserViewModel instance only has an ID and two passwords
			if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.PasswordConfirmation) ||
				model.Password != model.PasswordConfirmation)
			{
				ModelState.Clear();
				ModelState.AddModelError("Passwords", SiteStrings.ResetPassword_Error);
				return View(model);
			}
			else
			{
				RoadkillUser user = UserService.GetUserByResetKey(id);
				if (user != null)
				{
					UserService.ChangePassword(user.Email, model.Password);
					return View("CompleteResetPasswordSuccessful");
				}
				else
				{
					return View("CompleteResetPasswordInvalid");
				}
			}
		}

		/// <summary>
		/// Displays the login page.
		/// </summary>
		/// <remarks>If the session times out in the file manager, then an alternative
		/// login view with no theme is displayed.</remarks>
		public ActionResult Login()
		{
			if (ApplicationSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			// Show a plain login page if the session has ended inside the file explorer/help dialogs
			if (Request.QueryString["ReturnUrl"] != null)
			{
				if (Request.QueryString["ReturnUrl"].ToLower().Contains("/filemanager/select") ||
					Request.QueryString["ReturnUrl"].ToLower().Contains("/help"))
				{
					return View("BlankLogin");
				}
			}

			return View();
		}

		/// <summary>
		/// Handles the login page POST, validates the login and if successful redirects to the url provided.
		/// If the login is unsuccessful, the default Login view is re-displayed.
		/// </summary>
		[HttpPost]
		public ActionResult Login(string email, string password, string fromUrl)
		{
			if (ApplicationSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			string viewName = "Login";

			// Show a plain login page if the session has ended inside the file explorer/help dialogs
			if (Request.QueryString["ReturnUrl"] != null)
			{
				if (Request.QueryString["ReturnUrl"].ToLower().Contains("/filemanager/select") ||
					Request.QueryString["ReturnUrl"].ToLower().Contains("/help"))
				{
					viewName = "BlankLogin";
				}
			}

			if (UserService.Authenticate(email, password))
			{
				Context.CurrentUser = UserService.GetLoggedInUserName(HttpContext);

				if (!string.IsNullOrWhiteSpace(fromUrl))
					return Redirect(fromUrl);
				else
					return RedirectToAction("Index","Home");
			}
			else
			{
				ModelState.AddModelError("Username/Password", SiteStrings.Login_Error);
				return View(viewName);
			}
		}

		/// <summary>
		/// Logouts the current logged in user, and redirects to the homepage.
		/// </summary>
		public ActionResult Logout()
		{
			UserService.Logout();
			return RedirectToAction("Index", "Home");
		}

		/// <summary>
		/// Provides a page for editing the logged in user's profile details.
		/// </summary>
		public ActionResult Profile()
		{
			if (Context.IsLoggedIn)
			{
				UserViewModel model = null;
				if (!ApplicationSettings.UseWindowsAuthentication)
				{
					RoadkillUser user = UserService.GetUserById(new Guid(Context.CurrentUser));
					model = new UserViewModel(user);
				}

				return View(model);
			}
			else
			{
				return RedirectToAction("Login");
			}
		}

		/// <summary>
		/// Updates the POST'd user profile details.
		/// </summary>
		[HttpPost]
		public ActionResult Profile(UserViewModel model)
		{
			if (!Context.IsLoggedIn)
				return RedirectToAction("Login");

			// If the ID (and probably IsNew) have been tampered with in an attempt to create new users, just redirect.
			// We can't set summary.IsNew=false here as it's already been validated.
			if (model.Id == null || model.Id == Guid.Empty)
				return RedirectToAction("Login");

			// Don't allow the logged in user to change someone else's email - throw 403
			// so that it's logged in the server logs.
			if (model.Id.ToString() != Context.CurrentUser)
				return new HttpStatusCodeResult(403, "You cannot change the profile of another user");

#if DEMOSITE
			ModelState.AddModelError("General", "The demo site login cannot be changed.");
#endif

			if (ModelState.IsValid)
			{
				try
				{
					if (UserService.UpdateUser(model))
					{
						model.UpdateSuccessful = true;
					}
					else
					{
						ModelState.AddModelError("General", SiteStrings.Profile_Error);
						model.ExistingEmail = model.NewEmail;
					}

					if (!string.IsNullOrEmpty(model.Password))
					{
						UserService.ChangePassword(model.ExistingEmail, model.Password);
						model.PasswordUpdateSuccessful = true;
					}
				}
				catch (SecurityException e)
				{
					ModelState.AddModelError("General", e.Message);
				}
			}

			return View(model);
		}

		/// <summary>
		/// Displays the reset password view.
		/// </summary>
		public ActionResult ResetPassword()
		{
			if (ApplicationSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			return View();
		}

		/// <summary>
		/// Occurs when the reset password button is clicked, and sends the reset password request email.
		/// </summary>
		/// <param name="email">The email to reset the pasword for</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult ResetPassword(string email)
		{
			if (ApplicationSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

#if DEMOSITE
			ModelState.AddModelError("General", "The demo site login cannot be changed.");
			return View();
#endif

			if (string.IsNullOrEmpty(email))
			{
				// No email
				ModelState.AddModelError("General", SiteStrings.ResetPassword_Error_MissingEmail);
			}
			else
			{
				RoadkillUser user = UserService.GetUser(email);
				if (user == null)
				{
					ModelState.AddModelError("General", SiteStrings.ResetPassword_Error_EmailNotFound);
				}
				else
				{
					string key = UserService.ResetPassword(email);
					if (!string.IsNullOrEmpty(key))
					{
						// Everything worked, send the email
						user.PasswordResetKey = key;
						SiteSettings siteSettings = SettingsService.GetSiteSettings();
						_resetPasswordEmail.Send(new UserViewModel(user));

						return View("ResetPasswordSent",(object) email);
					}
					else
					{
						ModelState.AddModelError("General", SiteStrings.ResetPassword_Error_ServerError);
					}
				}
			}

			return View();
		}	

		/// <summary>
		/// Resends a signup confirmation email, from the signupcomplete page.
		/// </summary>
		[HttpPost]
		public ActionResult ResendConfirmation(string email)
		{
			RoadkillUser user = UserService.GetUser(email, false);

			if (user == null)
			{
				// Something went wrong with the signup, redirect to the first step of the signup.
				return View("Signup");
			}

			UserViewModel model = new UserViewModel(user);

			SiteSettings siteSettings = SettingsService.GetSiteSettings();
			_signupEmail.Send(model);

			TempData["resend"] = true;
			return View("SignupComplete", model);
		}

		/// <summary>
		/// Provides a page for creating a new user account. This redirects to the home page if
		/// windows authentication is enabled, or AllowUserSignup is disabled.
		/// </summary>
		public ActionResult Signup()
		{
			SiteSettings siteSettings = SettingsService.GetSiteSettings();
			if (Context.IsLoggedIn || !siteSettings.AllowUserSignup || ApplicationSettings.UseWindowsAuthentication)
			{
				return RedirectToAction("Index","Home");
			}
			else
			{
				return View();
			}
		}

		/// <summary>
		/// Attempts to create the new user, sending a validation key
		/// </summary>
		[HttpPost]
		[RecaptchaRequired]
		public ActionResult Signup(UserViewModel model, bool? isCaptchaValid)
		{
			SiteSettings siteSettings = SettingsService.GetSiteSettings();
			if (Context.IsLoggedIn || !siteSettings.AllowUserSignup || ApplicationSettings.UseWindowsAuthentication)
				return RedirectToAction("Index","Home");

			if (ModelState.IsValid)
			{
				if (isCaptchaValid.HasValue && isCaptchaValid == false)
				{
					// Invalid recaptcha
					ModelState.AddModelError("General", SiteStrings.Signup_Error_Recaptcha);
				}
				else
				{
					// Everything is valid.
					try
					{
						try
						{
							string key = UserService.Signup(model, null);
							if (string.IsNullOrEmpty(key))
							{
								ModelState.AddModelError("General", SiteStrings.Signup_Error_General);
							}
							else
							{
								// Send the confirm email
								_signupEmail.Send(model);
								return View("SignupComplete", model);
							}
						}
						catch (SecurityException e)
						{
							ModelState.AddModelError("General", e.Message);
						}
					}
					catch (SecurityException e)
					{
						ModelState.AddModelError("General", e.Message);
					}
				}
			}

			return View();
		}

		/// <summary>
		/// Displays the "Logged in As" view (top right for the media wiki theme)
		/// </summary>
		public ActionResult LoggedInAs()
		{
			return PartialView();
		}
	}
}
