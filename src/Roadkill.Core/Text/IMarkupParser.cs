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
		/// The start and end tokens to indicate bold text.
		/// </summary>
		string BoldToken { get; }

		/// <summary>
		/// The start end end tokens to underline italic text.
		/// </summary>
		string UnderlineToken { get; }

		/// <summary>
		/// The start end end tokens to indicate italic text.
		/// </summary>
		string ItalicToken { get; }

		/// <summary>
		/// The start token for a link.
		/// </summary>
		string LinkStartToken { get; }

		/// <summary>
		/// The ending token for a link.
		/// </summary>
		string LinkEndToken { get; }

		/// <summary>
		/// The start token for an image.
		/// </summary>
		string ImageStartToken { get; }

		/// <summary>
		/// The end token for an image.
		/// </summary>
		string ImageEndToken { get; }

		/// <summary>
		/// The start token for a bulleted list item.
		/// </summary>
		string BulletListToken { get; }

		/// <summary>
		/// The start token for a n umbered list item.
		/// </summary>
		string NumberedListToken { get; }

		/// <summary>
		/// The start and end tokens for headings. H5 headings are assumed to repeat this token 5 times.
		/// </summary>
		string HeadingToken { get; }
	}
}
