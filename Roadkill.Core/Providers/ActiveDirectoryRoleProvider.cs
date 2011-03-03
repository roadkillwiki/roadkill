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
		private string _applicationName;
		private string _activeDirectoryConnectionString;
		private string _domainDN;
		private bool _isAdditiveGroupMode;
		private List<string> _groupsToUse;
		private List<string> _groupsToIgnore;
		private List<string> _usersToIgnore;

		// IMPORTANT - DEFAULT LIST OF ACTIVE DIRECTORY USERS TO "IGNORE"
		//             DO NOT REMOVE ANY OF THESE UNLESS YOU FULLY UNDERSTAND THE SECURITY IMPLICATIONS
		//             VERYIFY THAT ALL CRITICAL USERS ARE IGNORED DURING TESTING
		private static readonly string[] _defaultUsersToIgnore = new string[]
        {
            "Administrator", "TsInternetUser", "Guest", "krbtgt", "Replicate", "SERVICE", "SMSService"
        };

		// IMPORTANT - DEFAULT LIST OF ACTIVE DIRECTORY DOMAIN GROUPS TO "IGNORE"
		//             PREVENTS ENUMERATION OF CRITICAL DOMAIN GROUP MEMBERSHIP
		//             DO NOT REMOVE ANY OF THESE UNLESS YOU FULLY UNDERSTAND THE SECURITY IMPLICATIONS
		//             VERIFY THAT ALL CRITICAL GROUPS ARE IGNORED DURING TESTING BY CALLING GetAllRoles MANUALLY
		private static readonly string[] _defaultGroupsToIgnore = new string[]
        {
            "Domain Guests", "Domain Computers", "Group Policy Creator Owners", "Guests", "Users",
            "Domain Users", "Pre-Windows 2000 Compatible Access", "Exchange Domain Servers", "Schema Admins",
            "Enterprise Admins", "Domain Admins", "Cert Publishers", "Backup Operators", "Account Operators",
            "Server Operators", "Print Operators", "Replicator", "Domain Controllers", "WINS Users",
            "DnsAdmins", "DnsUpdateProxy", "DHCP Users", "DHCP Administrators", "Exchange Services",
            "Exchange Enterprise Servers", "Remote Desktop Users", "Network Configuration Operators",
            "Incoming Forest Trust Builders", "Performance Monitor Users", "Performance Log Users",
            "Windows Authorization Access Group", "Terminal Server License Servers", "Distributed COM Users",
            "Administrators", "Everybody", "RAS and IAS Servers", "MTS Trusted Impersonators",
            "MTS Impersonators", "Everyone", "LOCAL", "Authenticated Users"
        };

		public override string ApplicationName
		{
			get { return _applicationName; }
			set { _applicationName = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ActiveDirectoryRoleProvider"/> class.
		/// </summary>
		public ActiveDirectoryRoleProvider()
		{
			_groupsToUse = new List<string>();
			_groupsToIgnore = new List<string>();
			_usersToIgnore = new List<string>();
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
			{
				throw new ArgumentNullException("config");
			}
			if (string.IsNullOrEmpty(name))
			{
				name = "ActiveDirectoryRoleProvider";
			}
			if (string.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", "Active Directory Role Provider");
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			// Check the activeDirectoryConnectionstring attribute is valid
			string adConnectionStringName = config["activeDirectoryConnectionString"];
			if (string.IsNullOrEmpty(adConnectionStringName))
				throw new ProviderException("The attribute 'activeDirectoryConnectionString' is missing or empty.");

			// Check the connection string it points to exists
			if (ConfigurationManager.ConnectionStrings[adConnectionStringName] == null ||
				string.IsNullOrWhiteSpace(ConfigurationManager.ConnectionStrings[adConnectionStringName].ConnectionString))
			{
				throw new ProviderException("The connection name 'activeDirectoryConnectionString' was not found in the applications configuration or the connection string is empty.");
			}

			_activeDirectoryConnectionString = ConfigurationManager.ConnectionStrings[adConnectionStringName].ConnectionString;
			if (_activeDirectoryConnectionString.Substring(0, 10) == "LDAP://DC=")
			{
				_domainDN = _activeDirectoryConnectionString.Substring(7, _activeDirectoryConnectionString.Length - 7);
			}
			else
			{
				throw new ProviderException("The connection string specified in 'activeDirectoryConnectionString' does not appear to be a valid LDAP connection string.");
			}

			// Retrieve Application Name
			_applicationName = config["applicationName"];
			if (string.IsNullOrEmpty(_applicationName))
			{
				_applicationName = GetDefaultAppName();
			}
			if (_applicationName.Length > 256)
			{
				throw new ProviderException("The application name is too long.");
			}

			// Retrieve Group Mode
			// "Additive" indicates that only the groups specified in groupsToUse will be used
			// "Subtractive" indicates that all Active Directory groups will be used except those specified in groupsToIgnore
			// "Additive" is somewhat more secure, but requires more maintenance when groups change
			string groupMode = config["groupMode"];
			if (string.IsNullOrEmpty(groupMode))
			{
				throw new ProviderException("The attribute 'groupMode' is missing or empty.");
			}
			if (groupMode == "Additive")
			{
				_isAdditiveGroupMode = true;
			}
			else if (groupMode == "Subtractive")
			{
				_isAdditiveGroupMode = false;
			}
			else
			{
				throw new ProviderException("The attribute 'groupMode' must be set to 'Additive' or 'Subtractive'.");
			}

			// If Additive group mode, populate GroupsToUse with specified AD groups
			if (_isAdditiveGroupMode)
			{
				if (!string.IsNullOrEmpty(config["groupsToUse"]))
				{
					foreach (string group in config["groupsToUse"].Trim().Split(','))
					{
						_groupsToUse.Add(group.Trim());
					}
				}
			}

			// Populate GroupsToIgnore ArrayList with AD groups that should be ignored for roles purposes
			foreach (string group in _defaultGroupsToIgnore)
			{
				_groupsToIgnore.Add(group.Trim());
			}
			if (!string.IsNullOrEmpty(config["groupsToIgnore"]))
			{
				foreach (string group in config["groupsToIgnore"].Trim().Split(','))
				{
					_groupsToIgnore.Add(group.Trim());
				}
			}

			// Populate UsersToIgnore ArrayList with AD users that should be ignored for roles purposes
			foreach (string group in _defaultUsersToIgnore)
			{
				_usersToIgnore.Add(group.Trim());
			}
			if (!string.IsNullOrEmpty(config["usersToIgnore"]))
			{
				foreach (string group in config["usersToIgnore"].Trim().Split(','))
				{
					_usersToIgnore.Add(group.Trim());
				}
			}
		}

		/// <summary>
		/// Retrieve the current app name if none has been specified in config.
		/// As implimented by MS, lifted from SqlRoleProvider.
		/// </summary>
		/// <returns>string containing the current app name.</returns>
		private static string GetDefaultAppName()
		{
			try
			{
				string appName = HostingEnvironment.ApplicationVirtualPath;
				if (string.IsNullOrEmpty(appName))
				{
					appName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
					int indexOfDot = appName.IndexOf('.');
					if (indexOfDot != -1)
					{
						appName = appName.Remove(indexOfDot);
					}
				}
				if (string.IsNullOrEmpty(appName))
				{
					return "/";
				}
				else
				{
					return appName;
				}
			}
			catch
			{
				return "/";
			}
		}

		/// <summary>
		/// Retrieve listing of all roles to which a specified user belongs.
		/// </summary>
		/// <param name="username"></param>
		/// <returns>string array of roles</returns>
		public override string[] GetRolesForUser(string username)
		{
			List<string> results = new List<string>();
			using (PrincipalContext context = new PrincipalContext(ContextType.Domain, null, _domainDN))
			{
				try
				{
					UserPrincipal principal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
					var groups = principal.GetAuthorizationGroups();
					foreach (GroupPrincipal group in groups)
					{
						if (!_groupsToIgnore.Contains(group.SamAccountName))
						{
							if (_isAdditiveGroupMode)
							{
								if (_groupsToUse.Contains(group.SamAccountName))
								{
									results.Add(group.SamAccountName);
								}
							}
							else
							{
								results.Add(group.SamAccountName);
							}
						}
					}
				}
				catch (Exception ex)
				{
					throw new ProviderException("Unable to query Active Directory.", ex);
				}
			}

			return results.ToArray();
		}

		/// <summary>
		/// Retrieve listing of all users in a specified role.
		/// </summary>
		/// <param name="rolename">string array of users</param>
		/// <returns></returns>
		public override string[] GetUsersInRole(string rolename)
		{
			if (!RoleExists(rolename))
			{
				throw new ProviderException(string.Format("The role '{0}' was not found.", rolename));
			}

			List<string> results = new List<string>();
			using (PrincipalContext context = new PrincipalContext(ContextType.Domain, null, _domainDN))
			{
				try
				{
					GroupPrincipal p = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, rolename);
					var users = p.GetMembers(true);
					foreach (UserPrincipal user in users)
					{
						if (!_usersToIgnore.Contains(user.SamAccountName))
						{
							results.Add(user.SamAccountName);
						}
					}
				}
				catch (Exception ex)
				{
					throw new ProviderException("Unable to query Active Directory.", ex);
				}
			}

			return results.ToArray();
		}

		/// <summary>
		/// Determine if a specified user is in a specified role.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="rolename"></param>
		/// <returns>Boolean indicating membership</returns>
		public override bool IsUserInRole(string username, string rolename)
		{
			foreach (string strUser in GetUsersInRole(rolename))
			{
				if (username == strUser) return true;
			}
			return false;
		}

		/// <summary>
		/// Retrieve listing of all roles.
		/// </summary>
		/// <returns>string array of roles</returns>
		public override string[] GetAllRoles()
		{
			List<string> results = new List<string>();
			string[] roles = Search(_activeDirectoryConnectionString, "(&(objectCategory=group)(|(groupType=-2147483646)(groupType=-2147483644)(groupType=-2147483640)))", "samAccountName");
			foreach (string strRole in roles)
			{
				if (!_groupsToIgnore.Contains(strRole))
				{
					if (_isAdditiveGroupMode)
					{
						if (_groupsToUse.Contains(strRole))
						{
							results.Add(strRole);
						}
					}
					else
					{
						results.Add(strRole);
					}
				}
			}

			return results.ToArray();
		}

		/// <summary>
		/// Determine if given role exists
		/// </summary>
		/// <param name="rolename">Role to check</param>
		/// <returns>Boolean indicating existence of role</returns>
		public override bool RoleExists(string rolename)
		{
			foreach (string strRole in GetAllRoles())
			{
				if (rolename == strRole) return true;
			}
			return false;
		}

		/// <summary>
		/// Return sorted list of usernames like usernameToMatch in rolename
		/// </summary>
		/// <param name="rolename">Role to check</param>
		/// <param name="usernameToMatch">Partial username to check</param>
		/// <returns></returns>
		public override string[] FindUsersInRole(string rolename, string usernameToMatch)
		{
			if (!RoleExists(rolename))
			{
				throw new ProviderException(string.Format("The role '{0}' was not found.", rolename));
			}

			List<string> results = new List<string>();
			string[] roles = GetAllRoles();
			foreach (string role in roles)
			{
				if (role.ToLower().Contains(usernameToMatch.ToLower()))
				{
					results.Add(role);
				}
			}
			results.Sort();

			return results.ToArray();
		}

		/// <summary>
		/// Performs an extremely constrained query against Active Directory.  Requests only a single value from
		/// AD based upon the filtering parameter to minimize performance hit from large queries.
		/// </summary>
		/// <param name="connectionString">Active Directory Connection string</param>
		/// <param name="filter">LDAP format search filter</param>
		/// <param name="field">AD field to return</param>
		/// <param name="scopeQuery">Display name of the distinguished name attribute to search in</param>
		/// <returns>string array containing values specified by 'field' parameter</returns>
		private string[] Search(string connectionString, string filter, string field)
		{
			List<string> results = new List<string>();

			DirectorySearcher searcher = new DirectorySearcher();
			searcher.SearchRoot = new DirectoryEntry(connectionString);
			searcher.Filter = filter;
			searcher.PropertiesToLoad.Clear();
			searcher.PropertiesToLoad.Add(field);
			searcher.PageSize = 500;

			try
			{
				//HttpContext.Current.Cache
				using (SearchResultCollection searchResults = searcher.FindAll())
				{
					foreach (SearchResult result in searchResults)
					{
						int resultCount = result.Properties[field].Count;
						for (int i = 0; i < resultCount; i++)
						{
							string temp = result.Properties[field][i].ToString();
							results.Add(temp);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new ProviderException("Unable to query Active Directory.", ex);
			}

			return results.ToArray();
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