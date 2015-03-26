namespace Roadkill.Core.Converters
{
	/// <summary>
	/// Contains help documentation for a parser.
	/// </summary>
	public class MarkupParserHelp
	{
		/// <summary>
		/// The start and end tokens to indicate bold text.
		/// </summary>
		public string BoldToken { get; set; }

		/// <summary>
		/// The start end end tokens to underline italic text.
		/// </summary>
		public string UnderlineToken { get; set; }

		/// <summary>
		/// The start end end tokens to indicate italic text.
		/// </summary>
		public string ItalicToken { get; set; }

		/// <summary>
		/// The start token for a link.
		/// </summary>
		public string LinkStartToken { get; set; }

		/// <summary>
		/// The ending token for a link.
		/// </summary>
		public string LinkEndToken { get; set; }

		/// <summary>
		/// The start token for an image.
		/// </summary>
		public string ImageStartToken { get; set; }

		/// <summary>
		/// The end token for an image.
		/// </summary>
		public string ImageEndToken { get; set; }

		/// <summary>
		/// The start token for a bulleted list item.
		/// </summary>
		public string BulletListToken { get; set; }

		/// <summary>
		/// The start token for a n umbered list item.
		/// </summary>
		public string NumberedListToken { get; set; }

		/// <summary>
		/// The start and end tokens for headings. H5 headings are assumed to repeat this token 5 times.
		/// </summary>
		public string HeadingToken { get; set; }
	}
}