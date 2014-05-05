using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Security.Permissions.Capabilities;
using Roadkill.Core.Security.Permissions.Capabilities;

namespace Roadkill.Core.Security.Permissions
{
	/// <summary>
	/// Represents a role containing a group of capabilities/tasks that can be performed.
	/// </summary>
	public class Role
	{
		public static readonly Role SuperAdmin = RoleBuilder.SuperAdmin();
		public static readonly Role Admin = RoleBuilder.Admin();

		// Properties must have public setters for XML serialization, and be Lists not Enumerables
		public string Name { get; set; }
		public string Description { get; set; }

		public List<IOCapability> IOCapabilities { get; set; }
		public List<PageCapability> PageCapabilities { get; set; }
		public List<SiteSettingsCapability> SiteSettingsCapabilities { get; set; }

		public Role()
		{
			IOCapabilities = new List<IOCapability>();
			PageCapabilities = new List<PageCapability>();
			SiteSettingsCapabilities = new List<SiteSettingsCapability>();
		}

		internal void RemoveIOCapability(IOCapability capability)
		{
			IOCapabilities.Remove(capability);
		}
	}
}
