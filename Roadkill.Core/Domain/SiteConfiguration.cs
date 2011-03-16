using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BottleBank;
using FluentNHibernate.Mapping;
using NHibernate;

namespace Roadkill.Core
{
	public class SiteConfiguration : NHibernateObject<SiteConfiguration, SiteConfigurationRepository>
	{
		/// <summary>
		/// The title of the site.
		/// </summary>
		public virtual string Title { get; set; }

		/// <summary>
		/// The site theme, defaults to "Blackbar"
		/// </summary>
		public virtual string Theme { get; set; }

		/// <summary>
		/// The type of markup used: Three available options are: Creole, Markdown, MediaWiki.
		/// The default is Creole.
		/// </summary>
		/// <remarks>This is a string because it's easier with the Javascript interaction.</remarks>
		public virtual string MarkupType { get; set; }

		/// <summary>
		/// The files types allowed for uploading.
		/// </summary>
		public virtual string AllowedFileTypes { get; set; }
		
		/// <summary>
		/// Whether users can register themselves, or if the administrators should do it.
		/// </summary>
		public virtual bool AllowUserSignup { get; set; }

		/// <summary>
		/// The configuration class should be a singleton, this retrieves it.
		/// </summary>
		public static SiteConfiguration Current
		{
			get
			{
				return SiteConfiguration.Repository.Manager().Queryable<SiteConfiguration>().FirstOrDefault();
			}
		}
	}

	public class SiteConfigurationMap : ClassMap<SiteConfiguration>
	{
		public SiteConfigurationMap()
		{
			Table("roadkill_siteconfiguration");
			Id();
			Map(x => x.Title);
			Map(x => x.Theme);
			Map(x => x.MarkupType);
			Map(x => x.AllowedFileTypes);
			Map(x => x.AllowUserSignup);
		}
	}

	public class SiteConfigurationRepository : Repository<SiteConfiguration, SiteConfigurationRepository>
	{
	}
}
