using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// The SitePreferences XML stored in the database, and the Roadkill version.
	/// </summary>
	public class SitePreferencesEntity : DataStoreEntity
	{
		internal static readonly Guid ConfigurationId = new Guid("b960e8e5-529f-4f7c-aee4-28eb23e13dbd");

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
		public override Guid ObjectId
		{
			get { return Id; }
			set { Id = value; }
		}

		public SitePreferencesEntity()
		{
			Id = ConfigurationId;
		}
	}
}
