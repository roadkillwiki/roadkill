using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// Settings that are stored as JSON in the database. This is used by the SiteSettings and by 
	/// plugin settings.
	/// </summary>
	public class SiteConfigurationEntity : IDataStoreEntity
	{
		/// <summary>
		/// The unique ID for the instance (this doesn't change).
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The jsonified version of the settings.
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// A version for the settings, e.g. the Roadkill version.
		/// </summary>
		public string Version { get; set; }

		/// <summary>
		/// The same as the ID property.
		/// </summary>
		public Guid ObjectId
		{
			get { return Id; }
			set { Id = value; }
		}
	}
}
