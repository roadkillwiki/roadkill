using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FluentNHibernate.Mapping;
using NHibernate;

namespace Roadkill.Core
{
	/// <summary>
	/// Contains all configuration data stored with NHibernate/the database, for settings that do not require an application restart when changed.
	/// </summary>
	[Serializable]
	public class SitePreferences
	{
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
		/// An asp.net relativate path e.g. ~/Themes/ to the current theme directory. Does not include a trailing slash.
		/// </summary>
		[XmlIgnore]
		public virtual string ThemePath
		{
			get
			{
				return string.Format("~/Themes/{0}", Theme);
			}
		}

		/// <summary>
		/// Retrieves a list of the file extensions that are permitted for upload.
		/// </summary>
		[XmlIgnore]
		public virtual List<string> AllowedFileTypesList
		{
			get
			{
				return new List<string>(AllowedFileTypes.Replace(" ", "").Split(','));
			}
		}

		public virtual string GetXml()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SitePreferences));
			StringBuilder builder = new StringBuilder();
			using (StringWriter writer = new StringWriter(builder))
			{
				serializer.Serialize(writer, this);
				return builder.ToString();
			}
		}

		public static SitePreferences LoadFromXml(string xml)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SitePreferences));
			
			using (StringReader reader = new StringReader(xml))
			{
				return (SitePreferences) serializer.Deserialize(reader);
			}
		}
	}
}
