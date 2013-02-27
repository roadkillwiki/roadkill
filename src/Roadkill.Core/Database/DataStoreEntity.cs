using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// Defines a class that should be stored in the database or other kind of storage.
	/// </summary>
	public abstract class DataStoreEntity
	{
		/// <summary>
		/// The unique id for this object - for use with document stores or custom databases that NHibernate doesn't support.
		/// </summary>
		public abstract Guid ObjectId { get; set; }
	}
}
