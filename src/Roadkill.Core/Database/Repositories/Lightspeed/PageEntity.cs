using System;
using Mindscape.LightSpeed;

namespace Roadkill.Core.Database.LightSpeed
{
	[Table("roadkill_pages", IdentityMethod=IdentityMethod.IdentityColumn)]
	internal class PageEntity : Entity<int>
	{
		private string _title;
		private string _createdBy;
		private DateTime _createdOn;
		private string _modifiedBy;
		private DateTime _modifiedOn;
		private string _tags;
		private bool _isLocked;

		[ReverseAssociation("PageContents")]
		private readonly EntityCollection<PageContentEntity> _pageContents = new EntityCollection<PageContentEntity>();

		public EntityCollection<PageContentEntity> PageContents
		{
			get { return Get(_pageContents); }
		}

		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				Set<string>(ref _title, value);
			}
		}

		public string CreatedBy
		{
			get
			{
				return _createdBy;
			}
			set
			{
				Set<string>(ref _createdBy, value);
			}
		}

		public DateTime CreatedOn
		{
			get
			{
				return _createdOn;
			}
			set
			{
				Set<DateTime>(ref _createdOn, value);
			}
		}

		public string ModifiedBy
		{
			get
			{
				return _modifiedBy;
			}
			set
			{
				Set<string>(ref _modifiedBy, value);
			}
		}

		public DateTime ModifiedOn
		{
			get
			{
				return _modifiedOn;
			}
			set
			{
				Set<DateTime>(ref _modifiedOn, value);
			}
		}

		public string Tags
		{
			get
			{
				return _tags;
			}
			set
			{
				Set<string>(ref _tags, value);
			}
		}

		public bool IsLocked
		{
			get
			{
				return _isLocked;
			}
			set
			{
				Set<bool>(ref _isLocked, value);
			}
		}
	}
}
