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

						using (PrincipalSearchResult<Principal> list = group.GetMembers())
						{
							foreach (Principal user in list)
							{
								UserPrincipal userPrincipal = user as UserPrincipal;
								if (userPrincipal != null)
								{
									results.Add(new PrincipalWrapper(userPrincipal));
									userPrincipal.Dispose();
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					throw new SecurityException(ex, "Unable to query Active Directory.");
				}
			}

			return results;
		}
	}
}
