using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Converters
{
	public interface IParser
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

		string BoldToken { get; }
		string UnderlineToken { get; }
		string ItalicToken { get; }
		string LinkStartToken { get; }
		string LinkEndToken { get; }
		string ImageStartToken { get; }
		string ImageEndToken { get; }
		string BulletListToken { get; }
		string NumberedListToken { get; }
		string HeadingToken { get; }
	}
}
