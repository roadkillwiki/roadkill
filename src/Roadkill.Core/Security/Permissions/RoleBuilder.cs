using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Security.Permissions.Capabilities;

namespace Roadkill.Core.Security.Permissions
{
	internal class RoleBuilder
	{
		private static Role AllCapabilities()
		{
			return new Role()
			{
				Name = "Super Admin",
				Description = "The god of Roadkill",

				IOCapabilities = GetEnumValues<IOCapability>(),
				PageCapabilities = GetEnumValues<PageCapability>(),
				SiteSettingsCapabilities = GetEnumValues<SiteSettingsCapability>()
			};
		}

		public static Role SuperAdmin()
		{
			Role superAdmin = AllCapabilities();
			superAdmin.Name = "Super Admin";
			superAdmin.Description = "God of Roadkill";

			return superAdmin;
		}

		public static Role Admin()
		{
			// Exactly the same as a Super Admin
			Role admin = AllCapabilities();
			admin.Name = "Admin";
			admin.Description = "The same as a Super Admin, but without the ability to add/remove admin users.";

			return admin;
		}

		private static List<T> GetEnumValues<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>().ToList();
		}
	}
}
