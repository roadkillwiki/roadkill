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

		[ConfigurationProperty("connectionString",IsRequired=true)]
		public string ConnectionString
		{
			get { return (string) this["connectionString"]; }
			set { this["connectionString"] = value; }
		}

		[ConfigurationProperty("adminGroup", IsRequired = true)]
		public string AdminGroup
		{
			get { return (string)this["adminGroup"]; }
			set { this["adminGroup"] = value; }
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

		public override bool IsReadOnly()
		{
			return false;
		}
	}
}
