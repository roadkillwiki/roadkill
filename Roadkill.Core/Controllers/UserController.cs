using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using Roadkill.Core.Search;
using System.ComponentModel.DataAnnotations;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// All actions related to user based tasks.
	/// </summary>
	public class UserController : ControllerBase
    {
		/// <summary>
		/// Activates a newly registered account.
		/// </summary>
		/// <returns></returns>
		public ActionResult Activate(string id)
		{
			if (string.IsNullOrEmpty(id))
				RedirectToAction("Index", "Home");

			if (!UserManager.Current.ActivateUser(id))
			{
				ModelState.AddModelError("General", "There was a problem activating your account. It may have already been activated.");
			}

			return View();
		}

		/// <summary>
		/// Displays the password text boxes for a password reset request.
		/// </summary>
		public ActionResult CompleteResetPassword(string id)
		{
			if (RoadkillSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			User user = UserManager.Current.GetUserByResetKey(id);
			
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
			if (RoadkillSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			// Don't use ModelState.isvalid as the summary object only has an ID and two passwords
			if (UserSummary.VerifyPassword(summary,null) != ValidationResult.Success || 
				UserSummary.VerifyPasswordsMatch(summary,null) != ValidationResult.Success)
			{
				ModelState.Clear();
				ModelState.AddModelError("Passwords", "The passwords do not match, or are too short");
				return View(summary);
			}
			else
			{
				User user = UserManager.Current.GetUserByResetKey(id);
				if (user != null)
				{
					UserManager.Current.ChangePassword(user.Email, summary.Password);
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
			// Show a plain login page if the session has ended inside the file explorer dialog
			if (Request.QueryString["ReturnUrl"] != null && Request.QueryString["ReturnUrl"].Contains("Files"))
				return View("BlankLogin");

			return View();
		}

		/// <summary>
		/// Handles the login page POST, validates the login and if successful redirects to the url provided.
		/// If the login is unsucessful, the default Login view is re-displayed.
		/// </summary>
		[HttpPost]
		public ActionResult Login(string username, string password, string fromUrl)
		{
			if (UserManager.Current.Authenticate(username, password))
			{
				FormsAuthentication.SetAuthCookie(username, true);

				if (!string.IsNullOrWhiteSpace(fromUrl))
					return Redirect(fromUrl);
				else
					return RedirectToAction("Index","Home");
			}
			else
			{
				ModelState.AddModelError("Username/Password", "The username/password are incorrect");
				return View();
			}
		}

		/// <summary>
		/// Logouts the current logged in user, and redirects to the homepage.
		/// </summary>
		public ActionResult Logout()
		{
			UserManager.Current.Logout();
			return RedirectToAction("Index","Home");
		}

		/// <summary>
		/// Provides a page for editing the logged in user's profile details.
		/// </summary>
		public ActionResult Profile()
		{
			if (RoadkillContext.Current.IsLoggedIn)
			{
				UserSummary summary = UserManager.Current.GetUser(RoadkillContext.Current.CurrentUser).ToSummary();
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
			if (!RoadkillContext.Current.IsLoggedIn)
				return RedirectToAction("Login");

			if (ModelState.IsValid)
			{
				try
				{
					if (!UserManager.Current.UpdateUser(summary))
					{
						ModelState.AddModelError("General", "An error occured updating your profile");

						summary.ExistingEmail = summary.NewEmail;
					}

					if (!string.IsNullOrEmpty(summary.Password))
						UserManager.Current.ChangePassword(summary.ExistingEmail, summary.Password);
				}
				catch (SecurityException e)
				{
					ModelState.AddModelError("General", e.Message);
				}
			}

			return View();
		}

		/// <summary>
		/// Displays the reset password view.
		/// </summary>
		public ActionResult ResetPassword()
		{
			if (RoadkillSettings.UseWindowsAuthentication)
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
			if (RoadkillSettings.UseWindowsAuthentication)
				return RedirectToAction("Index", "Home");

			if (string.IsNullOrEmpty(email))
			{
				// No email
				ModelState.AddModelError("General", "Please enter an email address");
			}
			else
			{	
				UserSummary summary = UserManager.Current.GetUser(email).ToSummary();
				if (summary == null)
				{
					ModelState.AddModelError("General", "The email address could not be found");
				}
				else
				{
					string key = UserManager.Current.ResetPassword(email);
					if (!string.IsNullOrEmpty(key))
					{
						// Everything worked, send the email
						summary.PasswordResetKey = key;
						Email.Send(new ResetPasswordEmail(summary));
						return View("ResetPasswordSent",(object) email);
					}
					else
					{
						ModelState.AddModelError("General", "The password reset failed from a server error.");
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
			UserSummary summary = UserManager.Current.GetUser(email).ToSummary();
			if (summary == null)
			{
				// Something went wrong with the signup, redirect to the first step of the signup.
				return View("Signup");
			}

			Email.Send(new SignupEmail(summary));
			TempData["resend"] = true;
			return View("SignupComplete", summary);
		}

		/// <summary>
		/// Searches the lucene index using the search string provided.
		/// </summary>
		public ActionResult Search(string q)
		{
			ViewData["search"] = q;

			List<SearchResult> results = SearchManager.Current.SearchIndex(q);
			return View(results);
		}

		/// <summary>
		/// Provides a page for creating a new user account. This redirects to the home page if
		/// windows authentication is enabled, or AllowUserSignup is disabled.
		/// </summary>
		public ActionResult Signup()
		{
			if (RoadkillContext.Current.IsLoggedIn || !RoadkillSettings.AllowUserSignup || RoadkillSettings.UseWindowsAuthentication)
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
			if (RoadkillContext.Current.IsLoggedIn || !RoadkillSettings.AllowUserSignup || RoadkillSettings.UseWindowsAuthentication)
				return RedirectToAction("Index","Home");

			if (ModelState.IsValid)
			{
				if (isCaptchaValid.HasValue && isCaptchaValid == false)
				{
					// Invalid recaptcha
					ModelState.AddModelError("General", "The two words for the anti-spam box were incorrect");
				}
				else
				{
					// Everything is valid.
					try
					{
						try
						{
							string key = UserManager.Current.Signup(summary, null);
							if (string.IsNullOrEmpty(key))
							{
								ModelState.AddModelError("General", "An error occured with the signup.");
							}
							else
							{
								// Send the confirm email
								Email.Send(new SignupEmail(summary));
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
    }
}
