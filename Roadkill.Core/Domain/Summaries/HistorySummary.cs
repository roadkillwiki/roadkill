using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides summary data for a page's version history.
	/// </summary>
	public class HistorySummary
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
		/// When this version was created.
		/// </summary>
		public DateTime EditedOn { get; set; }

		/// <summary>
		/// Whether the page can only be edited by administrators. This disables the "revert" behaviour.
		/// </summary>
		public bool IsPageAdminOnly { get; set; }
	}
}
