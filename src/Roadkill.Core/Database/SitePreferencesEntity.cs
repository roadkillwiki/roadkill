using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	public class SitePreferencesEntity : DataStoreEntity
	{
		internal static readonly Guid ConfigurationId = new Guid("b960e8e5-529f-4f7c-aee4-28eb23e13dbd");

		public virtual Guid Id { get; set; }

		public virtual string Xml { get; set; }

		/// <summary>
		/// The current version of Roadkill. This is used for upgrades.
		/// </summary>
		public virtual string Version { get; set; }

		public override Guid ObjectId
		{
			get { return Id; }
			set { Id = value; }
		}

		public SitePreferencesEntity()
		{
			Id = ConfigurationId;
		}
	}
}
