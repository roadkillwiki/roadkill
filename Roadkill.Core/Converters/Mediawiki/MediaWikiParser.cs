/*
 * 
 * This is an altered version of Tom Laird-McConnell's CreoleParser, changing the bold tag
 * and some (not very robust) alterations for media-wiki markup links. As creole is the default
 * wiki format, hopefully this parser won't be used that often as this the media-wiki wiki markup
 * syntax is full of silly edge cases.
 * 
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Roadkill.Core.Converters.MediaWiki
{
	public class MediaWikiParser : Creole.CreoleParser
	{
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
			NoWikiEscapeStart = "<nowiki>";
			NoWikiEscapeEnd = "</nowiki>";
		}

		protected override string _processBoldCreole(string markup)
		{
			return _processBracketingCreole("'''", _getStartTag("<strong>"), "</strong>", markup);
		}

		protected override string _processItalicCreole(string markup)
		{
			return _processBracketingCreole("''", _getStartTag("<em>"), "</em>", markup);
		}

		static int _imgTagLen = "[[File:".Length;

		/// <summary>
		/// This is very good proof why the parsing for creole and mediwiki needs a grammar based parsed
		/// like grammatica. For now it sort of works.
		/// </summary>
		/// <param name="markup"></param>
		/// <returns></returns>
		protected override string _processImageCreole(string markup)
		{
			int iPos = markup.IndexOf("[[File:");
			while (iPos >= 0)
			{
				int iEnd = markup.IndexOf("]]", iPos);
				if (iEnd > iPos)
				{
					iPos += _imgTagLen;
					string innards = markup.Substring(iPos, iEnd - iPos);
					string href = innards;
					string text = href;
					int iSplit = innards.IndexOf('|');

					int pipeCount = innards.Split('|').Length;
					if (pipeCount >= 2)
					{
						if (iSplit > 0)
						{
							href = innards.Substring(0, iSplit);
							text = _processCreoleFragment(innards.Substring(iSplit + 1));
						}

						ImageEventArgs args = new ImageEventArgs(href,href,text,"");
						OnImageParsed(args);

						int start = iPos - _imgTagLen;
						int length = (iEnd + 2) - start;
						if (start <= markup.Length && length > start && length <= markup.Length)
						{
							string found = markup.Substring(start,length);
							markup = markup.Replace(found, string.Format("<img src=\"{0}\" alt=\"{1}\" />", args.Src, args.Alt));
						}
					}
				}
				else
					break;
				iPos = markup.IndexOf("[[File:");
			}
			return markup;
		}
	}
}