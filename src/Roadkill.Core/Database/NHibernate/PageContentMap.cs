using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;

namespace Roadkill.Core.Database.NHibernate
{
	/// <summary>
	/// Configures the Fluent NHibernate mapping for a <see cref="PageContent"/>
	/// </summary>
	public class PageContentMap : ClassMap<PageContent>
	{
		public PageContentMap()
		{
			Table("roadkill_pagecontent");
			Id(x => x.Id);
			Map(x => x.EditedBy);
			Map(x => x.EditedOn);
			Map(x => x.VersionNumber);
			Map(x => x.Text).CustomType("StringClob").Length(Int16.MaxValue).LazyLoad();

			References<Page>(x => x.Page)
				.Column("pageid")
				.Cascade
				.None().Index("pageId");

			Cache.ReadWrite().IncludeAll();
		}
	}
}
