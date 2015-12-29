using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;
using Roadkill.Core.Security;

// Don't change the namespace to "Roadkill.Core.Configuration" it will break older web.config files
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
		/// TODO: comments
		/// </summary>
		[ConfigurationProperty("attachmentsRoutePath", IsRequired = false, DefaultValue = "Attachments")]
		public string AttachmentsRoutePath
		{
			get { return (string)this["attachmentsRoutePath"]; }
			set { this["attachmentsRoutePath"] = value; }
		}


		/// <summary>
		/// Gets or sets the name of the connection string in the connectionstrings section.
		/// </summary>
		[ConfigurationProperty("connectionStringName", IsRequired = true)]
		public string ConnectionStringName
		{
			get { return (string)this["connectionStringName"]; }
			set { this["connectionStringName"] = value; }
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
		/// Whether to remove all HTML tags from the markup except those found in the whitelist.xml file,
		/// inside the App_Data folder.
		/// </summary>
		[ConfigurationProperty("useHtmlWhiteList", IsRequired = false, DefaultValue = true)]
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
		/// The type used for the managing users, in the format "MyNamespace.Type".
		/// This class should inherit from the <see cref="UserServiceBase"/> class or a one of its derived types.
		/// </summary>
		[ConfigurationProperty("userServiceType", IsRequired = false)]
		public string UserServiceType
		{
			get { return (string)this["userServiceType"]; }
			set { this["userServiceType"] = value; }
		}

		/// <summary>
		/// Indicates whether server-based page object caching is enabled.
		/// </summary>
		[ConfigurationProperty("useObjectCache", IsRequired = false, DefaultValue = true)]
		public bool UseObjectCache
		{
			get { return (bool)this["useObjectCache"]; }
			set { this["useObjectCache"] = value; }
		}

		/// <summary>
		/// Indicates whether page content should be cached, if <see cref="UseObjectCache"/> is true.
		/// </summary>
		[ConfigurationProperty("useBrowserCache", IsRequired = false, DefaultValue = false)]
		public bool UseBrowserCache
		{
			get { return (bool)this["useBrowserCache"]; }
			set { this["useBrowserCache"] = value; }
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

		/// <summary>
		/// The database type for Roadkill. This defaults to SQLServer2008 (MongoDB on Mono) if empty.
		/// </summary>
		[ConfigurationProperty("databaseName", IsRequired = false)]
		internal string DatabaseName
		{
			get { return (string)this["databaseName"]; }
			set { this["databaseName"] = value; }
		}

		/// <summary>
		/// TODO: comments + tests
		/// </summary>
		[ConfigurationProperty("useAzureFileStorage", IsRequired = false, DefaultValue = false)]
		public bool UseAzureFileStorage
		{
			get { return (bool)this["useAzureFileStorage"]; }
			set { this["useAzureFileStorage"] = value; }
		}

		/// <summary>
		/// TODO: comments + tests
		/// </summary>
		[ConfigurationProperty("azureConnectionString", IsRequired = false, DefaultValue = "Attachments")]
		public string AzureConnectionString
		{
			get { return (string)this["azureConnectionString"]; }
			set { this["azureConnectionString"] = value; }
		}

		/// <summary>
		/// TODO: comments + tests
		/// </summary>
		[ConfigurationProperty("azureContainer", IsRequired = false, DefaultValue = "Attachments")]
		public string AzureContainer
		{
			get { return (string)this["azureContainer"]; }
			set { this["azureContainer"] = value; }
		}
	}
}
