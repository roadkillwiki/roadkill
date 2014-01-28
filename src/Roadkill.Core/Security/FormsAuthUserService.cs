using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Web;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Security
{
	/// <summary>
	/// Provides user management using the Roadkill datastore and the current repository.
	/// </summary>
	public class FormsAuthUserService : UserServiceBase
	{
		public FormsAuthUserService(ApplicationSettings settings, IRepository repository)
			: base(settings, repository)
		{
		}

		/// <summary>
		/// Indicates whether this UserService can perform deletes, updates or inserts for Repository.Users.
		/// </summary>
		public override bool IsReadonly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Activates the user with given activation key
		/// </summary>
		/// <param name="activationKey">The randomly generated activation key for the user.</param>
		/// <returns>True if the activation was successful</returns>
		public override bool ActivateUser(string activationKey)
		{
			try
			{
				User user = Repository.GetUserByActivationKey(activationKey);
				if (user != null)
				{
					user.IsActivated = true;
					Repository.SaveOrUpdateUser(user);

					return true;
				}
				else
				{
					return false;
				}
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred while activating the user with key {0}", activationKey);
			}
		}

		/// <summary>
		/// Adds a user to the system, and sets the <see cref="User.IsActivated"/> to true.
		/// </summary>
		/// <param name="email">The email or username.</param>
		/// <param name="password">The password.</param>
		/// <param name="isAdmin">if set to <c>true</c> the user is added as an admin.</param>
		/// <param name="isEditor">if set to <c>true</c> the user is added as an editor.</param>
		/// <returns>
		/// true if the user was added; false if the user already exists.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while adding the new user.</exception>
		public override bool AddUser(string email, string username, string password, bool isAdmin, bool isEditor)
		{
			try
			{
				User user = Repository.GetUserByUsernameOrEmail(username, email);
				if (user == null)
				{
					user = new User();
					user.Email = email;
					user.Username = username;
					user.SetPassword(password);
					user.IsAdmin = isAdmin;
					user.IsEditor = isEditor;
					user.IsActivated = true;
					Repository.SaveOrUpdateUser(user);

					return true;
				}
				else
				{
					return false;
				}
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred while adding the new user {0}", email);
			}
		}

		/// <summary>
		/// Authenticates the user with the specified email.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <param name="password">The password.</param>
		/// <returns>
		/// true if the authentication was sucessful;false otherwise.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while authenticating the user.</exception>
		public override bool Authenticate(string email, string password)
		{
			try
			{
				User user = Repository.GetUserByEmail(email);
				if (user != null)
				{
					if (user.Password == User.HashPassword(password, user.Salt))
					{
						bool isFormsAuthEnabled = FormsAuthenticationWrapper.IsEnabled();
						if (isFormsAuthEnabled)
						{
							FormsAuthentication.SetAuthCookie(user.Id.ToString(), true);
						}

						return true;
					}
				}

				return false;
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred authentication user {0}", email);
			}
		}

		/// <summary>
		/// Changes the password of a user with the given email.
		/// </summary>
		/// <param name="email">The email address of the user.</param>
		/// <param name="newPassword">The new password.</param>
		/// <exception cref="SecurityException">An databaseerror occurred while changing the password OR the password is empty.</exception>
		public override void ChangePassword(string email, string newPassword)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(newPassword))
					throw new SecurityException(null, "Cannot change the password as it's empty.");

				User user = Repository.GetUserByEmail(email);
				if (user != null)
				{
					user.Salt = new Salt();
					user.SetPassword(newPassword);
					Repository.SaveOrUpdateUser(user);
				}
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred changing the password for {0}", email);
			}
		}

		/// <summary>
		/// Changes the password of the user with the given email, authenticating their password first..
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <param name="oldPassword">The old password.</param>
		/// <param name="newPassword">The new password to change to.</param>
		/// <returns>
		/// true if the password change was successful;false if the previous password was incorrect.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while changing the password OR the new password is empty.</exception>
		public override bool ChangePassword(string email, string oldPassword, string newPassword)
		{
			try
			{
				if (!Authenticate(email, oldPassword))
					return false;

				User user = Repository.GetUserByEmail(email);
				if (user != null)
				{
					if (string.IsNullOrWhiteSpace(newPassword))
					{
						throw new SecurityException(null, "Changing password failed. The new password is blank.");
					}

					user.Salt = new Salt();
					user.SetPassword(newPassword);

					Repository.SaveOrUpdateUser(user);
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred changing the password for {0}", email);
			}
		}

		/// <summary>
		/// Deletes a user with the given email from the system.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <returns>
		/// true if the deletion was successful;false if the user could not be found.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while deleting the user.</exception>
		public override bool DeleteUser(string email)
		{
			try
			{
				User user = Repository.GetUserByEmail(email);
				if (user != null)
				{
					Repository.DeleteUser(user);
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred deleting the user with the email {0}", email);
			}
		}

		/// <summary>
		/// Retrieves a full <see cref="User"/> object using the unique ID provided.
		/// </summary>
		/// <param name="id">The ID of the user.</param>
		/// <returns>A <see cref="User"/> object</returns>
		public override User GetUserById(Guid id, bool? isActivated = null)
		{
			return Repository.GetUserById(id, isActivated);
		}

		/// <summary>
		/// Retrieves a full <see cref="User"/> object for the email address provided, or null if the user doesn't exist.
		/// </summary>
		/// <param name="email">The email address of the user to get</param>
		/// <returns>A <see cref="User"/> object</returns>
		public override User GetUser(string email, bool? isActivated = null)
		{
			return Repository.GetUserByEmail(email, isActivated);
		}

		/// <summary>
		/// Retrieves a full <see cref="User"/> object for a password reset request.
		/// </summary>
		/// <param name="resetKey"></param>
		/// <returns>A <see cref="User"/> object</returns>
		public override User GetUserByResetKey(string resetKey)
		{
			return Repository.GetUserByPasswordResetKey(resetKey);
		}

		/// <summary>
		/// Determines whether the specified user with the given email is an admin.
		/// </summary>
		/// <param name="cookieValue">The user id or username of the user.</param>
		/// <returns>
		/// true if the user is an admin; false otherwise.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while email/username.</exception>
		public override bool IsAdmin(string cookieValue)
		{
			Guid id;
			if (Guid.TryParse(cookieValue, out id))
			{
				return Repository.GetAdminById(id) != null;
			}
			else
			{
				// Work-around for 1.5's breaking change that changed the cookie to store the ID instead of username.
				Logout();
				return false;

				//throw new SecurityException("The user's cookie value does not contain a Guid when checking for admin rights.", null);
			}
		}

		/// <summary>
		/// Determines whether the specified user with the given email is an editor.
		/// </summary>
		/// <param name="cookieValue">The user id or username of the user.</param>
		/// <returns>
		/// true if the user is an editor; false otherwise.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while checking email/username.</exception>
		public override bool IsEditor(string cookieValue)
		{
			try
			{
				Guid id;
				if (Guid.TryParse(cookieValue, out id))
				{
					return Repository.GetEditorById(id) != null;
				}
				else
				{
					Logout();
					return false;
					throw new SecurityException("The user's cookie value does not contain a Guid when checking for editor rights.", null);
				}
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred checking if {0} is an editor", cookieValue);
			}
		}

		/// <summary>
		/// Lists all admins in the system.
		/// </summary>
		/// <returns>
		/// A list of email/usernames who are admins.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while getting the admins.</exception>
		public override IEnumerable<UserViewModel> ListAdmins()
		{
			try
			{
				var users = Repository.FindAllAdmins().Select(u => new UserViewModel(u));
				return users;
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred listing all the admins");
			}
		}

		/// <summary>
		/// Lists all editors in the system.
		/// </summary>
		/// <returns>
		/// A list of email/usernames who are editors.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while getting the editors.</exception>
		public override IEnumerable<UserViewModel> ListEditors()
		{
			try
			{
				var users = Repository.FindAllEditors().Select(u => new UserViewModel(u));
				return users;
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred listing all the editor");
			}
		}

		/// <summary>
		/// Signs the user out with (typically with <see cref="FormsAuthentication"/>).
		/// </summary>
		public override void Logout()
		{
			bool isFormsAuthEnabled = FormsAuthenticationWrapper.IsEnabled();
			if (isFormsAuthEnabled)
			{
				FormsAuthentication.SignOut();
			}
		}

		/// <summary>
		/// Resets the password for the user with the given email.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <returns>
		/// The new randomly generated password
		/// </returns>
		public override string ResetPassword(string email)
		{
			if (string.IsNullOrEmpty(email))
				throw new SecurityException("The email provided to ResetPassword is null or empty.", null);

			try
			{
				User user = GetUser(email);

				if (user != null)
				{
					user.PasswordResetKey = Guid.NewGuid().ToString();
					Repository.SaveOrUpdateUser(user);

					return user.PasswordResetKey;
				}
				else
				{
					return "";
				}
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred with resetting the password of {0}", email);
			}
		}

		/// <summary>
		/// Creates a user in the system without setting the <see cref="User.IsActivated"/>, in other words for a user confirmation email.
		/// </summary>
		/// <param name="user">The user details to signup.</param>
		/// <param name="completed">Called once the signup (e.g. email is sent) is complete. Pass Null for no action.</param>
		/// <returns>
		/// The activation key for the signup.
		/// </returns>
		public override string Signup(UserViewModel model, Action completed)
		{
			if (model == null)
				throw new SecurityException("The summary provided to Signup is null.", null);

			try
			{
				// Create the new user
				model.ActivationKey = Guid.NewGuid().ToString();
				User user = new User();
				user.Username = model.NewUsername;
				user.ActivationKey = model.ActivationKey;
				user.Email = model.NewEmail;
				user.Firstname = model.Firstname;
				user.Lastname = model.Lastname;
				user.SetPassword(model.Password);
				user.IsEditor = true;
				user.IsAdmin = false;
				user.IsActivated = false;
				Repository.SaveOrUpdateUser(user);

				if (completed != null)
					completed();

				return user.ActivationKey;
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred with the signup of {0}", model.NewEmail);
			}
		}

		/// <summary>
		/// Adds or remove the user with the email address as an editor.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <exception cref="SecurityException">An databaseerror occurred while setting the user to an editor.</exception>
		public override void ToggleEditor(string email)
		{
			try
			{
				User user = Repository.GetUserByEmail(email);
				if (user != null)
				{
					user.IsEditor = !user.IsEditor;
					Repository.SaveOrUpdateUser(user);
				}
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred adding the editor {0}", email);
			}
		}

		/// <summary>
		/// Adds or remove the user with the email address as an admin.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <exception cref="SecurityException">An databaseerror occurred while setting the user to an admin.</exception>
		public override void ToggleAdmin(string email)
		{
			try
			{
				User user = Repository.GetUserByEmail(email);
				if (user != null)
				{
					user.IsAdmin = !user.IsAdmin;
					Repository.SaveOrUpdateUser(user);
				}
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred adding the admin {0}", email);
			}
		}

		/// <summary>
		/// Changes the username of a user to a new username.
		/// </summary>
		/// <param name="model">The user details to change. The password property is ignored for this object - use ChangePassword instead.</param>
		/// <returns>
		/// true if the change was successful;false if the new username already exists in the system.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while changing the email/username.</exception>
		public override bool UpdateUser(UserViewModel model)
		{
			try
			{
				User user;

				// These checks are run in the UserViewModel object by MVC - but doubled up in here for _when_ the API is used without MVC.
				if (model.ExistingEmail != model.NewEmail)
				{
					user = Repository.GetUserByEmail(model.NewEmail);
					if (user != null)
						throw new SecurityException(null, "The email provided already exists.");
				}

				if (model.ExistingUsername != model.NewUsername)
				{
					user = Repository.GetUserByUsername(model.NewUsername);
					if (user != null)
						throw new SecurityException(null, "The username provided already exists.");
				}

				user = Repository.GetUserById(model.Id.Value);
				if (user == null)
					throw new SecurityException(null, "The user does not exist.");

				// Update the profile details
				user.Firstname = model.Firstname;
				user.Lastname = model.Lastname;
				Repository.SaveOrUpdateUser(user);

				// Save the email
				if (model.ExistingEmail != model.NewEmail)
				{
					user = Repository.GetUserByEmail(model.ExistingEmail);
					if (user != null)
					{
						user.Email = model.NewEmail;
						Repository.SaveOrUpdateUser(user);
					}
					else
					{
						return false;
					}
				}

				// Save the username
				if (model.ExistingUsername != model.NewUsername)
				{
					user = Repository.GetUserByUsername(model.ExistingUsername);
					if (user != null)
					{
						user.Username = model.NewUsername;
						Repository.SaveOrUpdateUser(user);

						//
						// Update the PageContent.EditedBy history
						//
						IList<PageContent> pageContents = Repository.FindPageContentsEditedBy(model.ExistingUsername).ToList();
						for (int i = 0; i < pageContents.Count; i++)
						{
							pageContents[i].EditedBy = model.NewUsername;
							Repository.UpdatePageContent(pageContents[i]);
						}

						//
						// Update all Page.CreatedBy and Page.ModifiedBy
						//
						IList<Page> pages = Repository.FindPagesCreatedBy(model.ExistingUsername).ToList();
						for (int i = 0; i < pages.Count; i++)
						{
							pages[i].CreatedBy = model.NewUsername;
							Repository.SaveOrUpdatePage(pages[i]);
						}

						pages = Repository.FindPagesModifiedBy(model.ExistingUsername).ToList();
						for (int i = 0; i < pages.Count; i++)
						{
							pages[i].ModifiedBy = model.NewUsername;
							Repository.SaveOrUpdatePage(pages[i]);
						}
					}
					else
					{
						return false;
					}
				}

				return true;
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred updating the user {0} ", model.ExistingEmail);
			}
		}

		/// <summary>
		/// Determines whether the user with the given email exists.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <returns>
		/// true if the user exists;false otherwise.
		/// </returns>
		/// <exception cref="SecurityException">An databaseerror occurred while checking the email/user.</exception>
		public override bool UserExists(string email)
		{
			try
			{
				User user = Repository.GetUserByEmail(email);
				return (user != null);
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred checking if user email {0} exists", email);
			}
		}


		/// <summary>
		/// Determines whether the user with the given username exists.
		/// </summary>
		/// <param name="username"></param>
		/// <returns>
		/// true if the user exists;false otherwise.
		/// </returns>
		public override bool UserNameExists(string username)
		{
			try
			{
				User user = Repository.GetUserByUsername(username);
				return (user != null);
			}
			catch (DatabaseException ex)
			{
				throw new SecurityException(ex, "An error occurred checking if username {0} exists", username);
			}
		}

		/// <summary>
		/// Gets the current username by decrypting the cookie. If FormsAuthentication is disabled or
		/// there is no logged in user, this returns an empty string.
		/// </summary>
		public override string GetLoggedInUserName(HttpContextBase context)
		{
			if (context == null || context.Request == null || context.Request.Cookies == null)
				return "";

			bool isFormsAuthEnabled = FormsAuthenticationWrapper.IsEnabled();

			if (isFormsAuthEnabled)
			{
				string cookieName = FormsAuthenticationWrapper.CookieName();
				if (!string.IsNullOrEmpty(cookieName) && context.Request.Cookies[cookieName] != null)
				{
					string cookie = context.Request.Cookies[cookieName].Value;
					if (!string.IsNullOrEmpty(cookie))
					{
						FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie);
						if (ticket != null)
							return ticket.Name;
					}
				}
			}

			return "";
		}

		/// <summary>
		/// Gets the currently logged in user, based off the cookie or HttpContext user identity value set during authentication. 
		/// The value for FormsAuthentication is the user's Guid id.
		/// </summary>
		/// <param name="cookieValue">The user id stored in the cookie.</param>
		/// <returns>A new <see cref="User"/> object</returns>
		public override User GetLoggedInUser(string cookieValue)
		{
			Guid userId;

			// Guids are used for the FormsAuth cookies
			if (Guid.TryParse(cookieValue, out userId) && userId != Guid.Empty)
			{
				return GetUserById(userId);
			}
			else
			{
				return null;
			}
		}
	}
}
