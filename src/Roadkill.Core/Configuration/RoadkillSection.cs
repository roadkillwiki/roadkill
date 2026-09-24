using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// The Roadkill settings stored in the "Roadkill" section of appsettings.json (previously the roadkill section of the web.config). The connection string is stored in ConnectionStrings:Roadkill.
	/// </summary>
	public class RoadkillSection
	{
		/// <summary>
		/// The name of the role that users should belong to in order to create,edit,delete pages,
		/// manage users, manage site settings and use the admin tools.
		/// </summary>
		public string AdminRoleName { get; set; } = "Admin";

		/// <summary>
		/// A comma separated list of API keys for the REST api. If this is empty, the REST api is disabled.
		/// </summary>
		public string ApiKeys { get; set; } = "";

		/// <summary>
		/// The folder where all uploads (typically image files) are saved to.
		/// </summary>
		public string AttachmentsFolder { get; set; } = "~/App_Data/Attachments";

		/// <summary>
		/// The route used for all attachment HTTP requests.
		/// </summary>
		public string AttachmentsRoutePath { get; set; } = "Attachments";

		/// <summary>
		/// The connection string to the Roadkill database.
		/// </summary>
		public string ConnectionString { get; set; } = "";

		/// <summary>
		/// The name of the role that users should belong to in order to create and edit pages.
		/// </summary>
		public string EditorRoleName { get; set; } = "Editor";

		/// <summary>
		/// Whether errors in updating the lucene index throw exceptions or are just ignored.
		/// </summary>
		public bool IgnoreSearchIndexErrors { get; set; } = true;

		/// <summary>
		/// Whether the installation has been completed.
		/// </summary>
		public bool Installed { get; set; }

		/// <summary>
		/// Whether the site is public, i.e. all pages are visible by default.
		/// </summary>
		public bool IsPublicSite { get; set; } = true;

		/// <summary>
		/// Whether to remove all HTML tags from the markup except those found in the whitelist.xml file.
		/// </summary>
		public bool UseHtmlWhiteList { get; set; } = true;

		/// <summary>
		/// The type used for the user service (an assembly qualified name). Blank uses the default forms based service.
		/// </summary>
		public string UserServiceType { get; set; } = "";

		/// <summary>
		/// Whether to enabled the server side object cache.
		/// </summary>
		public bool UseObjectCache { get; set; } = true;

		/// <summary>
		/// Whether to send HTTP cache headers to the browser.
		/// </summary>
		public bool UseBrowserCache { get; set; }

		/// <summary>
		/// The database type/provider, SqlServer, Postgres or MongoDB (the Roadkill 2.x names, e.g. SqlServer2008, are read as SqlServer).
		/// </summary>
		public string DatabaseName { get; set; } = "SqlServer";

		/// <summary>
		/// Whether to use Azure blob storage for attachments.
		/// </summary>
		public bool UseAzureFileStorage { get; set; }

		/// <summary>
		/// The connection string for Azure blob storage.
		/// </summary>
		public string AzureConnectionString { get; set; } = "";

		/// <summary>
		/// The Azure blob storage container for attachments.
		/// </summary>
		public string AzureContainer { get; set; } = "";

		/// <summary>
		/// The UI language code, e.g. "en" or "fr".
		/// </summary>
		public string UiLanguage { get; set; } = "en";

		/// <summary>
		/// The SMTP server for sending emails (signup and password reset). If this is empty, emails are written to the
		/// <see cref="SmtpPickupDirectory"/> instead.
		/// </summary>
		public string SmtpHost { get; set; } = "";

		/// <summary>
		/// The SMTP server port.
		/// </summary>
		public int SmtpPort { get; set; } = 25;

		/// <summary>
		/// The SMTP username, if the server requires authentication.
		/// </summary>
		public string SmtpUsername { get; set; } = "";

		/// <summary>
		/// The SMTP password, if the server requires authentication.
		/// </summary>
		public string SmtpPassword { get; set; } = "";

		/// <summary>
		/// Whether to use SSL/TLS for the SMTP connection.
		/// </summary>
		public bool SmtpEnableSsl { get; set; }

		/// <summary>
		/// The from address for emails.
		/// </summary>
		public string SmtpFrom { get; set; } = "signup@roadkillwiki.net";

		/// <summary>
		/// The folder emails are written to when no SMTP host is set. "~/" is the site root.
		/// </summary>
		public string SmtpPickupDirectory { get; set; } = "~/App_Data/TempSmtp";

		/// <summary>
		/// Whether this instance is running as the (read only) demo site.
		/// </summary>
		public bool IsDemoSite { get; set; }
	}
}
