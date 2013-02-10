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
	/// Config file settings - represents a &lt;roadkill&gt; section inside a configuration file.
	/// </summary>
	public class RoadkillSection : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets the name of the admin role.
		/// </summary>
		[ConfigurationProperty("adminRoleName", IsRequired = true)]
		public string AdminRoleName
		{
			get { return (string)this["adminRoleName"]; }
			set { this["adminRoleName"] = value; }
		}

		/// <summary>
		/// Gets or sets the attachments folder, which should begin with "~/".
		/// </summary>
		[ConfigurationProperty("attachmentsFolder", IsRequired = true)]
		public string AttachmentsFolder
		{
			get { return (string)this["attachmentsFolder"]; }
			set { this["attachmentsFolder"] = value; }
		}

		/// <summary>
		/// Indicates whether NHibernate 2nd level caching is enabled.
		/// </summary>
		[ConfigurationProperty("cacheEnabled", IsRequired = true)]
		public bool CacheEnabled
		{
			get { return (bool)this["cacheEnabled"]; }
			set { this["cacheEnabled"] = value; }
		}

		/// <summary>
		/// Indicates whether page content should be cached, if <see cref="CacheEnabled"/> is true.
		/// </summary>
		[ConfigurationProperty("cacheText", IsRequired = true)]
		public bool CacheText
		{
			get { return (bool)this["cacheText"]; }
			set { this["cacheText"] = value; }
		}

		/// <summary>
		/// Gets or sets the name of the connection string in the connectionstrings section.
		/// </summary>
		[ConfigurationProperty("connectionStringName", IsRequired = true)]
		public string ConnectionStringName
		{
			get { return (string) this["connectionStringName"]; }
			set { this["connectionStringName"] = value; }
		}

		/// <summary>
		/// The database type for Roadkill. This defaults to SQLServer2005 if empty - see DatabaseType enum for all options.
		/// </summary>
		[ConfigurationProperty("databaseType", IsRequired = false)]
		public string DataStoreType
		{
			// TODO: update the key to be "dataStoreType", plus make it backward compatible
			get { return (string)this["databaseType"]; }
			set { this["databaseType"] = value; }
		}

		/// <summary>
		/// Gets or sets the name of the editor role.
		/// </summary>
		[ConfigurationProperty("editorRoleName", IsRequired = true)]
		public string EditorRoleName
		{
			get { return (string)this["editorRoleName"]; }
			set { this["editorRoleName"] = value; }
		}

		/// <summary>
		/// Whether errors in updating the lucene index throw exceptions or are just ignored.
		/// </summary>
		[ConfigurationProperty("ignoreSearchIndexErrors", IsRequired = false)]
		public bool IgnoreSearchIndexErrors
		{
			get { return (bool)this["ignoreSearchIndexErrors"]; }
			set { this["ignoreSearchIndexErrors"] = value; }
		}

		/// <summary>
		/// Gets or sets whether this roadkill instance has been installed.
		/// </summary>
		[ConfigurationProperty("installed", IsRequired = true)]
		public bool Installed
		{
			get { return (bool)this["installed"]; }
			set { this["installed"] = value; }
		}

		/// <summary>
		/// Whether the site is public, i.e. all pages are visible by default. The default is true,
		/// and this is optional.
		/// </summary>
		[ConfigurationProperty("isPublicSite", IsRequired = false, DefaultValue = true)]
		public bool IsPublicSite
		{
			get { return (bool)this["isPublicSite"]; }
			set { this["isPublicSite"] = value; }
		}

		/// <summary>
		/// For example: LDAP://mydc01.company.internal
		/// </summary>
		[ConfigurationProperty("ldapConnectionString", IsRequired = false)]
		public string LdapConnectionString
		{
			get { return (string)this["ldapConnectionString"]; }
			set { this["ldapConnectionString"] = value; }
		}

		/// <summary>
		/// The username to authenticate against the AD with
		/// </summary>
		[ConfigurationProperty("ldapUsername", IsRequired = false)]
		public string LdapUsername
		{
			get { return (string)this["ldapUsername"]; }
			set { this["ldapUsername"] = value; }
		}

		/// <summary>
		/// The password to authenticate against the AD with
		/// </summary>
		[ConfigurationProperty("ldapPassword", IsRequired = false)]
		public string LdapPassword
		{
			get { return (string)this["ldapPassword"]; }
			set { this["ldapPassword"] = value; }
		}

		/// <summary>
		/// The repository type used for all datastore queries.
		/// </summary>
		[ConfigurationProperty("repositoryType", IsRequired = false, DefaultValue = "Roadkill.Core.NHibernateRepository, Roadkill.Core")]
		public string RepositoryType
		{
			get { return (string)this["repositoryType"]; }
			set { this["repositoryType"] = value; }
		}

		/// <summary>
		/// Whether to scale images dynamically on the page, using Javascript, so they fit inside the main page container (400x400px).
		/// </summary>
		[ConfigurationProperty("resizeImages", IsRequired = false, DefaultValue=true)]
		public bool ResizeImages
		{
			get 
			{
				if (this["resizeImages"] == null)
					return true;
				else
					return (bool)this["resizeImages"]; 
			}
			set { this["resizeImages"] = value; }
		}

		/// <summary>
		/// Whether to remove all HTML tags from the markup except those found in the whitelist.xml file,
		/// inside the App_Data folder.
		/// </summary>
		[ConfigurationProperty("useHtmlWhiteList", IsRequired = false, DefaultValue=true)]
		public bool UseHtmlWhiteList
		{
			get { return (bool)this["useHtmlWhiteList"]; }
			set { this["useHtmlWhiteList"] = value; }
		}

		/// <summary>
		/// Whether to enabled Windows and Active Directory authentication.
		/// </summary>
		[ConfigurationProperty("useWindowsAuthentication", IsRequired = true)]
		public bool UseWindowsAuthentication
		{
			get { return (bool)this["useWindowsAuthentication"]; }
			set { this["useWindowsAuthentication"] = value; }
		}

		/// <summary>
		/// The type used for the managing users, in the format "MyNamespace.Type, MyAssembly".
		/// This class should inherit from the <see cref="UserManager"/> class or a one of its derived types.
		/// </summary>
		[ConfigurationProperty("userManagerType", IsRequired = false)]
		public string UserManagerType
		{
			get { return (string)this["userManagerType"]; }
			set { this["userManagerType"] = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only,
		/// and can therefore be saved back to disk.
		/// </summary>
		/// <returns>This returns true.</returns>
		public override bool IsReadOnly()
		{
			return false;
		}
	}
}
