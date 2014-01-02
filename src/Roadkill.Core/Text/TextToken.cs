using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Text.RegularExpressions;

namespace Roadkill.Core
{
	/// <summary>
	/// Represents a single token for text replacement inside the wiki markup, an array of
	/// object is used for the custom-tokens.xml.
	/// </summary>
	public class TextToken
	{
		/// <summary>
		/// The name of the token, for use as in-line help.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The description of the token, for use as in-line help.
		/// </summary>
		public string Description { get; set; }
		
		/// <summary>
		/// The regex to search the text with. This is a single line, compiled regex.
		/// </summary>
		public string SearchRegex { get; set; }

		/// <summary>
		/// The HTML replacement for the search regex.
		/// </summary>
		public string HtmlReplacement { get; set; }

		/// <summary>
		/// Whether to strip the contents of the token for any unsafe HTML (defaults to false).
		/// </summary>
		public bool SanitizeContent { get; set; }

		/// <summary>
		/// The cache regex for the token
		/// </summary>
		[XmlIgnore]
		public Regex CachedRegex { get; set; }
	}
}
