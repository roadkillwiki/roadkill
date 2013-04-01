using System;
using Mindscape.LightSpeed;

namespace Roadkill.Core.Database.LightSpeed
{
	[Table("roadkill_pagecontent")]
	internal class PageContentEntity : Entity<Guid>
	{
		private string _text;
		private string _editedBy;
		private DateTime _editedOn;
		private int _versionNumber;

		[ReverseAssociation("PageId")]
		private readonly EntityHolder<PageEntity> _page = new EntityHolder<PageEntity>();
		private int _pageId;

		public PageEntity Page
		{
			get
			{
				return Get(_page);
			}
			set
			{
				Set(_page, value);
			}
		}

		public int PageId
		{
			get { return Get(ref _pageId, "PageId"); }
			set { Set(ref _pageId, value, "PageId"); }
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				Set<string>(ref _text, value);
			}
		}

		public string EditedBy
		{
			get
			{
				return _editedBy;
			}
			set
			{
				Set<string>(ref _editedBy, value);
			}
		}

		public DateTime EditedOn
		{
			get
			{
				return _editedOn;
			}
			set
			{
				Set<DateTime>(ref _editedOn, value);
			}
		}

		public int VersionNumber
		{
			get
			{
				return _versionNumber;
			}
			set
			{
				Set<int>(ref _versionNumber, value);
			}
		}
	}
}
