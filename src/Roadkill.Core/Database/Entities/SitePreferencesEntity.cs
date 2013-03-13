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
		public virtual Guid Id { get; set; }

		/// <summary>
		/// The Serialized version of the <see cref="SitePreferences"/> class.
		/// </summary>
		public virtual string Xml { get; set; }

		/// <summary>
		/// The current version of Roadkill. This is used for upgrades.
		/// </summary>
		public virtual string Version { get; set; }

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
			Id = SitePreferences.SitePreferencesId;
		}
	}
}
