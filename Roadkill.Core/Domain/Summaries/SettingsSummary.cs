using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Roadkill.Core
{
	[Serializable]
	public class SettingsSummary
	{
		[Required(ErrorMessage="The sitename is empty")]
		public string SiteName { get; set; }
		
		[Required(ErrorMessage="The connection string is empty")]
		public string ConnectionString { get; set; }

		public string AdminPassword { get; set; }
		public string RolesConnectionString { get; set; }

		public bool UseWindowsAuth { get; set; }
		public string LdapConnectionString { get; set; }
		public string LdapUsername { get; set; }
		public string LdapPassword { get; set; }
		
		public string EditorRoleName { get; set; }
		public string AdminRoleName { get; set; }

		public string AllowedExtensions { get; set; }
		
		[Required(ErrorMessage = "The attachments folder is empty")]
		public string AttachmentsFolder { get; set; }
		
		[Required(ErrorMessage = "The markup type is empty")]
		public string MarkupType { get; set; }
		
		[Required(ErrorMessage = "The theme is empty")]
		
		public string Theme { get; set; }
		public bool CacheEnabled { get; set; }
		public bool CacheText { get; set; }
		public bool AllowUserSignup { get; set; }
	}
}
