using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NHibernate;

namespace Roadkill.Core
{
	public class SitePreferencesMap : ClassMap<SitePreferencesEntity>
	{
		public SitePreferencesMap()
		{
			Table("roadkill_siteconfiguration");
			Id(x => x.Id).GeneratedBy.Assigned();
			//Map(x => x.AllowedFileTypes);
			//Map(x => x.AllowUserSignup);
			//Map(x => x.IsRecaptchaEnabled).Column("EnableRecaptcha");
			//Map(x => x.MarkupType);
			//Map(x => x.RecaptchaPrivateKey);
			//Map(x => x.RecaptchaPublicKey);
			//Map(x => x.SiteUrl);
			//Map(x => x.SiteName).Column("Title");
			//Map(x => x.Theme);
			Map(x => x.Version);
			Map(x => x.Xml).CustomType("StringClob").Length(Int16.MaxValue);

			Cache.ReadWrite().IncludeAll();
		}
	}
}
