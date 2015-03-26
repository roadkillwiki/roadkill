using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// Represents a class that can convert a markup syntax into HTML. The markups syntax 
	/// should include formatting support as well as images and links.
	/// </summary>
	public interface IMarkupParser
	{
		/// <summary>
		/// Transforms the provided specific markup text to HTML
		/// </summary>
		string Transform(string transform);

		/// <summary>
		/// Occurs when an image tag is parsed.
		/// </summary>
		event EventHandler<ImageEventArgs> ImageParsed;

		/// <summary>
		/// Occurs when a hyperlink is parsed.
		/// </summary>
		event EventHandler<LinkEventArgs> LinkParsed;

		/// <summary>
		/// Help/documentation for the parser's tokens.
		/// </summary>
		MarkupParserHelp MarkupParserHelp { get; }
	}
}
