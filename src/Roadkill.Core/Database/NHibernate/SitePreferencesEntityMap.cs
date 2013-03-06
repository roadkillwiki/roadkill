using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NHibernate;

namespace Roadkill.Core
{
	public class SitePreferencesEntityMap : ClassMap<SitePreferencesEntity>
	{
		public SitePreferencesEntityMap()
		{
			Table("roadkill_siteconfiguration");
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Version);
			Map(x => x.Xml).CustomType("StringClob").Length(Int16.MaxValue);

			Cache.ReadWrite().IncludeAll();
		}
	}
}
