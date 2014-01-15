using System;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// Defines a class that should be stored in the database or other kind of data store.
	/// </summary>
	public interface IDataStoreEntity
	{
		/// <summary>
		/// The unique id for this object - for use with document stores that require a unique id for storage.
		/// </summary>
		Guid ObjectId { get; set; }
	}
}
