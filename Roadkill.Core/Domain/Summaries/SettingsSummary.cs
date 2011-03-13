using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	public class SettingsSummary
	{
		public string SiteName { get; set; }
		public string ConnectionString { get; set; }
		public string RolesConnectionString { get; set; }
		public bool UseWindowsAuth { get; set; }
		public bool CacheEnabled { get; set; }
		public bool CacheText { get; set; }
		public string EditorRoleName { get; set; }
		public string AdminRoleName { get; set; }
	}
}
