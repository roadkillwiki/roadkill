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
	public class SiteConfiguration
	{
		private static Guid _configurationId = new Guid("b960e8e5-529f-4f7c-aee4-28eb23e13dbd");
		[ThreadStatic]
		private static bool _initialized;

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
		public virtual bool EnableRecaptcha { get; set; }

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
		public virtual string Title { get; set; }

		/// <summary>
		/// The site theme, defaults to "Blackbar"
		/// </summary>
		public virtual string Theme { get; set; }

		/// <summary>
		/// The current version of Roadkill. This is used for upgrades.
		/// </summary>
		public virtual string Version { get; set; }

		public SiteConfiguration()
		{
			Id = _configurationId;
		}

		/// <summary>
		/// The configuration class should be a singleton, this retrieves it.
		/// </summary>
		public static SiteConfiguration Current
		{
			get
			{
				if (!_initialized)
					Initialize(null);

				return Nested.Current;
			}
		}

		/// <summary>
		/// Re-initializes the SiteConfiguration's singleton instance.
		/// </summary>
		/// <param name="configuration">The SiteConfiguration type to re-initialize with.</param>
		public static void Initialize(SiteConfiguration configuration)
		{
			Nested.Initialize(configuration);
			_initialized = true;
		}

		/// <summary>
		/// Singleton for Current
		/// </summary>
		class Nested
		{
			internal static SiteConfiguration Current;

			public static void Initialize(SiteConfiguration configuration)
			{
				if (configuration == null)
				{
					Current = NHibernateRepository.Current.Queryable<SiteConfiguration>().FirstOrDefault(s => s.Id == _configurationId);

					if (Current == null)
						throw new DatabaseException(null, "No configuration settings could be found in the database (id {0}). " +
							"Has SettingsManager.SaveSiteConfiguration() been called?", _configurationId);
				}
				else
					Current = configuration;
			}
		}
	}

	public class SiteConfigurationMap : ClassMap<SiteConfiguration>
	{
		public SiteConfigurationMap()
		{
			Table("roadkill_siteconfiguration");
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.AllowedFileTypes);
			Map(x => x.AllowUserSignup);
			Map(x => x.EnableRecaptcha);
			Map(x => x.MarkupType);
			Map(x => x.RecaptchaPrivateKey);
			Map(x => x.RecaptchaPublicKey);
			Map(x => x.SiteUrl);
			Map(x => x.Title);
			Map(x => x.Theme);
			Map(x => x.Version);

			Cache.ReadWrite().IncludeAll();
		}
	}
}
