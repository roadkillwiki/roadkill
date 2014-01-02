using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Security.Windows
{
	/// <summary>
	/// Default AD implentation of the provider.
	/// </summary>
	public class ActiveDirectoryProvider : IActiveDirectoryProvider
	{
		public IEnumerable<IPrincipalDetails> GetMembers(string domainName, string username, string password, string groupName)
		{
			List<PrincipalDetails> results = new List<PrincipalDetails>();

			using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domainName, username, password))
			{
				if (!string.IsNullOrEmpty(username))
				{
					bool valid = context.ValidateCredentials(username, password);
					if (!valid)
						throw new SecurityException(null, "Unable to authenticate with '{0}' using '{1}'", domainName, username);
				}

				try
				{
					using (GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, groupName))
					{
						// FindByIdentity returns null if no matches were found
						if (group == null)
							throw new InvalidOperationException(string.Format("The group {0} could not be found", groupName));

                        // Add all of the members of this group, and any sub-group to the list of members.
                        AddGroupMembers(group, results);
					}
				}
				catch (Exception ex)
				{
					throw new SecurityException(ex, "Unable to query Active Directory.");
				}
			}

			return results;
		}

        /// <summary>
        /// This method adds all of the user principals in the specified group to the list of principals.
        /// It will also include any user principal that is a member of a group within the specified group.
        /// </summary>
        /// <param name="group">The group from which users will be added to the principal list.</param>
        /// <param name="principals">The list of user principals.</param>
        private static void AddGroupMembers(GroupPrincipal group, List<PrincipalDetails> principals)
        {            
            using (PrincipalSearchResult<Principal> list = group.GetMembers())
            {
                foreach (Principal principal in list)
                {
                    UserPrincipal userPrincipal = principal as UserPrincipal;
                    if (userPrincipal != null)
                    {
                        principals.Add(new PrincipalDetails(userPrincipal));
                        userPrincipal.Dispose();
                    }
                    else
                    {
                        GroupPrincipal groupPrincipal = principal as GroupPrincipal;

                        if (groupPrincipal != null)
                        {
                            AddGroupMembers(groupPrincipal, principals);
                        }
                    }
                }
            }
        }

		/// <summary>
		/// Tests a LDAP (Active Directory) connection.
		/// </summary>
		/// <param name="connectionString">The LDAP connection string (requires LDAP:// at the start).</param>
		/// <param name="username">The ldap username.</param>
		/// <param name="password">The ldap password.</param>
		/// <param name="groupName">The Active Directory group name to test against. Defaults to "Users" if empty</param>
		/// <returns>Any error messages or an empty string if no errors occurred.</returns>
		public string TestLdapConnection(string connectionString, string username, string password, string groupName)
		{
			if (string.IsNullOrEmpty(connectionString))
				return "The connection string is empty";

			try
			{
				int length = "ldap://".Length;
				if (!connectionString.StartsWith("LDAP://") || connectionString.Length < length)
					throw new Exception(string.Format("The LDAP connection string: '{0}' does not appear to be a valid (make sure it's uppercase LDAP).", connectionString));

				DirectoryEntry entry = new DirectoryEntry(connectionString);

				if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
				{
					entry.Username = username;
					entry.Password = password;
				}
				else
				{
					// Use built-in ones for querying
					username = "administrator"; // may need to use Guest here.
					groupName = "Users";
				}

				string accountName = username;
				string filter = "(&(objectCategory=user)(samAccountName=" + username + "))";

				if (!string.IsNullOrEmpty(groupName))
				{
					filter = "(&(objectCategory=group)(samAccountName=" + groupName + "))";
					accountName = groupName;
				}

				DirectorySearcher searcher = new DirectorySearcher(entry);
				searcher.Filter = filter;
				searcher.SearchScope = SearchScope.Subtree;

				SearchResult searchResult = searcher.FindOne();
				if (searchResult == null)
					return "Warning only: Unable to find " + accountName + " in the AD";
				else
					return "";
			}
			catch (Exception e)
			{
				return e.ToString();
			}
		}
	}
}
