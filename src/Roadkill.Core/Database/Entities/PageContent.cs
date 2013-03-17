using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;

namespace Roadkill.Core
{
	/// <summary>
	/// Contains versioned text data for a page for use with the NHibernate data store. This object is intended for internal use only.
	/// </summary>
	public class PageContent : IDataStoreEntity
	{
		public Guid Id { get; set; }
		public Page Page { get; set; }
		public string Text { get; set; }
		public string EditedBy { get; set; }
		public DateTime EditedOn { get; set; }
		public int VersionNumber { get; set; }
		public Guid ObjectId
		{
			get { return Id; }
			set { Id = value; }
		}

		public PageSummary ToSummary(MarkupConverter markupConverter)
		{
			return new PageSummary()
			{
				Id = Page.Id,
				Title = Page.Title,
				PreviousTitle = Page.Title,
				CreatedBy = Page.CreatedBy,
				CreatedOn = Page.CreatedOn,
				IsLocked = Page.IsLocked,
				ModifiedBy = Page.ModifiedBy,
				ModifiedOn = Page.ModifiedOn,
				RawTags = Page.Tags,
				Content = Text,
				ContentAsHtml = markupConverter.ToHtml(Text),
				VersionNumber = VersionNumber,
			};
		}
	}
}
