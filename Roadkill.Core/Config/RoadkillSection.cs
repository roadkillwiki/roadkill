using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;

namespace Roadkill.Core
{
	/// <summary>
	/// Represents a &lt;roadkill&gt; section inside a configuration file.
	/// </summary>
	public class RoadkillSection : ConfigurationSection
	{
		private static RoadkillSection _section;
		public static RoadkillSection Current
		{
			get
			{
				if (_section == null)
					_section = ConfigurationManager.GetSection("roadkill") as RoadkillSection;

				return _section;
			}
		}

		[ConfigurationProperty("connectionStringName", IsRequired = true)]
		public string ConnectionStringName
		{
			get { return (string) this["connectionStringName"]; }
			set { this["connectionStringName"] = value; }
		}

		[ConfigurationProperty("editorRoleName", IsRequired = true)]
		public string EditorRoleName
		{
			get { return (string)this["editorRoleName"]; }
			set { this["editorRoleName"] = value; }
		}

		[ConfigurationProperty("adminRoleName", IsRequired = true)]
		public string AdminRoleName
		{
			get { return (string)this["adminRoleName"]; }
			set { this["adminRoleName"] = value; }
		}

		[ConfigurationProperty("attachmentsFolder", IsRequired = true)]
		public string AttachmentsFolder
		{
			get { return (string)this["attachmentsFolder"]; }
			set { this["attachmentsFolder"] = value; }
		}

		[ConfigurationProperty("markupType", IsRequired = true)]
		public string MarkupType
		{
			get { return (string)this["markupType"]; }
			set { this["markupType"] = value; }
		}

		[ConfigurationProperty("installed", IsRequired = true)]
		public bool Installed
		{
			get { return (bool)this["installed"]; }
			set { this["installed"] = value; }
		}

		[ConfigurationProperty("theme", IsRequired = true)]
		public string Theme
		{
			get { return (string)this["theme"]; }
			set { this["theme"] = value; }
		}

		[ConfigurationProperty("cacheEnabled", IsRequired = true)]
		public bool CacheEnabled
		{
			get { return (bool)this["cacheEnabled"]; }
			set { this["cacheEnabled"] = value; }
		}

		[ConfigurationProperty("cacheText", IsRequired = true)]
		public bool CacheText
		{
			get { return (bool)this["cacheText"]; }
			set { this["cacheText"] = value; }
		}

		public override bool IsReadOnly()
		{
			return false;
		}
	}
}
