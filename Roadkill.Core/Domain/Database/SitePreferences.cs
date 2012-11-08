using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NHibernate;

namespace Roadkill.Core
{
	/// <summary>
	/// Contains all configuration data stored with NHibernate/the database, for settings that do not 
	/// require an application restart when changed. This object is intended for internal use only.
	/// </summary>
	public class SitePreferences
	{
		internal static readonly Guid ConfigurationId = new Guid("b960e8e5-529f-4f7c-aee4-28eb23e13dbd");

		/// <summary>
		/// The files types allowed for uploading.
		/// </summary>
		public virtual string AllowedFileTypes { get; set; }

		/// <summary>
		/// Whether users can register themselves, or if the administrators should do it. 
		/// If windows authentication is enabled, this setting is ignored.
		/// </summary>
		public virtual bool AllowUserSignup { get; set; }

		/// <summary>
		/// Whether to Recaptcha is enabled for user signups and password resets.
		/// </summary>
		public virtual bool IsRecaptchaEnabled { get; set; }

		/// <summary>
		/// Used to keep NHibernate happy
		/// </summary>
		public virtual Guid Id { get; set; }

		/// <summary>
		/// The type of markup used: Three available options are: Creole, Markdown, MediaWiki.
		/// The default is Creole.
		/// </summary>
		/// <remarks>This is a string because it's easier with the Javascript interaction.</remarks>
		public virtual string MarkupType { get; set; }

		/// <summary>
		/// The private key for the recaptcha service, if enabled. This is optained when you sign up for the free service at https://www.google.com/recaptcha/.
		/// </summary>
		public virtual string RecaptchaPrivateKey { get; set; }

		/// <summary>
		/// The public key for the recaptcha service, if enabled. This is optained when you sign up for the free service at https://www.google.com/recaptcha/.
		/// </summary>
		public virtual string RecaptchaPublicKey { get; set; }

		/// <summary>
		/// The full url of the site.
		/// </summary>
		public virtual string SiteUrl { get; set; }

		/// <summary>
		/// The title of the site.
		/// </summary>
		public virtual string SiteName { get; set; }

		/// <summary>
		/// The site theme, defaults to "Blackbar"
		/// </summary>
		public virtual string Theme { get; set; }

		/// <summary>
		/// The current version of Roadkill. This is used for upgrades.
		/// </summary>
		public virtual string Version { get; set; }

		public virtual string ThemePath
		{
			get
			{
				return string.Format("~/Themes/{0}", Theme);
			}
		}

		public virtual List<string> AllowedFileTypesList
		{
			get
			{
				return new List<string>(AllowedFileTypes.Replace(" ", "").Split(','));
			}
		}

		public SitePreferences()
		{
			Id = ConfigurationId;
		}
	}

	public class SitePreferencesMap : ClassMap<SitePreferences>
	{
		public SitePreferencesMap()
		{
			Table("roadkill_siteconfiguration");
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.AllowedFileTypes);
			Map(x => x.AllowUserSignup);
			Map(x => x.IsRecaptchaEnabled).Column("EnableRecaptcha");
			Map(x => x.MarkupType);
			Map(x => x.RecaptchaPrivateKey);
			Map(x => x.RecaptchaPublicKey);
			Map(x => x.SiteUrl);
			Map(x => x.SiteName).Column("Title");
			Map(x => x.Theme);
			Map(x => x.Version);

			Cache.ReadWrite().IncludeAll();
		}
	}
}
