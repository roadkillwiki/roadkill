using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// Represents settings that are stored as JSON in the database. This is used by the <see cref="SiteSettings"/> 
	/// class and by plugin settings.
	/// </summary>
	public class SiteConfigurationEntity : IDataStoreEntity
	{
		/// <summary>
		/// Gets or sets the unique ID for the settings.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
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
		/// The unique id for this object, this is the same as the <see cref="Id"/> property.
		/// </summary>
		public Guid ObjectId
		{
			get { return Id; }
			set { Id = value; }
		}
	}
}
