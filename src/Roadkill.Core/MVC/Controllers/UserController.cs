using System;
using System.Web.Mvc;
using System.Web.Security;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Configuration;
using RoadkillUser = Roadkill.Core.Database.User;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.Attributes;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// All actions related to user based tasks.
	/// </summary>
	public class UserController : ControllerBase
	{
		public UserController(ApplicationSettings settings, UserManagerBase userManager,
			IUserContext context, SettingsManager siteSettingsManager)
			: base(settings, userManager, context, siteSettingsManager) 
		{

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

			if (!UserManager.ActivateUser(id))
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

			RoadkillUser user = UserManager.GetUserByResetKey(id);
			
			if (user == null)
			{
				return View("CompleteResetPasswordInvalid");
			}
			else
			{
				UserSummary summary = user.ToSummary();
				return View(summary);
			}
		}

		/// <summary>
		/// Updates the password for a user based for a reset key.
		/// </summary>
		[HttpPost]
		public ActionResult CompleteResetPassword(string id, UserSummary summary)
		{
			if (ApplicationSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			// Don't use ModelState.isvalid as the summary object only has an ID and two passwords
			if (string.IsNullOrEmpty(summary.Password) || string.IsNullOrEmpty(summary.PasswordConfirmation) ||
				summary.Password != summary.PasswordConfirmation)
			{
				ModelState.Clear();
				ModelState.AddModelError("Passwords", SiteStrings.ResetPassword_Error);
				return View(summary);
			}
			else
			{
				RoadkillUser user = UserManager.GetUserByResetKey(id);
				if (user != null)
				{
					UserManager.ChangePassword(user.Email, summary.Password);
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

			// Show a plain login page if the session has ended inside the file explorer dialog
			if (Request.QueryString["ReturnUrl"] != null && Request.QueryString["ReturnUrl"].ToLower().Contains("files"))
				return View("BlankLogin");

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
			if (Request.QueryString["ReturnUrl"] != null && Request.QueryString["ReturnUrl"].ToLower().Contains("files"))
				viewName = "BlankLogin";

			if (UserManager.Authenticate(email, password))
			{
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
			UserManager.Logout();
			return RedirectToAction("Index", "Home");
		}

		/// <summary>
		/// Provides a page for editing the logged in user's profile details.
		/// </summary>
		public ActionResult Profile()
		{
			if (Context.IsLoggedIn)
			{
				UserSummary summary = null;
				if (!ApplicationSettings.UseWindowsAuthentication)
				{
					summary = UserManager.GetUserById(new Guid(Context.CurrentUser)).ToSummary();
				}

				return View(summary);
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
		public ActionResult Profile(UserSummary summary)
		{
			if (!Context.IsLoggedIn)
				return RedirectToAction("Login");

			// If the ID (and probably IsNew) have been tampered with in an attempt to create new users, just redirect.
			// We can't set summary.IsNew=false here as it's already been validated.
			if (summary.Id == null || summary.Id == Guid.Empty)
				return RedirectToAction("Login");

#if DEMOSITE
			ModelState.AddModelError("General", "The demo site login cannot be changed.");
#endif

			if (ModelState.IsValid)
			{
				try
				{
					if (!UserManager.UpdateUser(summary))
					{
						ModelState.AddModelError("General", SiteStrings.Profile_Error);
						summary.ExistingEmail = summary.NewEmail;
					}

					if (!string.IsNullOrEmpty(summary.Password))
						UserManager.ChangePassword(summary.ExistingEmail, summary.Password);
				}
				catch (SecurityException e)
				{
					ModelState.AddModelError("General", e.Message);
				}
			}

			return View(summary);
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
				RoadkillUser user = UserManager.GetUser(email);
				if (user == null)
				{
					ModelState.AddModelError("General", SiteStrings.ResetPassword_Error_EmailNotFound);
				}
				else
				{
					string key = UserManager.ResetPassword(email);
					if (!string.IsNullOrEmpty(key))
					{
						// Everything worked, send the email
						user.PasswordResetKey = key;
						SiteSettings siteSettings = SiteSettingsManager.GetSiteSettings();
						Email.Send(new ResetPasswordEmail(user.ToSummary(), ApplicationSettings, siteSettings));

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
			UserSummary summary = UserManager.GetUser(email, false).ToSummary();
			if (summary == null)
			{
				// Something went wrong with the signup, redirect to the first step of the signup.
				return View("Signup");
			}

			SiteSettings siteSettings = SiteSettingsManager.GetSiteSettings();
			Email.Send(new SignupEmail(summary, ApplicationSettings, siteSettings));

			TempData["resend"] = true;
			return View("SignupComplete", summary);
		}

		/// <summary>
		/// Provides a page for creating a new user account. This redirects to the home page if
		/// windows authentication is enabled, or AllowUserSignup is disabled.
		/// </summary>
		public ActionResult Signup()
		{
			SiteSettings siteSettings = SiteSettingsManager.GetSiteSettings();
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
		public ActionResult Signup(UserSummary summary, bool? isCaptchaValid)
		{
			SiteSettings siteSettings = SiteSettingsManager.GetSiteSettings();
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
							string key = UserManager.Signup(summary, null);
							if (string.IsNullOrEmpty(key))
							{
								ModelState.AddModelError("General", SiteStrings.Signup_Error_General);
							}
							else
							{
								// Send the confirm email
								Email.Send(new SignupEmail(summary, ApplicationSettings, siteSettings));
								return View("SignupComplete", summary);
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

		/// <summary>
		/// Displays the left side menu view, including new page/settings if logged in.
		/// </summary>
		public ActionResult LeftMenu()
		{
			return PartialView();
		}
	}
}
