using System.Collections.Generic;

namespace Roadkill.Core.Mvc.ViewModels
{
	/// <summary>
	/// A tag and the pages that have it, for the "pages by tag" page (/pages/alltagswithpages).
	/// </summary>
	public class TagPagesViewModel
	{
		/// <summary>
		/// The tag name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The pages that have the tag (only their id and title are set), sorted by title.
		/// </summary>
		public List<PageViewModel> Pages { get; set; } = new List<PageViewModel>();

		/// <summary>
		/// The number of pages that have the tag.
		/// </summary>
		public int Count => Pages.Count;
	}
}
