using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// Implements a parser for Media Wiki syntax markup. The base <see cref="CreoleParser"/>'s behaviour is changed for certain tokens.
	/// </summary>
	public class MediaWikiParser : CreoleParser
	{
		private static Regex _imageRegex = new Regex(@"\[\[File:(.*?)\]\]", RegexOptions.IgnoreCase);

		/// <summary>
		/// The start and end tokens to indicate bold text.
		/// </summary>
		public override string BoldToken
		{
			get { return "'''"; }
		}

		/// <summary>
		/// The start end end tokens to indicate italic text.
		/// </summary>
		public override string ItalicToken
		{
			get { return "''"; }
		}

		/// <summary>
		/// The start end end tokens to underline italic text.
		/// </summary>
		public override string UnderlineToken
		{
			get { return ""; }
		}

		/// <summary>
		/// Gets the link start token.
		/// </summary>
		public override string LinkStartToken
		{
			get { return "[[%URL%"; }
		}

		/// <summary>
		/// The ending token for a link.
		/// </summary>
		public override string LinkEndToken
		{
			get { return "|%LINKTEXT%]]"; }
		}

		/// <summary>
		/// The start token for an image.
		/// </summary>
		public override string ImageStartToken
		{
			get { return "[[File:%FILENAME%"; }
		}

		/// <summary>
		/// The end token for an image.
		/// </summary>
		public override string ImageEndToken
		{
			get { return "|%ALT%]]"; }
		}

		/// <summary>
		/// The start token for a bulleted list item.
		/// </summary>
		public override string BulletListToken
		{
			get { return "*"; }
		}

		/// <summary>
		/// The start token for a n umbered list item.
		/// </summary>
		public override string NumberedListToken
		{
			get { return "#"; }
		}

		/// <summary>
		/// Processes all bold (''') tokens
		/// </summary>
		protected override string _processBoldCreole(string markup)
		{
			return _processBracketingCreole("'''", _getStartTag("<strong>"), "</strong>", markup);
		}

		/// <summary>
		/// Processes all italic ('') tokens
		/// </summary>
		protected override string _processItalicCreole(string markup)
		{
			return _processBracketingCreole("''", _getStartTag("<em>"), "</em>", markup);
		}

		/// <summary>
		/// Convert Media Wiki markup to HTML.
		/// </summary>
		public override string Transform(string transform)
		{
			if (string.IsNullOrEmpty(transform))
				return "";

			transform = ConvertToCreole(transform);
			return base.Transform(transform);
		}

		/// <summary>
		/// Does a crude conversion from media wiki format to creole. The output is then parsed as Creole.
		/// </summary>
		/// <remarks>
		/// Unsupported features with mediawiki right now:
		/// - Definition lists
		/// - Formatting using a space at the start of the line
		/// - Tables
		/// - Image captions, and 'option' e.g. width
		/// </remarks>
		private string ConvertToCreole(string text)
		{
			// For now, converting the nowiki tags to the Creole syntax is easier
			// than trying to alter the CreoleParser's isoteric parsing of {{{ }}}
			text = text.Replace("<nowiki>", "{{{");
			text = text.Replace("</nowiki>", "}}}");

			//
			// Images
			// Mediawiki image syntax:[[File:filename.extension|options|caption]]
			//
			text = _imageRegex.Replace(text, delegate(Match match)
			{
				return "{{" + match.Groups[1].Value + "}}";
			});

			return text;
		}
	}
}