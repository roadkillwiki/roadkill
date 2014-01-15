using System;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// Represents a version of a page's textual content, for use in the data store.
	/// </summary>
	public class PageContent : IDataStoreEntity
	{
		/// <summary>
		/// Gets or sets the unique ID for the page version.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets the page the content belongs to.
		/// </summary>
		/// <value>
		/// The page.
		/// </value>
		public Page Page { get; set; }

		/// <summary>
		/// Gets or sets the markdown text for the page.
		/// </summary>
		/// <value>
		/// The text.
		/// </value>
		public string Text { get; set; }

		/// <summary>
		/// Gets or sets the user who edited this version of the page.
		/// </summary>
		/// <value>
		/// The edited by.
		/// </value>
		public string EditedBy { get; set; }

		/// <summary>
		/// Gets or sets the date the version was edited on.
		/// </summary>
		/// <value>
		/// The edited on.
		/// </value>
		public DateTime EditedOn { get; set; }

		/// <summary>
		/// Gets or sets the version number of the content, which starts at 0.
		/// </summary>
		/// <value>
		/// The version number.
		/// </value>
		public int VersionNumber { get; set; }

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
