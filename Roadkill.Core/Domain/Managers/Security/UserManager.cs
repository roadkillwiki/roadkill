using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Web.Configuration;
using System.Configuration;
using System.Data.SqlClient;
using System.Configuration.Provider;
using NHibernate;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides user management using the Roadkill database (via NHibernate).
	/// </summary>
	public class UserManager : UserManagerBase
	{
		/// <summary>
		/// Indicates whether this UserManager can perform deletes, updates or inserts for users.
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
			throw new NotImplementedException("This feature has not been implemented yet");
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
		/// <exception cref="SecurityException">An NHibernate (database) error occured while adding the new user.</exception>
		public override bool AddUser(string email, string password, bool isAdmin, bool isEditor)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email);
				if (user == null)
				{
					user = new User();
					user.Email = email;
					user.SetPassword(password);
					user.IsAdmin = isAdmin;
					user.IsEditor = isEditor;
					user.IsActivated = true;
					NHibernateRepository.Current.SaveOrUpdate<User>(user);

					return true;
				}
				else
				{
					return false;
				}
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured while adding the new user {0}", email);
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
		/// <exception cref="SecurityException">An NHibernate (database) error occured while authenticating the user.</exception>
		public override bool Authenticate(string email, string password)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email);
				if (user != null)
				{
					if (user.Password == User.HashPassword(password, user.Salt))
						return true;
				}

				return false;
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occurred authentication user {0}", email);
			}
		}

		/// <summary>
		/// Changes the password of user with the given email.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <param name="newPassword">The new password.</param>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while changing the password OR the password is empty.</exception>
		public override void ChangePassword(string email, string newPassword)
		{
			try
			{
				if (string.IsNullOrEmpty(newPassword))
					throw new SecurityException(null, "Cannot change the password as it's empty.");

				User user = Users.FirstOrDefault(u => u.Email == email);
				if (user != null)
				{
					user.Salt = new Salt();
					user.SetPassword(newPassword);
					NHibernateRepository.Current.SaveOrUpdate<User>(user);
				}
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured changing the password for {0}", email);
			}
		}

		/// <summary>
		/// Changes the email(username) of user to a new email address.
		/// </summary>
		/// <param name="oldEmail"></param>
		/// <param name="newEmail">The new email/username.</param>
		/// <returns>
		/// true if the change was successful;false if the new email address already exists in the system.
		/// </returns>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while changing the email/username.</exception>
		public override bool ChangeEmail(string oldEmail, string newEmail)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == oldEmail);
				if (user != null)
				{
					user.Email = newEmail;
					NHibernateRepository.Current.SaveOrUpdate<User>(user);

					return true;
				}
				else
				{
					return false;
				}
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured changing the email from {0} to {1}", oldEmail, newEmail);
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
		/// <exception cref="SecurityException">An NHibernate (database) error occured while changing the password OR the new password is empty.</exception>
		public override bool ChangePassword(string email, string oldPassword, string newPassword)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email && u.Password == oldPassword);
				if (user != null)
				{
					if (string.IsNullOrWhiteSpace(newPassword))
					{
						throw new SecurityException(null, "Changing password failed. The new password is blank.");
					}

					user.Salt = new Salt();
					user.SetPassword(newPassword);

					NHibernateRepository.Current.SaveOrUpdate<User>(user);
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured changing the password for {0}", email);
			}
		}

		/// <summary>
		/// Deletes a user with the given email from the system.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <returns>
		/// true if the deletion was successful;false if the user could not be found.
		/// </returns>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while deleting the user.</exception>
		public override bool DeleteUser(string email)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email);
				if (user != null)
				{
					NHibernateRepository.Current.Delete<User>(user);
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured deleting the user with the email {0}", email);
			}
		}

		/// <summary>
		/// Determines whether the specified user with the given email is an admin.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <returns>
		/// true if the user is an admin; false otherwise.
		/// </returns>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while email/username.</exception>
		public override bool IsAdmin(string email)
		{
			try
			{
				return Users.FirstOrDefault(u => u.Email == email && u.IsAdmin) != null;
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured checking if {0} is an admin", email);
			}
		}

		/// <summary>
		/// Determines whether the specified user with the given email is an editor.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <returns>
		/// true if the user is an editor; false otherwise.
		/// </returns>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while checking email/username.</exception>
		public override bool IsEditor(string email)
		{
			try
			{
				return Users.FirstOrDefault(u => u.Email == email && u.IsEditor) != null;
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured checking if {0} is an editor", email);
			}
		}

		/// <summary>
		/// Lists all admins in the system.
		/// </summary>
		/// <returns>
		/// A list of email/usernames who are admins.
		/// </returns>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while getting the admins.</exception>
		public override IEnumerable<string> ListAdmins()
		{
			try
			{
				return from user in Users.Where(u => u.IsAdmin) select user.Email;
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured listing all the admins");
			}
		}

		/// <summary>
		/// Lists all editors in the system.
		/// </summary>
		/// <returns>
		/// A list of email/usernames who are editors.
		/// </returns>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while getting the editors.</exception>
		public override IEnumerable<string> ListEditors()
		{
			try
			{
				return from user in Users.Where(u => u.IsEditor) select user.Email;
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured listing all the editor");
			}
		}

		/// <summary>
		/// Signs the user out with (typically with <see cref="FormsAuthentication"/>).
		/// </summary>
		public override void Logout()
		{
			FormsAuthentication.SignOut();
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
			throw new NotImplementedException("This feature has not been implemented yet");
		}

		/// <summary>
		/// Creates a user in the system without setting the <see cref="User.IsActivated"/>, in other words for a user confirmation email.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <param name="password">The password for the user.</param>
		/// <param name="completed">Called once the signup (e.g. email is sent) is complete. Pass Null for no action.</param>
		/// <returns>
		/// The activation key for the signup.
		/// </returns>
		public override string Signup(string email, string password, Action completed)
		{
			throw new NotImplementedException("This feature has not been implemented yet");
		}

		/// <summary>
		/// Adds or remove the user with the email address as an editor.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while setting the user to an editor.</exception>
		public override void ToggleEditor(string email)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email);
				if (user != null)
				{
					user.IsEditor = true;
					NHibernateRepository.Current.SaveOrUpdate<User>(user);
				}
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured adding the editor {0}", email);
			}
		}

		/// <summary>
		/// Adds or remove the user with the email address as an admin.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while setting the user to an admin.</exception>
		public override void ToggleAdmin(string email)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email);
				if (user != null)
				{
					user.IsAdmin = true;
					NHibernateRepository.Current.SaveOrUpdate<User>(user);
				}
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured adding the admin {0}", email);
			}
		}

		/// <summary>
		/// Determines whether the user with the given email exists.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <returns>
		/// true if the user exists;false otherwise.
		/// </returns>
		/// <exception cref="SecurityException">An NHibernate (database) error occured while checking the email/user.</exception>
		public override bool UserExists(string email)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email);
				return (user != null);
			}
			catch (HibernateException ex)
			{
				throw new SecurityException(ex, "An error occured checking if user {0} exists", email);
			}
		}
	}
}
