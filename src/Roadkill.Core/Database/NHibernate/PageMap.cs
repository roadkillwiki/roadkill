using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NHibernate;

namespace Roadkill.Core
{
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
