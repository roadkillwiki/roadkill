using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Database;

namespace Roadkill.Core.Mvc.ViewModels
{
	/// <summary>
	/// Provides summary data for a page's version history.
	/// </summary>
	public class PageHistoryViewModel
	{
		/// <summary>
		/// The unique Guid for the version.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The id of the page.
		/// </summary>
		public int PageId { get; set; }

		/// <summary>
		/// The version number for the version.
		/// </summary>
		public int VersionNumber { get; set; }

		/// <summary>
		/// Who edited this version.
		/// </summary>
		public string EditedBy { get; set; }

		/// <summary>
		/// When this version was created. This should always be a UTC time / date.
		/// </summary>
		public DateTime EditedOn { get; set; }

		/// <summary>
		/// Displays Editedon in IS8601 format, plus the timezone offset included
		/// </summary>
		public string EditedOnWithOffset
		{
			get
			{
				// EditedOn (ModifiedOn in the domain) is stored in UTC time, so just add a Z to indicate this.
				return string.Format("{0}Z", EditedOn.ToString("s"));
			}
		}

		/// <summary>
		/// Whether the page can only be edited by administrators. This disables the "revert" behaviour.
		/// </summary>
		public bool IsPageAdminOnly { get; set; }

		public PageHistoryViewModel()
		{
			EditedOn = DateTime.UtcNow;
		}

		/// <summary>
		/// Fills this instance using the <see cref="PageContent"/> object.
		/// </summary>
		/// <exception cref="ArgumentNullException">PageContent.Page should not be null.</exception>
		public PageHistoryViewModel(PageContent pageContent)
		{
			if (pageContent.Page == null)
				throw new ArgumentNullException("pageContent.Page should not be null");

			Id = pageContent.Id;
			PageId = pageContent.Page.Id;
			EditedBy = pageContent.EditedBy;
			EditedOn = pageContent.EditedOn;
			VersionNumber = pageContent.VersionNumber;
			IsPageAdminOnly = pageContent.Page.IsLocked;
		}
	}
}
