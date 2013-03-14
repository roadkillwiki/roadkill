using System;
using Mindscape.LightSpeed;

namespace Roadkill.Core.Database.LightSpeed
{
	[Table("roadkill_siteconfiguration")]
	[Cached(ExpiryMinutes = 15)]
	internal class SitePreferencesEntity : Entity<Guid>
	{
		private string _xml;
		private string _version;

		public Guid Id
		{
			get
			{
				return SitePreferences.SitePreferencesId;
			}
		}


		public string Xml
		{
			get
			{
				return _xml;
			}
			set
			{
				Set<string>(ref _xml, value);
			}
		}


		public string Version
		{
			get
			{
				return _version;
			}
			set
			{
				Set<string>(ref _version, value);
			}
		}
	}
}
