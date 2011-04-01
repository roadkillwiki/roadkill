using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace Roadkill.Core
{
	public class ActiveDirectoryUserManager : IUserManager
	{
		private ActiveDirectoryUserManager()
		{
		}

		#region IUserManager Members

		public bool AddUser(string email, string password, bool isAdmin, bool isEditor)
		{
			throw new NotImplementedException();
		}

		public void AddAdmin(string email, string password)
		{
			throw new NotImplementedException();
		}

		public void AddEditor(string email, string password)
		{
			throw new NotImplementedException();
		}

		public bool Authenticate(string email, string password)
		{
			throw new NotImplementedException();
		}

		public bool ChangeEmail(string oldEmail, string newEmail)
		{
			throw new NotImplementedException();
		}

		public void ChangePassword(string email, string newPassword)
		{
			throw new NotImplementedException();
		}

		public bool ChangePassword(string email, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		public bool DeleteUser(string email)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> ListAdmins()
		{
			return GetUsersInRole(_adminRolename);
		}

		public IEnumerable<string> ListEditors()
		{
			return GetUsersInRole(_editorRolename);
		}

		public void Logout()
		{
			throw new NotImplementedException();
		}

		public bool UserExists(string email)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Determine if a specified user is in a specified role.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="rolename"></param>
		/// <returns>Boolean indicating membership</returns>
		public bool IsUserAdmin(string email)
		{
			List<string> users = GetUsersInRole(_adminRolename);
			return users.Contains(email);
		}

		public bool IsUserEditor(string email)
		{
			List<string> users = GetUsersInRole(_editorRolename);
			return users.Contains(email);
		}
		#endregion

		// Very simplistic caching.
		private static Dictionary<string, List<string>> _usersInRoleCache = new Dictionary<string, List<string>>();
		private static Dictionary<string, List<string>> _rolesForUserCache = new Dictionary<string, List<string>>();

		private string _connectionString;
		private string _username;
		private string _password;
		private string _editorRolename;
		private string _adminRolename;

		/// <summary>
		/// Support cross-domain querying, for example if you are in another forest (i.e. office) this is important.
		/// </summary>
		private string _domainName;

		public ActiveDirectoryUserManager(string connectionString,string username,string password,string editorRolename,string adminRolename)
		{
			_connectionString = connectionString;
			_username = username;
			_password = password;
			_editorRolename = editorRolename;
			_adminRolename = adminRolename;

			// Remove the "LDAP://" part for the daomin name, as the PrincipleContext doesn't like it.
			int length = "ldap://".Length;
			if (!_connectionString.ToLower().StartsWith("ldap://") || _connectionString.Length < length)
				throw new UserException(string.Format("The LDAP connection string: '{0}' does not appear to be a valid LDAP. A correct connection string example is LDAP://dc=megacorp,dc=com.", _connectionString));

			_domainName = _connectionString.Substring(length);
		}

		/// <summary>
		/// Retrieve listing of all users in a specified role.
		/// </summary>
		/// <param name="rolename">string array of users</param>
		/// <returns></returns>
		private List<string> GetUsersInRole(string rolename)
		{
			if (!_usersInRoleCache.ContainsKey(rolename))
			{
				List<string> results = new List<string>();
				using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _domainName, _username, _password))
				{

					try
					{
						using (GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, rolename))
						{
							using (PrincipalSearchResult<Principal> users = group.GetMembers())
							{
								foreach (Principal principle in users)
								{
									if (principle is UserPrincipal)
										results.Add(principle.SamAccountName);

									principle.Dispose();
								}
							}
						}
					}
					catch (Exception)
					{
						throw new UserException("Unable to query Active Directory.");
					}
				}

				_usersInRoleCache.Add(rolename, results);
			}

			return _usersInRoleCache[rolename];
		}
	}
}
