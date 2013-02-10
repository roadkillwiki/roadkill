using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NHibernate;

namespace Roadkill.Core
{
	/// <summary>
	/// A page object for use with the NHibernate data store. This object is intended for internal use only.
	/// </summary>
	public class Page : DataStoreEntity
	{
		private Guid _objectId;

		/// <remarks>
		/// Reasons for using an int for the primary key:
		/// + Clustered PKs without using guid.comb
		/// + Nice URLs.
		/// - Losing the certainty of uniqueness like a guid
		/// - Oracle is not supported.
		/// </remarks>
		public virtual int Id { get; set; }
		public virtual string Title { get; set; }
		public virtual string CreatedBy { get; set; }
		public virtual DateTime CreatedOn { get; set; }
		public virtual string ModifiedBy { get; set; }
		public virtual DateTime ModifiedOn { get; set; }

		/// <summary>
		/// Tag tags for the page, in the format "tag1,tag2,tag3" (no spaces between tags).
		/// </summary>
		public virtual string Tags { get; set; }

		/// <summary>
		/// Whether the page is locked for admin-only editing.
		/// </summary>
		public virtual bool IsLocked { get; set; }

		public override Guid ObjectId
		{
			get { return _objectId; }
			set { _objectId = value; }
		}
	}
}
