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
	public class Page
	{
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
		public virtual string Tags { get; set; }
		public virtual bool IsLocked { get; set; }

		public virtual PageContent CurrentContent()
		{
			if (RoadkillSettings.DatabaseType == DatabaseType.SqlServerCe)
				return ContentForSqlCeBug();

			// Fetches the parent page object via SQL as well as the PageContent, avoiding lazy loading.
			IQuery query = NHibernateRepository.Current.SessionFactory.OpenSession()
					.CreateQuery("FROM PageContent fetch all properties WHERE Page.Id=:Id AND VersionNumber=(SELECT max(VersionNumber) FROM PageContent WHERE Page.Id=:Id)");

			query.SetCacheable(true);
			query.SetInt32("Id", Id);
			query.SetMaxResults(1);
			PageContent content = query.UniqueResult<PageContent>();

			return content;
		}

		/// <summary>
		/// Work around for an NHibernate 3.3.1 SQL CE bug with the HQL query in CurrentContent() - this is two SQL queries per page instead of one.
		/// </summary>
		/// <returns></returns>
		private PageContent ContentForSqlCeBug()
		{
			PageContent latest;
			using (ISession session = NHibernateRepository.Current.SessionFactory.OpenSession())
			{
				latest = session.QueryOver<PageContent>().OrderBy(p => p.VersionNumber).Desc.Take(1).SingleOrDefault();
				latest.Page = session.Get<Page>(latest.Page.Id);
			}

			return latest;
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
				PreviousTitle = Title,
				CreatedBy = CreatedBy,
				CreatedOn = CreatedOn,
				IsLocked = IsLocked,
				ModifiedBy = ModifiedBy,
				ModifiedOn = ModifiedOn,
				Tags = Tags,
				Content = content.Text,
				VersionNumber = content.VersionNumber,
			};
		}
	}

	/// <summary>
	/// Configures the Fluent NHibernate mapping for a <see cref="Page"/>
	/// </summary>
	public class PageMap : ClassMap<Page>
	{
		public PageMap()
		{
			Table("roadkill_pages");
			Id(x => x.Id).GeneratedBy.Identity();
			Map(x => x.Title);
			Map(x => x.Tags);
			Map(x => x.CreatedBy);
			Map(x => x.CreatedOn);
			Map(x => x.IsLocked);
			Map(x => x.ModifiedBy);
			Map(x => x.ModifiedOn);
			Cache.ReadWrite().IncludeAll();
		}
	}
}
