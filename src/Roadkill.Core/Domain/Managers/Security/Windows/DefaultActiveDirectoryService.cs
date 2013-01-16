using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// Default AD implentation of the service.
	/// </summary>
	internal class DefaultActiveDirectoryService : IActiveDirectoryService
	{
		public IEnumerable<IRoadKillPrincipal> GetMembers(string domainName, string username, string password, string groupName)
		{
			List<PrincipalWrapper> results = new List<PrincipalWrapper>();

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
        private static void AddGroupMembers(GroupPrincipal group, List<PrincipalWrapper> principals)
        {

            using (PrincipalSearchResult<Principal> list = group.GetMembers())
            {
                foreach (Principal principal in list)
                {
                    UserPrincipal userPrincipal = principal as UserPrincipal;
                    if (userPrincipal != null)
                    {
                        principals.Add(new PrincipalWrapper(userPrincipal));
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

	}
}
