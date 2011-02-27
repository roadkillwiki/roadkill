using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BottleBank;
using FluentNHibernate.Mapping;
using NHibernate;

namespace Roadkill.Core
{
	public class Page : NHibernateObject<Page, PageRepository>
	{
		public virtual Guid Id { get; set; }
		public virtual string Title { get; set; }
		public virtual string CreatedBy { get; set; }
		public virtual DateTime CreatedOn { get; set; }
		public virtual string ModifiedBy { get; set; }
		public virtual DateTime ModifiedOn { get; set; }
		public virtual string Tags { get; set; }

		public virtual PageContent CurrentContent()
		{
				IQuery query = PageContent.Repository.Manager().SessionFactory.OpenSession()
					.CreateQuery("FROM PageContent fetch all properties WHERE Page.Id=:Id AND VersionNumber=(SELECT max(VersionNumber) FROM PageContent WHERE Page.Id=:Id)");

			query.SetGuid("Id", Id);
			query.SetMaxResults(1);
			PageContent content = query.UniqueResult<PageContent>();

			return content;
		}

		public virtual PageSummary ToSummary()
		{
			PageContent content = CurrentContent();
			return ToSummary(content);
		}

		public virtual PageSummary ToSummary(PageContent content)
		{
			return new PageSummary()
			{
				Id = Id,
				Title = Title,
				CreatedBy = CreatedBy,
				CreatedOn = CreatedOn,
				ModifiedBy = ModifiedBy,
				ModifiedOn = ModifiedOn,
				Tags = Tags.Replace(";", " ").Trim(),
				Content = content.Text,
				VersionNumber = content.VersionNumber
			};
		}
	}

	public class PageMap : ClassMap<Page>
	{
		public PageMap()
		{
			Table("roadkill_pages");
			Id(x => x.Id);
			Map(x => x.Title);
			Map(x => x.Tags);
			Map(x => x.CreatedBy);
			Map(x => x.CreatedOn);
			Map(x => x.ModifiedBy);
			Map(x => x.ModifiedOn);
		}
	}

	public class PageRepository : Repository<Page, PageRepository>
	{
	}
}
