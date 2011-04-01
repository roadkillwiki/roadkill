using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Web.Configuration;
using System.Configuration;
using System.Data.SqlClient;
using System.Configuration.Provider;
using Roadkill.Core.Domain.Managers;
using NHibernate;

namespace Roadkill.Core
{
	public class UserManager : ManagerBase, IUserManager
	{
		private static IUserManager _current;
		public static IUserManager Current
		{
			get { return _current; }
		}

		private UserManager()
		{
		}

		static UserManager()
		{
			if (RoadkillSettings.IsWindowsAuthentication)
			{
				_current = new UserManager();
			}
			else
			{
				_current = new ActiveDirectoryUserManager(RoadkillSettings.ConnectionString, 
																RoadkillSettings.LdapUsername,
																RoadkillSettings.LdapPassword, 
																RoadkillSettings.EditorRoleName, 
																RoadkillSettings.AdminRoleName);
			}
		}

		public bool AddUser(string email, string password, bool isAdmin, bool isEditor)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email);
				if (user != null)
				{
					user = new User();
					user.Salt = new Salt();
					user.Password = User.HashPassword(password, user.Salt);
					user.IsAdmin = isAdmin;
					user.IsEditor = isEditor;
					NHibernateRepository.Current.SaveOrUpdate<User>(user);

					return true;
				}
				else
				{
					return false;
				}
			}
			catch (HibernateException)
			{
				throw new UserException("An error occured changing the password for {0}", email);
			}
		}

		public void AddEditor(string email, string password)
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
			catch (HibernateException)
			{
				throw new UserException("An error occured adding the editor {0}", email);
			}
		}

		public void AddAdmin(string email, string password)
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
			catch (HibernateException)
			{
				throw new UserException("An error occured adding the admin {0}", email);
			}
		}

		public bool Authenticate(string email,string password)
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
			catch (HibernateException)
			{
				throw new UserException("An error occurred authentication user {0}", email);
			}
		}

		public void ChangePassword(string email, string newPassword)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email);
				if (user != null)
				{
					user.Salt = new Salt();
					user.Password = User.HashPassword(newPassword, user.Salt);
					NHibernateRepository.Current.SaveOrUpdate<User>(user);
				}
			}
			catch (HibernateException)
			{
				throw new UserException("An error occured changing the password for {0}", email);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldEmail"></param>
		/// <param name="newEmail"></param>
		/// <returns>True if the change was successful, false if the new email address already exists.</returns>
		public bool ChangeEmail(string oldEmail, string newEmail)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == newEmail);
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
			catch (HibernateException)
			{
				throw new UserException("An error occured changing the email from {0} to {1}", oldEmail,newEmail);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="email"></param>
		/// <param name="oldPassword"></param>
		/// <param name="newPassword"></param>
		/// <returns>Returns true if the user exists and the password was changed.</returns>
		public bool ChangePassword(string email, string oldPassword, string newPassword)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email && u.Password == oldPassword);
				if (user != null)
				{
					if (string.IsNullOrWhiteSpace(newPassword))
					{
						throw new UserException("Changing password failed. The new password is blank.");
					}

					user.Salt = new Salt();
					user.Password = User.HashPassword(newPassword, user.Salt);

					NHibernateRepository.Current.SaveOrUpdate<User>(user);
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (HibernateException)
			{
				throw new UserException("An error occured changing the password for {0}", email);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="email"></param>
		/// <returns>True if the delete was successful, false if the new email address already exists.</returns>
		public bool DeleteUser(string email)
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
			catch (HibernateException)
			{
				throw new UserException("An error occured deleting the user with the email {0}", email);
			}
		}

		public IEnumerable<string> ListAdmins()
		{
			try
			{
				return from user in Users.Where(u => u.IsAdmin) select user.Email;
			}
			catch (HibernateException)
			{
				throw new UserException("An error occured listing all the admins");
			}
		}

		public IEnumerable<string> ListEditors()
		{
			try
			{
				return from user in Users.Where(u => u.IsEditor) select user.Email;
			}
			catch (HibernateException)
			{
				throw new UserException("An error occured listing all the editor");
			}
		}

		public void Logout()
		{
			FormsAuthentication.SignOut();
		}

		public bool UserExists(string email)
		{
			try
			{
				User user = Users.FirstOrDefault(u => u.Email == email);
				return (user != null);
			}
			catch (HibernateException)
			{
				throw new UserException("An error occured checking if user {0} exists", email);
			}
		}

		public bool IsUserAdmin(string email)
		{
			try
			{
				return Users.FirstOrDefault(u => u.Email == email && u.IsAdmin) != null;
			}
			catch (HibernateException)
			{
				throw new UserException("An error occured checking if {0} is an admin",email);
			}
		}

		public bool IsUserEditor(string email)
		{
			try
			{
				return Users.FirstOrDefault(u => u.Email == email && u.IsEditor) != null;
			}
			catch (HibernateException)
			{
				throw new UserException("An error occured checking if {0} is an editor", email);
			}
		}
	}
}
