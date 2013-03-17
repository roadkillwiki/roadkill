using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Database;

namespace Roadkill.Core
{
	/// <summary>
	/// A page object for use with the NHibernate data store. This object is intended for internal use only.
	/// </summary>
	public class Page : IDataStoreEntity
	{
		private Guid _objectId;

		/// <remarks>
		/// Reasons for using an int for the primary key:
		/// + Clustered PKs without using guid.comb
		/// + Nice URLs.
		/// - Losing the certainty of uniqueness like a guid
		/// - Oracle is not supported.
		/// </remarks>
		public int Id { get; set; }
		public string Title { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string ModifiedBy { get; set; }
		public DateTime ModifiedOn { get; set; }

		/// <summary>
		/// Tag tags for the page, in the format "tag1,tag2,tag3" (no spaces between tags).
		/// </summary>
		public string Tags { get; set; }

		/// <summary>
		/// Whether the page is locked for admin-only editing.
		/// </summary>
		public bool IsLocked { get; set; }

		public Guid ObjectId
		{
			get { return _objectId; }
			set { _objectId = value; }
		}
	}
}
