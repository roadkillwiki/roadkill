using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides user management with Active Directory.
	/// </summary>
	public class ActiveDirectoryUserManager : UserManager
	{
		// Very simplistic caching.
		private static Dictionary<string, List<string>> _usersInGroupCache = new Dictionary<string, List<string>>();
		private string _connectionString;
		private string _username;
		private string _password;
		private string _editorRolename;
		private string _adminRolename;
		private string _domainName;

		/// <summary>
		/// Returns false as <see cref="ActiveDirectoryUserManager"/> does not support user updates.
		/// </summary>
		public override bool IsReadonly
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ActiveDirectoryUserManager"/> class.
		/// </summary>
		/// <param name="ldapConnectionString">The LDAP connection string.</param>
		/// <param name="username">The username to authenticate against the Active Directory with.</param>
		/// <param name="password">The password to authenticate against the Active Directory with.</param>
		/// <param name="editorGroupName">The name of the group for editors.</param>
		/// <param name="adminGroupName">The name of the group for admins.</param>
		public ActiveDirectoryUserManager(string ldapConnectionString, string username, string password, string editorGroupName, string adminGroupName)
		{
			_connectionString = ldapConnectionString;
			_username = username;
			_password = password;
			_editorRolename = editorGroupName;
			_adminRolename = adminGroupName;

			// Remove the "LDAP://" part for the domain name, as the PrincipleContext doesn't like it.
			int length = "ldap://".Length;
			if (!_connectionString.ToLower().StartsWith("ldap://") || _connectionString.Length < length)
				throw new SecurityException(null,"The LDAP connection string: '{0}' does not appear to be a valid LDAP. A correct connection string example is LDAP://dc=megacorp,dc=com.", _connectionString);

			_domainName = _connectionString.Substring(length);
		}


		/// <summary>
		/// Retrieves a full <see cref="User"/> object for the email address provided, or null if the user doesn't exist.
		/// </summary>
		/// <param name="email">The username of the user to get</param>
		/// <returns>
		/// A <see cref="User"/> object
		/// </returns>
		public override User GetUser(string email)
		{
			return new User()
			{
				Email = email,
				Username = email,
				IsActivated = true,
				IsEditor = IsEditor(email),
				IsAdmin = IsAdmin(email),
			};
		}

		/// <summary>
		/// Determines whether the specified user with the given email/username is an admin.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <returns>
		/// true if the user is an admin; false otherwise.
		/// </returns>
		/// <exception cref="SecurityException">An Active Directory releated error occured while checking the username.email.</exception>
		public override bool IsAdmin(string email)
		{
			try
			{
				List<string> users = GetUsersInGroup(_adminRolename);
				return users.Contains(CleanUsername(email));
			}
			catch (Exception ex)
			{
				throw new SecurityException(ex, "An error occured querying IsAdmin with Active Directory");
			}
		}

		/// <summary>
		/// Determines whether the specified user with the given email/username is an editor.
		/// </summary>
		/// <param name="email">The email address or username of the user.</param>
		/// <returns>
		/// true if the user is an editor; false otherwise.
		/// </returns>
		/// <exception cref="SecurityException">An Active Directory releated error occured while checking the username.email.</exception>
		public override bool IsEditor(string email)
		{
			try
			{
				List<string> users = GetUsersInGroup(_editorRolename);
				return users.Contains(CleanUsername(email));
			}
			catch (Exception ex)
			{
				throw new SecurityException(ex, "An error occured querying IsEditor with Active Directory");
			}
		}

		/// <summary>
		/// Lists all admins in the system.
		/// </summary>
		/// <returns>
		/// A list of email/usernames who belong to the admin group/security group.
		/// </returns>
		public override IEnumerable<UserSummary> ListAdmins()
		{
			List<string> usernames = GetUsersInGroup(_adminRolename);
			List<UserSummary> list = new List<UserSummary>();
			foreach (string editor in usernames)
			{
				list.Add(new UserSummary() { ExistingUsername = editor, ExistingEmail = editor });
			}

			return list;
		}

		/// <summary>
		/// Lists all editors in the system.
		/// </summary>
		/// <returns>
		/// A list of email/usernames wwho belong to the editor group/security group.
		/// </returns>
		public override IEnumerable<UserSummary> ListEditors()
		{
			List<string> usernames = GetUsersInGroup(_editorRolename);
			List<UserSummary> list = new List<UserSummary>();
			foreach (string editor in usernames)
			{
				list.Add(new UserSummary() { ExistingUsername = editor, ExistingEmail = editor });
			}

			return list;
		}

		/// <summary>
		/// Lowercases the username and takes the "john" part from "DOMAIN\john".
		/// </summary>
		private string CleanUsername(string username)
		{
			int start = username.IndexOf(@"\");
			if (start > 0)
			{
				username = username.Substring(start+1);
			}

			username = username.ToLower();
			return username;
		}

		/// <summary>
		/// Retrieve listing of all users in a specified group.
		/// </summary>
		private List<string> GetUsersInGroup(string groupName)
		{
			if (!_usersInGroupCache.ContainsKey(groupName))
			{
				List<string> results = new List<string>();

				// Ensure they're null if they're empty
				if (_username == "")
				{
					_username = null;
					_password = null;
				}

				using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _domainName,_username,_password))
				{
					if (!string.IsNullOrEmpty(_username))
					{
						bool valid = context.ValidateCredentials(_username, _password);
						if (!valid)
							throw new SecurityException(null,"Unable to authenticate with '{0}' using '{1}'", _domainName, _username);
					}

					try
					{
						using (GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, groupName))
						{
							using (PrincipalSearchResult<Principal> users = group.GetMembers())
							{
								foreach (Principal principle in users)
								{
									if (principle is UserPrincipal)
										results.Add(principle.SamAccountName.ToLower());

									principle.Dispose();
								}
							}
						}
					}
					catch (Exception ex)
					{
						throw new SecurityException(ex, "Unable to query Active Directory.");
					}
				}

				_usersInGroupCache.Add(groupName, results);
			}

			return _usersInGroupCache[groupName];
		}

		#region Not implemented methods
		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override bool ActivateUser(string activationKey)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override bool AddUser(string email, string username, string password, bool isAdmin, bool isEditor)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override bool Authenticate(string email, string password)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override void ChangePassword(string email, string newPassword)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override bool ChangePassword(string email, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override bool UpdateUser(UserSummary summary)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override bool DeleteUser(string email)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override User GetUserById(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override User GetUserByResetKey(string resetKey)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override void Logout()
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override string ResetPassword(string email)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override void ToggleAdmin(string email)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override void ToggleEditor(string email)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override string Signup(UserSummary summary, Action completed)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override bool UserExists(string email)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="NotImplementedException">This feature is not available with the <see cref="ActiveDirectoryUserManager"/></exception>
		public override bool UserNameExists(string username)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
