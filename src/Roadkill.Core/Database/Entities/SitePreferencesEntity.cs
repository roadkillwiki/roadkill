using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// The SitePreferences XML stored in the database, and the Roadkill version.
	/// </summary>
	public class SitePreferencesEntity : IDataStoreEntity
	{
		/// <summary>
		/// The unique ID for the instance (this doesn't change).
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The jsonified version of the <see cref="SiteSettings"/> class.
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// The current version of Roadkill. This is used for upgrades.
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

		public SitePreferencesEntity()
		{
			Id = SiteSettings.SiteSettingsId;
		}
	}
}
