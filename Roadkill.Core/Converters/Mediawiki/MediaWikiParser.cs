/*
 * 
 * This is an altered version of Tom Laird-McConnell's CreoleParser, changing the bold tag
 * and some (not very robust) alterations for media-wiki markup links. As creole is the default
 * wiki format, hopefully this parser won't be used that often as the media-wiki wiki markup
 * syntax is full of silly edge cases.
 * 
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Converters.MediaWiki
{
	public class MediaWikiParser : Creole.CreoleParser
	{
		private static Regex _imageRegex = new Regex(@"\[\[File:(.*?)\]\]", RegexOptions.IgnoreCase);

		public override string BoldToken
		{
			get { return "'''"; }
		}

		public override string ItalicToken
		{
			get { return "''"; }
		}

		public override string UnderlineToken
		{
			get { return ""; }
		}

		public override string LinkStartToken
		{
			get { return "[[%URL%"; }
		}

		public override string LinkEndToken
		{
			get { return "|%LINKTEXT%]]"; }
		}

		public override string ImageStartToken
		{
			get { return "[[File:%FILENAME%"; }
		}

		public override string ImageEndToken
		{
			get { return "|%ALT%]]"; }
		}

		public override string BulletListToken
		{
			get { return "*"; }
		}

		public override string NumberedListToken
		{
			get { return "#"; }
		}

		public MediaWikiParser()
		{
		}

		protected override string _processBoldCreole(string markup)
		{
			return _processBracketingCreole("'''", _getStartTag("<strong>"), "</strong>", markup);
		}

		protected override string _processItalicCreole(string markup)
		{
			return _processBracketingCreole("''", _getStartTag("<em>"), "</em>", markup);
		}

		public override string Transform(string transform)
		{
			transform = ConvertToCreole(transform);
			return base.Transform(transform);
		}

		/// <summary>
		/// Mediawiki image syntax:[[File:filename.extension|options|caption]]
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
			//
			text = _imageRegex.Replace(text, delegate(Match match)
			{
				return "{{" + match.Groups[1].Value + "}}";
			});

			return text;
		}
	}
}