using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Web.Hosting;
using System.Web.Security;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Collections.Generic;
using System.Web.Caching;
using System.Web;
using System.Linq;

namespace Roadkill.Core
{
	/// <summary>
	/// A RoleProvider for ActiveDirectory.
	/// </summary>
	/// <remarks>
	/// Based off an original by "Daniel_PS": http://www.codeproject.com/KB/aspnet/active_directory_roles.aspx
	/// </remarks>
	public sealed class ActiveDirectoryRoleProvider : RoleProvider
	{
		// Very simplistic caching. This can be improved in later versions.
		private static Dictionary<string, List<string>> _usersInRoleCache = new Dictionary<string, List<string>>();
		private static Dictionary<string, List<string>> _rolesForUserCache = new Dictionary<string, List<string>>();

		private string _connectionString;
		private string _username;
		private string _password;

		/// <summary>
		/// Support cross-domain querying, for example if you are in another forest (i.e. office) this is important.
		/// </summary>
		private string _domainName;

		public override string ApplicationName
		{
			get { return "roadkill"; }
			set { throw new NotSupportedException("The application name cannot be changed from 'Roadkill'"); }
		}

		/// <summary>
		/// Initialize ActiveDirectoryRoleProvider with config values
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public override void Initialize(string name, NameValueCollection config)
		{
			// Initialize values from web.config.
			if (config == null)
				throw new ArgumentNullException("config");

			// Initialize the abstract base class.
			base.Initialize(name, config);

			// Optional username and password fields
			_username = config["connectionUsername"];
			_password = config["connectionPassword"];

			if (string.IsNullOrEmpty(_username))
			{
				_username = null;
				_password = null;
			}

			// Check the activeDirectoryConnectionstring attribute is valid
			string connectionStringName = config["connectionStringName"];
			if (string.IsNullOrEmpty(connectionStringName))
				throw new ProviderException("The attribute 'connectionStringName' is missing or empty.");

			// Check the connection string
			if (ConfigurationManager.ConnectionStrings[connectionStringName] == null)
				throw new ProviderException(string.Format("The connection name '{0}' could not be found.",connectionStringName));

			_connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
			if (string.IsNullOrEmpty(_connectionString))
				throw new ProviderException(string.Format("The connection string named '{0}' is empty.", connectionStringName));

			// Remove the "LDAP://" part for the daomin name, as the PrincipleContext doesn't like it.
			int length = "ldap://".Length;
			if (!_connectionString.ToLower().StartsWith("ldap://") || _connectionString.Length < length)
				throw new ProviderException(string.Format("The LDAP connection string: '{0}' does not appear to be a valid LDAP. A correct connection string example is LDAP://dc=megacorp,dc=com.", _connectionString));

			_domainName = _connectionString.Substring(length);
		}

		/// <summary>
		/// Retrieve listing of all roles to which a specified user belongs.
		/// </summary>
		/// <param name="username"></param>
		/// <returns>string array of roles</returns>
		public override string[] GetRolesForUser(string username)
		{
			if (!_rolesForUserCache.ContainsKey(username))
			{
				List<string> results = new List<string>();
				using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _domainName, _username, _password))
				{
					// TODO: throw
					if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
						context.ValidateCredentials(_username, _password);

					try
					{
						using (UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username))
						{
							using (PrincipalSearchResult<Principal> groups = user.GetGroups())
							{
								foreach (Principal principle in groups)
								{
									if (principle is GroupPrincipal)
										results.Add(principle.SamAccountName);

									principle.Dispose();
								}
							}
						}
					}
					catch (Exception ex)
					{
						throw new ProviderException("Unable to query Active Directory.", ex);
					}
				}

				_rolesForUserCache.Add(username, results);
			}

			return _rolesForUserCache[username].ToArray();
		}

		/// <summary>
		/// Retrieve listing of all users in a specified role.
		/// </summary>
		/// <param name="rolename">string array of users</param>
		/// <returns></returns>
		public override string[] GetUsersInRole(string rolename)
		{
			if (!RoleExists(rolename))
				throw new ProviderException(string.Format("The role '{0}' was not found.", rolename));

			if (!_usersInRoleCache.ContainsKey(rolename))
			{
				List<string> results = new List<string>();
				using (PrincipalContext context = new PrincipalContext(ContextType.Domain,_domainName,_username,_password))
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
					catch (Exception ex)
					{
						throw new ProviderException("Unable to query Active Directory.", ex);
					}
				}

				_usersInRoleCache.Add(rolename, results);
			}

			return _usersInRoleCache[rolename].ToArray();
		}

		/// <summary>
		/// Determine if a specified user is in a specified role.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="rolename"></param>
		/// <returns>Boolean indicating membership</returns>
		public override bool IsUserInRole(string username, string rolename)
		{
			List<string> users = GetUsersInRole(rolename).ToList();
			return users.Contains(username);
		}

		/// <summary>
		/// Determine if given role exists
		/// </summary>
		/// <param name="rolename">Role to check</param>
		/// <returns>Boolean indicating existence of role</returns>
		public override bool RoleExists(string rolename)
		{
			string filter = "(&(objectCategory=group)(samAccountName=" + rolename + "))";

			DirectoryEntry entry = new DirectoryEntry(_connectionString);

			if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
			{
				entry.Username = _username;
				entry.Password = _password;
			}

			DirectorySearcher searcher = new DirectorySearcher(entry);
			searcher.Filter = filter;
			searcher.SearchScope = SearchScope.Subtree;

			try
			{
				SearchResult searchResult = searcher.FindOne();
				return searchResult != null;
			}
			catch (Exception ex)
			{
				throw new ProviderException("Unable to query Active Directory.", ex);
			}
		}

		/// <summary>
		/// Return sorted list of usernames like usernameToMatch in rolename
		/// </summary>
		/// <param name="rolename">Role to check</param>
		/// <param name="usernameToMatch">Partial username to check</param>
		/// <returns></returns>
		public override string[] FindUsersInRole(string rolename, string usernameToMatch)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// This is a bit redundant if we only ever need to query the memberof part for a user,
		/// to check if they're in the editor or admin group.
		/// 
		/// Should it bother returning every group (or security group)?
		/// </summary>
		/// <returns>string array of roles</returns>
		public override string[] GetAllRoles()
		{
			throw new NotImplementedException();
		}

		#region NotSupported Base Class Functions
		/// <summary>
		/// AddUsersToRoles not supported.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Direcory. 
		/// </summary>
		public override void AddUsersToRoles(string[] usernames, string[] rolenames)
		{
			throw new NotSupportedException("Unable to add users to roles.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Direcory.");
		}

		/// <summary>
		/// CreateRole not supported.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Direcory. 
		/// </summary>
		public override void CreateRole(string rolename)
		{
			throw new NotSupportedException("Unable to create new role.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Direcory.");
		}

		/// <summary>
		/// DeleteRole not supported.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Direcory. 
		/// </summary>
		public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
		{
			throw new NotSupportedException("Unable to delete role.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Direcory.");
		}

		/// <summary>
		/// RemoveUsersFromRoles not supported.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Direcory. 
		/// </summary>
		public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
		{
			throw new NotSupportedException("Unable to remove users from roles.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Direcory.");
		}
		#endregion
	}
}