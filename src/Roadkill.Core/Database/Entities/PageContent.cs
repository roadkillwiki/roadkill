using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
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
		public virtual Guid Id { get; set; }
		public virtual Page Page { get; set; }
		public virtual string Text { get; set; }
		public virtual string EditedBy { get; set; }
		public virtual DateTime EditedOn { get; set; }
		public virtual int VersionNumber { get; set; }
		public Guid ObjectId
		{
			get { return Id; }
			set { Id = value; }
		}

		public virtual PageSummary ToSummary(MarkupConverter markupConverter)
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
