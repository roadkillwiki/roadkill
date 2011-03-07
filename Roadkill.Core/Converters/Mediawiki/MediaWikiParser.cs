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
		protected override string _processBoldCreole(string markup)
		{
			return _processBracketingCreole("'''", _getStartTag("<strong>"), "</strong>", markup);
		}

		protected override string _processItalicCreole(string markup)
		{
			return _processBracketingCreole("''", _getStartTag("<em>"), "</em>", markup);
		}

		protected override string _processImageCreole(string markup)
		{
			int iPos = _indexOfWithSkip(markup, "[[", 0);
			while (iPos >= 0)
			{
				int iEnd = _indexOfWithSkip(markup, "]]", iPos);
				if (iEnd > iPos)
				{
					iPos += 2;
					string innards = markup.Substring(iPos, iEnd - iPos);
					string href = innards;
					string text = href;
					int iSplit = innards.IndexOf('|');

					int pipeCount = innards.Split('|').Length;
					if (pipeCount >= 3)
					{
						if (iSplit > 0)
						{
							href = innards.Substring(0, iSplit);
							text = _processCreoleFragment(innards.Substring(iSplit + 1));
						}

						markup = markup.Substring(0, iPos - 2)
								+ String.Format("<img src='{0}' alt='{1}'/>", href, text)
								+ markup.Substring(iEnd + 2);
					}
				}
				else
					break;
				iPos = _indexOfWithSkip(markup, "[[", iPos);
			}
			return markup;
		}
	}
}