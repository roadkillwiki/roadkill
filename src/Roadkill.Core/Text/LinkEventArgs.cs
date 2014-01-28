using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// Holds information when a hyperlink is processed, giving the caller the ability to translate the outputted HTML.
	/// </summary>
	public class LinkEventArgs : EventArgs
	{
		/// <summary>
		/// The original href.
		/// </summary>
		public string OriginalHref { get; set; }

		/// <summary>
		/// The href to use for the HTML.
		/// </summary>
		public string Href { get; set; }

		/// <summary>
		/// The link text.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The target attribute, e.g. _blank.
		/// </summary>
		public string Target { get; set; }

		/// <summary>
		/// The css class attribute.
		/// </summary>
		public string CssClass { get; set; }

		/// <summary>
		/// True if the link points to another page in the wiki, including Special: urls, and attachments.
		/// </summary>
		public bool IsInternalLink { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkEventArgs"/> class.
		/// </summary>
		public LinkEventArgs(string originalHref, string href, string text, string target)
		{
			OriginalHref = originalHref;
			Href = href;
			Text = text;
			Target = target;
			CssClass = "";
			IsInternalLink = false;
		}
	}
}
