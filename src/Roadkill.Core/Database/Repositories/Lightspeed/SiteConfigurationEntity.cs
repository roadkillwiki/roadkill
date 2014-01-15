using System;
using Mindscape.LightSpeed;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Database.LightSpeed
{
	[Table("roadkill_siteconfiguration")]
	[Cached(ExpiryMinutes = 10)]
	public class SiteConfigurationEntity : Entity<Guid>
	{
		[Column("content")]
		private string _content;

		[Column("version")]
		private string _version;

		public string Content
		{
			get
			{
				return _content;
			}
			set
			{
				Set<string>(ref _content, value);
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
