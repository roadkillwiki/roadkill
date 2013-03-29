using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Describes a class that contains both application settings and site preferences.
	/// </summary>
	public interface IConfigurationContainer
	{
		/// <summary>
		/// The site preferences, which are usually stored in the database or on disk.
		/// </summary>
		SiteSettings SitePreferences { get; set; }

		/// <summary>
		/// The application settings, which are usually stored in a app.config or web.config file.
		/// These settings are not intended to be changed without Roadkill requiring an application 
		/// pool recycle (although for most of the settings this isn't necessary).
		/// </summary>
		ApplicationSettings ApplicationSettings { get; set; }
	}
}
