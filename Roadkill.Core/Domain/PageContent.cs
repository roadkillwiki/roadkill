using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BottleBank;
using FluentNHibernate.Mapping;

namespace Roadkill.Core
{
	public class PageContent : NHibernateObject<PageContent, PageContentRepository>
	{
		public virtual Guid Id { get; set; }
		public virtual Page Page { get; set; }
		public virtual string Text { get; set; }
		public virtual string EditedBy { get; set; }
		public virtual DateTime EditedOn { get; set; }
		public virtual int VersionNumber { get; set; }
	}

	public class PageContentMap : ClassMap<PageContent>
	{
		public PageContentMap()
		{
			Table("roadkill_pagecontent");
			Id(x => x.Id);
			Map(x => x.Text).CustomType("StringClob").CustomSqlType("text").LazyLoad();
			Map(x => x.EditedBy);
			Map(x => x.EditedOn);
			Map(x => x.VersionNumber);
			References<Page>(x => x.Page)
				.Column("pageid")
				.Cascade
				.None();
		}
	}

	public class PageContentRepository : Repository<PageContent, PageContentRepository>
	{
	}
}
