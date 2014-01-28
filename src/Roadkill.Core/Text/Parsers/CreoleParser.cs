/*
 * 
 * Microsoft Public License (Ms-PL)  See (http://www.microsoft.com/opensource/licenses.mspx#Ms-PL)
 * 
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the 
 * license, do not use the software.
 * 
 * Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law. 
 * A "contribution" is the original software, or any additions or changes to the software. A "contributor" is any person that distributes 
 * its contribution under this license. "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 
 * Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each 
 * contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare 
 * derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 * each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have 
 * made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works 
 * of the contribution in the software.
 * 
 * Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks. 
 * 
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 *      your patent license from such contributor to the software ends automatically. 
 *      
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 *      notices that are present in the software. 
 *      
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 *      a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or 
 *      object code form, you may only do so under a license that complies with this license. 
 *      
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees, 
 *      or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent 
 *      permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 *      purpose and non-infringement.
 *      
 * by Tom Laird-McConnell
 * Microsoft Corp
 * 10/28/2008
 * 
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using Roadkill.Core.Configuration;
using System.Web;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// 
	///  The Creole Parser is a .NET class which will translate Wiki Creole 1.0 (see http://wikicreole.org into HTML 
	/// 
	///  This is fully 1.0 Wiki Creole Compliant 
	///  
	/// This class also supports the following creole additions:
	/// 1. __underline__        ==> <u>underlined</u>
	/// 2. ^^super^^script      ==> <sup>super</sup>script
	/// 3. ,,sub,,script        ==> <sub>sub</sub>script
	/// 4. --strikethrough--    ==> <del>strikethrough</del>
	/// 5. TAB chars are replaced with 7 &nbsp; unless the TabStop is set to 0.
	/// 
	/// You can add Interwiki mappings by using the parser.InterWiki collection
	/// You can add additional HTML markup by adding entires to the parser.HTMLAttributes collection
	/// Ex: 
	/// parser.HTMLAttributes.Add("<thead>", "<thead style=\"border: solid=;\">");
	/// 
	/// You if you define an event handler for OnLink you can modify the link that is generated
	/// </summary>
	public class CreoleParser : IMarkupParser
	{
		private string _tabStop;
		private int _nTabSpaces;
		private ApplicationSettings _applicationSettings;

		/// <summary>
		/// The start and end tokens to indicate bold text.
		/// </summary>
		public virtual string BoldToken
		{
			get { return "**"; }
		}

		/// <summary>
		/// The start end end tokens to indicate italic text.
		/// </summary>
		public virtual string ItalicToken
		{
			get { return "//"; }
		}

		/// <summary>
		/// The start end end tokens to underline italic text.
		/// </summary>
		public virtual string UnderlineToken
		{
			get { return "__"; }
		}

		public virtual string LinkStartToken
		{
			get { return "[[%URL%|"; }
		}

		/// <summary>
		/// The ending token for a link.
		/// </summary>
		public virtual string LinkEndToken
		{
			get { return "%LINKTEXT%]]"; }
		}

		/// <summary>
		/// The start token for an image.
		/// </summary>
		public virtual string ImageStartToken
		{
			get { return "{{%FILENAME%|"; }
		}

		/// <summary>
		/// The end token for an image.
		/// </summary>
		public virtual string ImageEndToken
		{
			get { return "%ALT%}}"; }
		}

		/// <summary>
		/// The start token for a bulleted list item.
		/// </summary>
		public virtual string BulletListToken
		{
			get { return "*"; }
		}

		/// <summary>
		/// The start token for a n umbered list item.
		/// </summary>
		public virtual string NumberedListToken
		{
			get { return "#"; }
		}

		/// <summary>
		/// The start and end tokens for headings. H5 headings are assumed to repeat this token 5 times.
		/// </summary>
		public virtual string HeadingToken
		{
			get { return "="; }
		}

		/// <summary>
		/// Occurs when an image tag is parsed.
		/// </summary>
		public event EventHandler<ImageEventArgs> ImageParsed;


		/// <summary>
		/// Occurs when a hyperlink is parsed.
		/// </summary>
		public event EventHandler<LinkEventArgs> LinkParsed;

		/// <summary>
		/// Whether to append id="CreoleLine1" etc. to p tags. False by default.
		/// </summary>
		public bool AddIdToParagraphTags { get; set; }

		/// <summary>
		/// The characters that start a no wiki escape sequence. {{{ by default.
		/// </summary>
		public virtual string NoWikiEscapeStart { get; set; }

		/// <summary>
		/// The characters that end the no wiki escape sequence. }}} by default.
		/// </summary>
		public virtual string NoWikiEscapeEnd { get; set; }

		/// <summary>
		/// This collection allows you to substitute markup with your own custom markup
		/// Example:
		/// HTMLAttributes.Add("<h1>", "<h1 classid=myH1Class>");
		/// </summary>
		public Dictionary<string, string> HTMLAttributes;

		/// <summary>
		/// Interwiki dictionary
		/// You can add interwiki links by adding the prefix to url mappings 
		/// Example:
		/// InterWiki.Add("wikipedia", "http://wikipedia.org/{0}");  
		/// allows the user to do a wikipedia link like:
		/// [[wikipedia:Microsoft]] 
		/// </summary>
		public Dictionary<string, string> InterWiki { get; set; }


		/// <summary>
		/// Map \t into &nbsp; 
		/// If you set this to 0 there will be no substitution.
		/// </summary>
		public int TabStop         // number of characters for a tab
		{
			set
			{
				_nTabSpaces = value;
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < _nTabSpaces; i++)
					sb.Append("&nbsp;");
				_tabStop = sb.ToString();
			}
			get
			{
				return _nTabSpaces;
			}
		}

		public CreoleParser(ApplicationSettings applicationSettings, SiteSettings siteSettings)
		{
			_applicationSettings = applicationSettings;
			AddIdToParagraphTags = false;
			HTMLAttributes = new Dictionary<string, string>();
			InterWiki = new Dictionary<string, string>();
			TabStop = 7; // default to 7 char tabstop
			NoWikiEscapeStart = "{{{";
			NoWikiEscapeEnd = "}}}";

			if (siteSettings != null)
				InterWiki.Add("tag", siteSettings.SiteUrl + "/pages/tag/");
		}


		/// <summary>
		/// Convert creole markup to HTML
		/// </summary>
		/// <param name="creole">creole markup</param>
		/// <returns>HTML</returns>
		public virtual string Transform(string transform)
		{
			if (string.IsNullOrEmpty(transform))
				return "";

			return _processAllMarkup(transform);
		}

		#region internal Process Creole methods

		private string _processAllMarkup(string markup)
		{
			// all of the syntax of flowing formatting markup across "soft" paragraph breaks gets a lot easier if we just merge soft breaks together.
			// This is what _mergeLines does, giving us an array of "lines" which can be processed for line based formatting such as ==, etc.
			List<int> originalLineNumbers;
			List<string> lines = _breakMarkupIntoLines(markup, out originalLineNumbers);
			StringBuilder htmlMarkup = new StringBuilder();

			if (AddIdToParagraphTags)
				htmlMarkup.Append(_getStartTag("<p>").Replace("<p", "<p id='CreoleLine0'")); // start with paragraph
			else
				htmlMarkup.Append(_getStartTag("<p>"));

			int iBullet = 0;    // bullet indentation level
			int iNumber = 0;    // ordered list indentation level
			bool InTable = false;  // are in a table definition
			bool InEscape = false; // we are in an escaped section
			int idParagraph = 1;  // id for paragraphs
			bool inRoadkillEscape = false;

			// process each line of the markup, since bullets, numbers and tables depend on start of line
			foreach (string l in lines)
			{
				// make a copy as we will modify line into HTML as we go
				string line = l.Trim('\r');
				string lineTrimmed = line.TrimStart(' ');

				// if we aren't in an escaped section
				if (!InEscape && !inRoadkillEscape)
				{
					// if we were in a table definition and this isn't another row 
					if ((InTable) && (lineTrimmed.Length > 0) && lineTrimmed[0] != '|')
					{
						// then we close the table out
						htmlMarkup.Append("</table>");
						InTable = false;
					}

					// process line based commands (aka, starts with a ** --> bulleted list) 

					// ---  if we found a line completely empty, translate it as end of paragraph
					if (lineTrimmed.Trim().Length == 0)
					{
						// close any pending lists
						_closeLists(ref htmlMarkup, ref iBullet, ref iNumber, lineTrimmed);

						// end of paragraph (NOTE: we id paragraphs for conveinence sake
						if (AddIdToParagraphTags)
						{
							htmlMarkup.Append(String.Format("</p>\n{0}",
											  _getStartTag("<p>").Replace("<p", String.Format("<p id='CreoleLine{0}'", originalLineNumbers[idParagraph]))));
						}
						else
						{
							htmlMarkup.Append(String.Format("</p>\n{0}", _getStartTag("<p>")));
						}
					}
					// --- process bullets
					else if (lineTrimmed[0] == '*')
					{
						if (lineTrimmed.Length > 1 && lineTrimmed[1] == '*' && iBullet == 0)
						{
							// If we're not in a bulleted list, then this might be bold.
							htmlMarkup.AppendLine(_processCreoleFragment(line));
						}
						else
						{
							// if we were doing an ordered list and this isn't one, we need to close the previous list down
							if ((iNumber > 0) && lineTrimmed[0] != '#')
								htmlMarkup.Append(_closeList(ref iNumber, "</ol>"));

							// generate correct indentation for bullets given current state of iBullet
							htmlMarkup.Append(_processListIndentations(lineTrimmed, '*', ref iBullet, _getStartTag("<ul>"), "</ul>"));
						}
					}
					// --- process numbers
					else if (lineTrimmed[0] == '#')
					{
						// if we were doing an bullet list and this isn't one, we need to close the previous list down
						if ((iBullet > 0) && lineTrimmed[0] != '*')
							htmlMarkup.Append(_closeList(ref iBullet, "</ul>"));

						// generate correct indentation for bullets given current state of iNumber
						htmlMarkup.Append(_processListIndentations(lineTrimmed, '#', ref iNumber, _getStartTag("<ol>"), "</ol>"));
					}
					// --- process Headers
					else if (!InTable && lineTrimmed[0] == '=')
					{
						// close any pending lists
						_closeLists(ref htmlMarkup, ref iBullet, ref iNumber, lineTrimmed);

						// process = as headers only on start of lines
						htmlMarkup.Append(_processCreoleFragment(_processHeadersCreole(line)));
					}
					// --- start of table with header row
					else if (!InTable && lineTrimmed.StartsWith("|="))
					{
						// close any pending lists
						_closeLists(ref htmlMarkup, ref iBullet, ref iNumber, lineTrimmed);

						// start a new table
						htmlMarkup.Append(_getStartTag("<table class=\"wikitable\">"));
						InTable = true;
						htmlMarkup.Append(_processTableHeaderRow(lineTrimmed));
					}
					// --- start of table - standard row
					else if (!InTable && lineTrimmed[0] == '|')
					{
						// close any pending lists
						_closeLists(ref htmlMarkup, ref iBullet, ref iNumber, lineTrimmed);

						// start a new table
						htmlMarkup.Append(_getStartTag("<table class=\"wikitable\">"));
						InTable = true;
						htmlMarkup.Append(_processTableRow(lineTrimmed));
					}
					// --- new header row in table
					else if (InTable && lineTrimmed.StartsWith("|="))
					{
						// we are already processing table so this must be a new header row
						htmlMarkup.Append(_processTableHeaderRow(lineTrimmed));
					}
					// --- new standard row in table
					else if (InTable && lineTrimmed[0] == '|')
					{
						// we are already processing table so this must be a new row
						htmlMarkup.Append(_processTableRow(lineTrimmed));
					}
					else if (lineTrimmed.Contains(TextPlugin.PARSER_IGNORE_STARTTOKEN))
					{
						inRoadkillEscape = true;
					}
					// --- process {{{ }}} <pre>
					else if (lineTrimmed.StartsWith(NoWikiEscapeStart) && (lineTrimmed.Length == NoWikiEscapeStart.Length))
					{
						// we are already processing table so this must be a new row
						htmlMarkup.Append(_getStartTag("<pre>"));
						InEscape = true;
					}
					else
					{
						// we didn't find a special "start of line" command, 
						// namely ordered list, unordered list or table definition

						// just add it, processing any markup on it.
						htmlMarkup.Append(String.Format("{0}\n", _processCreoleFragment(line)));
					}
				}
				else
				{
					if (lineTrimmed.Contains(TextPlugin.PARSER_IGNORE_ENDTOKEN))
					{
						inRoadkillEscape = false;
					}
					else if (lineTrimmed.StartsWith(NoWikiEscapeEnd))
					{
						// we are looking for a line which starts with }}} to close off the preformated
						htmlMarkup.Append("</pre>\n");
						InEscape = false;
					}
					else
					{
						if (!inRoadkillEscape)
						{
							htmlMarkup.Append(System.Web.HttpUtility.HtmlEncode(line) + "\n"); // just pass it straight through unparsed
						}
						else
						{
							htmlMarkup.Append(line + "\n"); // no html encoding
						}
					}
				}
				idParagraph++;
			}
			// close out paragraph
			htmlMarkup.Append("</p>");

			// lastly, we want to expand tabs out into hard spaces so that the creole tabs are preserved 
			// NOTE: this is non-standard CREOLE...
			htmlMarkup = htmlMarkup.Replace("\t", this._tabStop);

			// return the HTML we have generated 
			return htmlMarkup.ToString();
		}


		private List<string> _breakMarkupIntoLines(string markup, out List<int> originalLineNumbers)
		{
			originalLineNumbers = new List<int>();
			List<string> lines = new List<string>();
			char[] chars = { '\n' };
			// break the creole into lines so we can process each line  
			string[] tempLines = markup.Split(chars);
			bool InEscape = false; // we are in a preformated escape
			// all markup works on a per line basis EXCEPT for the contiuation of lines with simple CR, so we simply merge those in, which makes a 
			// much easier processing story later on
			for (int iLine = 0; iLine < tempLines.Length; iLine++)
			{
				string line = tempLines[iLine];
				int i = iLine + 1;

				if ((line.Length > 0) && (line != "\r") && line[0] != '=')
				{
					if (!InEscape)
					{
						if (line.StartsWith(NoWikiEscapeStart))
						{
							InEscape = true;
						}
						else
						{
							// merge all lines which don't start with a command line together
							while (true)
							{
								if (i == tempLines.Length)
								{
									iLine = i - 1;
									break;
								}

								string trimmedLine = tempLines[i].Trim();
								if ((trimmedLine.Length == 0) ||
									trimmedLine[0] == '\r' ||
									trimmedLine[0] == '#' ||
									trimmedLine[0] == '*' ||
									trimmedLine.StartsWith(TextPlugin.PARSER_IGNORE_STARTTOKEN) ||
									trimmedLine.StartsWith(NoWikiEscapeStart) ||
									trimmedLine[0] == '=' ||
									trimmedLine.StartsWith("----") ||
									trimmedLine[0] == '|')
								{
									iLine = i - 1;
									break;
								}
								line += " " + trimmedLine; // erg, does CR == whitespace?
								i++;
							}
						}
					}
					else
					{
						if (line.StartsWith(NoWikiEscapeEnd))
							InEscape = false;
					}
				}
				// add the merged line to our list
				originalLineNumbers.Add(iLine);
				lines.Add(line);
			}
			originalLineNumbers.Add(lines.Count - 1);
			return lines;
		}
		protected string _getStartTag(string tag)
		{
			if (HTMLAttributes.ContainsKey(tag))
				return HTMLAttributes[tag];
			return tag;
		}

		private void _closeLists(ref StringBuilder htmlMarkup, ref int iBullet, ref int iNumber, string lineTrimmed)
		{
			if (lineTrimmed.Length > 0)
			{
				// if we were doing an ordered list and this isn't one, we need to close the previous list down
				if ((iNumber > 0) && lineTrimmed[0] != '#')
					htmlMarkup.Append(_closeList(ref iNumber, "</ol>"));

				// if we were doing an bullet list and this isn't one, we need to close the previous list down
				if ((iBullet > 0) && lineTrimmed[0] != '*')
					htmlMarkup.Append(_closeList(ref iBullet, "</ul>"));
			}
			else
			{
				// if we were doing an ordered list and this isn't one, we need to close the previous list down
				if (iNumber > 0)
					htmlMarkup.Append(_closeList(ref iNumber, "</ol>"));

				// if we were doing an bullet list and this isn't one, we need to close the previous list down
				if (iBullet > 0)
					htmlMarkup.Append(_closeList(ref iBullet, "</ul>"));
			}
		}

		private string _processTableRow(string line)
		{
			string markup = _getStartTag("<tr>");
			int iPos = _indexOfWithSkip(line, "|", 0);
			while (iPos >= 0)
			{
				iPos += 1;
				int iEnd = _indexOfWithSkip(line, "|", iPos);
				if (iEnd >= iPos)
				{
					string cell = _processCreoleFragment(line.Substring(iPos, iEnd - iPos)).Trim();
					if (cell.Length == 0)
						cell = "&nbsp;"; // table won't render if there isn't at least something...
					markup += String.Format("{0}{1}</td>", _getStartTag("<td>"), cell);
					iPos = iEnd;
				}
				else
					break;
			}
			markup += "</tr>";
			return markup;
		}

		/// <summary>
		/// passed a table definition line which starts with "|=" it outputs the start of a table definition
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private string _processTableHeaderRow(string line)
		{
			string markup = "";
			// add header
			markup += String.Format("{0}\n{1}\n", _getStartTag("<thead>"), _getStartTag("<tr>"));

			// process each |= cell section
			int iPos = _indexOfWithSkip(line, "|=", 0);
			while (iPos >= 0)
			{
				int iEnd = _indexOfWithSkip(line, "|=", iPos + 1);
				string cell = "";
				if (iEnd > iPos)
				{
					iPos += 2;
					cell = line.Substring(iPos, iEnd - iPos);
					iPos = iEnd;
				}
				else
				{
					iPos += 2;
					if (line.Length - iPos > 0)
						cell = line.Substring(iPos).TrimEnd(new char[] { '|' });
					else
						cell = "";
					iPos = -1;
				}
				if (cell.Length == 0)
					// forces the table cell to always be rendered
					cell = "&nbsp;";

				// create cell entry
				markup += String.Format("{0}{1}</th>", _getStartTag("<th>"), _processCreoleFragment(cell));
			}
			// close up row and header
			markup += "</tr>\n</thead>\n";
			return markup;
		}

		private string _processListIndentations(string line, char indentMarker, ref int iCurrentIndent, string indentTag, string outdentTag)
		{
			string markup = "";
			int iNewIndent = 0;
			for (int i = 0; i < line.Length; i++)
			{
				if (line[i] == indentMarker)
					iNewIndent++;
				else
					break;
			}
			// strip off counters
			line = line.Substring(iNewIndent);

			// close down bullets if we have fewer *s
			while (iNewIndent < iCurrentIndent)
			{
				markup += String.Format("{0}\n", outdentTag);
				iCurrentIndent--;
			}

			// add bullets if we have more *s
			while (iNewIndent > iCurrentIndent)
			{
				markup += String.Format("{0}\n", indentTag);
				iCurrentIndent++;
			}
			// mark the line in the list, processing the inner fragment for any additional markup
			markup += String.Format("{0}{1}</li>\n", _getStartTag("<li>"), _processCreoleFragment(line));
			return markup;
		}

		/// <summary>
		/// Given the current indentation level, close out the list
		/// </summary>
		/// <param name="iNumber"></param>
		private string _closeList(ref int iIndent, string closeHTML)
		{
			string html = "";
			while (iIndent > 0)
			{
				html += string.Format("{0}\n", closeHTML);
				iIndent--;
			}
			return html;
		}

		private string _processFreeLink(string schema, string markup)
		{
			int iPos = _indexOfWithSkip(markup, schema, 0);
			while (iPos >= 0)
			{
				string href = "";
				int iEnd = _indexOfWithSkip(markup, " ", iPos);
				if (iEnd > iPos)
				{
					href = markup.Substring(iPos, iEnd - iPos);
					string anchor = String.Format("<a target=\"_blank\" href=\"{0}\">{0}</a>", href);
					markup = markup.Substring(0, iPos) + anchor + markup.Substring(iEnd);
					iPos = iPos + anchor.Length;
				}
				else
				{
					href = markup.Substring(iPos);
					markup = markup.Substring(0, iPos) + String.Format("<a target=\"_blank\" href=\"{0}\">{0}</a>", href);
					break;
				}
				iPos = _indexOfWithSkip(markup, schema, iPos);
			}
			return markup;
		}

		/// <summary>
		/// Process http:, https: ftp: links automatically into a hrefs
		/// </summary>
		/// <param name="markup"></param>
		/// <returns></returns>
		private string _processFreeLinks(string markup)
		{
			markup = _processFreeLink("ftp:", markup);
			markup = _processFreeLink("http:", markup);
			return _processFreeLink("https:", markup);
		}

		private string _stripTildeEscapeCreole(string markup)
		{
			int iPos = markup.IndexOf('~');
			while ((iPos >= 0) && (iPos < markup.Length - 2))
			{
				string token = markup.Substring(iPos, 2);
				if (token == "~~")
				{
					markup = markup.Substring(0, iPos) + "~" + markup.Substring(iPos + 2);
					iPos++;
				}
				else
				{
					// if a non-whitespace char follows, we want to strip it
					if (token.Trim().Length != 1)
					{
						markup = markup.Remove(iPos, 1);
					}
				}
				iPos = markup.IndexOf('~', iPos + 1);
			}
			return markup;
		}

		/// <summary>
		/// Process a fragment of markup
		/// </summary>
		/// <param name="markup">fragment</param>
		/// <returns></returns>
		protected string _processCreoleFragment(string fragment)
		{
			fragment = _processBoldCreole(fragment);
			fragment = _processItalicCreole(fragment);
			fragment = _processUnderlineCreole(fragment);
			fragment = _processSuperscriptCreole(fragment);
			fragment = _processSubscriptCreole(fragment);
			fragment = _processStrikethroughCreole(fragment);
			fragment = _processLineBreakCreole(fragment);
			fragment = _processHorzRuleCreole(fragment);
			fragment = _processFreeLinks(fragment);
			fragment = _processImageCreole(fragment);
			fragment = _processLinkCreole(fragment);
			fragment = _stripEscapeCreole(fragment);
			fragment = _stripTildeEscapeCreole(fragment);
			return fragment;
		}

		/// <summary>
		/// Helper function to get the index of a match but skipping the content of 
		/// bracketed tags which can have creole inside it.  
		/// 
		/// It is used just like string.IndexOf()
		/// 
		/// Using this allows creole inside of links and images.  It's not clear from the spec what is expected
		/// but it seems desirable to allow this stuff to be nested to the degree we can do it.
		/// [[Link|**bold**]]
		/// [[link|{{foo.jpg|test}}]]
		/// </summary>
		/// <param name="markup">creole</param>
		/// <param name="match">token</param>
		/// <param name="iPos">starting position</param>
		/// <returns>index of token</returns>
		protected virtual int _indexOfWithSkip(string markup, string match, int iPos)
		{
			bool fSkipLink = (match != "[[") && (match != "]]");
			bool fSkipEscape = (match != NoWikiEscapeStart) && (match != NoWikiEscapeEnd);
			bool fSkipImage = (match != "{{") && (match != "}}");

			int tokenLength = match.Length;
			if (tokenLength < 3)
				tokenLength = NoWikiEscapeStart.Length; // so we can match on {{{
			for (int i = 0; i <= markup.Length - match.Length; i++)
			{
				if ((markup.Length - i) < tokenLength)
					tokenLength = markup.Length - i;
				string token = markup.Substring(i, tokenLength);
				if (fSkipEscape && token.StartsWith(NoWikiEscapeStart))
				{
					// skip escape
					int iEnd = markup.IndexOf(NoWikiEscapeEnd, i);
					if (iEnd > 0)
					{
						i = iEnd + 2; // plus for loop ++
						continue;
					}
				}
				if (fSkipLink && token.StartsWith("[["))
				{
					// skip link
					int iEnd = markup.IndexOf("]]", i);
					if (iEnd > 0)
					{
						i = iEnd + 1; // plus for loop ++
						continue;
					}
				}
				if (fSkipImage && token.StartsWith("{{"))
				{
					// skip image
					int iEnd = markup.IndexOf("}}", i);
					if (iEnd > 0)
					{
						i = iEnd + 1; // plus for loop ++
						continue;
					}
				}
				if (token.StartsWith(match))
				{
					// make sure previous char is not a ~, for this we have to go back 2 chars as double ~ is an escaped escape char
					if (i > 2)
					{
						string tildeCheck = markup.Substring(i - 2, 2);
						if ((tildeCheck != "~~") && (tildeCheck[1] == '~'))
							continue; // then we don't want to match this...it's been escaped
					}

					// only if it starts past our starting point
					if (i >= iPos)
						return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Processes bracketing creole markup into HTML
		/// **foo** into <b>foo</b> etc..
		/// </summary>
		/// <param name="match">bracketing token Ex: "=="</param>
		/// <param name="tag">tag to replace it with Ex: "h1"</param>
		/// <param name="markup">creole markup</param>
		/// <returns>markup with bracketing tokens replaces with HTML</returns>
		protected string _processBracketingCreole(string match, string startTag, string endTag, string markup)
		{
			// look for a start 
			int iPos = _indexOfWithSkip(markup, match, 0);
			while (iPos >= 0)
			{
				// special case for italics and urls, if match = // we don't match if previous char is a url
				if ((match == "//") &&
					(((iPos >= 6) && (markup.Substring(iPos - 6, 6).ToLower() == "https:")) ||
					 ((iPos >= 5) && (markup.Substring(iPos - 5, 5).ToLower() == "http:")) ||
					 ((iPos >= 4) && (markup.Substring(iPos - 4, 4).ToLower() == "ftp:"))))
				{
					// skip it, it's a url (I think)
					iPos = _indexOfWithSkip(markup, match, iPos + 1); ;
					continue;
				}

				int iEnd = _indexOfWithSkip(markup, match, iPos + match.Length);
				if (iEnd > 0)
				{
					string markedText = markup.Substring(iPos + match.Length, iEnd - (iPos + match.Length));
					if (markedText.Length > 0)
					{
						// add previous + start tag + markedText + end Tag + end
						markup = markup.Substring(0, iPos)
							+ startTag
							+ markedText
							+ endTag
							+ markup.Substring(iEnd + match.Length);
					}
					iPos = _indexOfWithSkip(markup, match, iEnd + 1);
				}
				else
				{
					string markedText = markup.Substring(iPos + match.Length);
					// treat end of line as end of bracketing
					markup = markup.Substring(0, iPos)
						+ startTag
						+ markedText
						+ endTag;
					break;
				}
			}
			return markup;
		}

		/// <summary>
		/// Process Headers Creole into HTML
		/// </summary>
		/// <param name="markup"></param>
		/// <returns></returns>
		private string _processHeadersCreole(string markup)
		{
			markup = _processBracketingCreole("======", _getStartTag("<h6>"), "</h6>", markup);
			markup = _processBracketingCreole("=====", _getStartTag("<h5>"), "</h5>", markup);
			markup = _processBracketingCreole("====", _getStartTag("<h4>"), "</h4>", markup);
			markup = _processBracketingCreole("===", _getStartTag("<h3>"), "</h3>", markup);
			markup = _processBracketingCreole("==", _getStartTag("<h2>"), "</h2>", markup);
			markup = _processBracketingCreole("=", _getStartTag("<h1>"), "</h1>", markup);
			return markup;
		}

		protected virtual string _processBoldCreole(string markup)
		{
			return _processBracketingCreole("**", _getStartTag("<strong>"), "</strong>", markup);
		}

		protected virtual string _processItalicCreole(string markup)
		{
			return _processBracketingCreole("//", _getStartTag("<em>"), "</em>", markup);
		}

		protected virtual string _processUnderlineCreole(string markup)
		{
			return _processBracketingCreole("__", _getStartTag("<u>"), "</u>", markup);
		}

		protected virtual string _processSuperscriptCreole(string markup)
		{
			return _processBracketingCreole("^^", _getStartTag("<sup>"), "</sup>", markup);
		}

		protected virtual string _processSubscriptCreole(string markup)
		{
			return _processBracketingCreole(",,", _getStartTag("<sub>"), "</sub>", markup);
		}

		protected virtual string _processStrikethroughCreole(string markup)
		{
			return _processBracketingCreole("--", _getStartTag("<del>"), "</del>", markup);
		}

		/// <summary>
		/// Processes link markup into HTML
		/// </summary>
		/// <param name="markup">markup</param>
		/// <returns>markup with [[foo]] translated into <a href></a></returns>
		protected virtual string _processLinkCreole(string markup)
		{
			int iPos = _indexOfWithSkip(markup, "[[", 0);
			while (iPos >= 0)
			{
				int iEnd = _indexOfWithSkip(markup, "]]", iPos);
				if (iEnd > iPos)
				{
					iPos += 2;
					// get the contents of the cell
					string cell = markup.Substring(iPos, iEnd - iPos);
					string link = cell;
					string href = cell; //default to assuming it's the href
					string text = href; // as well as the text
					int iSplit = cell.IndexOf('|'); // unless of course there is a splitter
					if (iSplit > 0)
					{
						// href is the front
						href = cell.Substring(0, iSplit);
						link = href;

						// text is the creole processed fragment left over
						text = _processCreoleFragment(cell.Substring(iSplit + 1));
					}

					// handle interwiki links
					iSplit = href.IndexOf(':');
					if (iSplit > 0)
					{
						string scheme = href.Substring(0, iSplit);
						if (InterWiki.ContainsKey(scheme))
						{
							href = InterWiki[scheme] + href.Substring(iSplit + 1);
						}
					}

					// Remove any attribute enders: ", <, >, &, '
					href = href.Replace("&", "&#x26;");
					href = href.Replace("\"", "&#x32;");
					href = href.Replace("<", "&#x3C;");
					href = href.Replace(">", "&#x3E;");				
					href = href.Replace("'", "&#x27;");

					// Use the MarkupConverter to parse the link
					LinkEventArgs linkEventArgs = new LinkEventArgs(link, href, text, "");
					OnLinkParsed(linkEventArgs);

					string nofollow = "";
					if (linkEventArgs.IsInternalLink == false)
						nofollow = "rel=\"nofollow\" ";

					string target = "";
					if (!string.IsNullOrWhiteSpace(linkEventArgs.Target))
						target= " target=\"" + linkEventArgs.Target + "\"";

					string cssClass = "";
					if (!string.IsNullOrWhiteSpace(linkEventArgs.CssClass))
						cssClass = " class=\"" + linkEventArgs.CssClass + "\"";

					string anchorHtml = string.Format("<a {0}href=\"{1}\"{2}{3}>{4}</a>",
														nofollow,
														linkEventArgs.Href,
														target,
														cssClass,
														linkEventArgs.Text);

					markup = markup.Substring(0, iPos - 2) + anchorHtml	+ markup.Substring(iEnd + 2);
				}
				else
					break;
				iPos = _indexOfWithSkip(markup, "[[", iPos);
			}
			return markup;
		}

		/// <summary>
		/// Process line break creole into <br/>
		/// </summary>
		/// <param name="markup"></param>
		/// <returns></returns>
		private string _processLineBreakCreole(string markup)
		{
			int iPos = _indexOfWithSkip(markup, "\\\\", 0);
			while (iPos >= 0)
			{
				markup = markup.Substring(0, iPos) + _getStartTag("<br/>") + markup.Substring(iPos + 2);
				iPos = _indexOfWithSkip(markup, "\\\\", iPos);
			}
			return markup;
		}

		/// <summary>
		/// Process horz rulle creole into <hr/>
		/// </summary>
		/// <param name="markup"></param>
		/// <returns></returns>
		private string _processHorzRuleCreole(string markup)
		{
			if (markup.StartsWith("----"))
				return _getStartTag("<hr/>") + markup.Substring(4);
			return markup;
		}

		/// <summary>
		/// Process image creole into <img> </img>
		/// </summary>
		/// <param name="markup"></param>
		/// <returns></returns>
		protected virtual string _processImageCreole(string markup)
		{
			int sanityCheckMax = 5000;
			int sanityCheckCount = 0;

			int iPos = _indexOfWithSkip(markup, "{{", 0);
			while (iPos >= 0)
			{
				if (++sanityCheckCount > sanityCheckMax)
					break;

				int iEnd = _indexOfWithSkip(markup, "}}", iPos);
				if (iEnd > iPos)
				{
					iPos += 2;
					string innards = markup.Substring(iPos, iEnd - iPos);
					string href = innards;
					string text = href;
					ImageEventArgs.HorizontalAlignment align = ImageEventArgs.HorizontalAlignment.None;
					string[] splits = innards.Split('|');

					if (innards.Count(x => x == '|') > 1)
					{
						if (Enum.TryParse(splits[1], true, out align) == false)
							align = ImageEventArgs.HorizontalAlignment.None;
					}

					if (splits.Count() > 0)
					{
						href = splits.First();
						text = _processCreoleFragment(splits.Last());
					}

					ImageEventArgs args = new ImageEventArgs(href, href, text, text, align);
					OnImageParsed(args);

					string imageHtml = String.Format("<img src=\"{0}\" alt=\"{1}\" title=\"{2}\" border=\"0\" />", args.Src, args.Alt, args.Title);
					string divHtml = "<div class=\"{0}\">{1}</div>";
					string imageFrameContent = imageHtml;

					if (!args.Src.EndsWith(args.Alt))
						imageFrameContent += String.Format(divHtml, "caption", args.Alt);

					string imageFrame = String.Format(divHtml, "image_frame", imageFrameContent);

					string replacement = markup.Substring(0, iPos - 2);
					switch (align)
					{
						case ImageEventArgs.HorizontalAlignment.Left:
							replacement += String.Format(divHtml, "floatleft", imageFrame);
							break;
						case ImageEventArgs.HorizontalAlignment.Right:
							replacement += String.Format(divHtml, "floatright", imageFrame);
							break;
						case ImageEventArgs.HorizontalAlignment.Center:
							replacement += String.Format(divHtml, "center", String.Format(divHtml, "floatnone", imageFrame));
							break;
						default:
							replacement += String.Format(divHtml, "floatnone", imageFrame);
							break;
					}

					replacement += markup.Substring(iEnd + 2);

					markup = replacement;
				}
				else
				{
					break;
				}

				iPos = _indexOfWithSkip(markup, "{{", iPos);
			}
			return markup;
		}

		/// <summary>
		/// Remove escape creole (no longer needed after all other transforms are done
		/// </summary>
		/// <param name="markup"></param>
		/// <returns></returns>
		private string _stripEscapeCreole(string markup)
		{
			int iPos = markup.IndexOf(NoWikiEscapeStart);
			while (iPos >= 0)
			{
				int iEnd = markup.IndexOf(NoWikiEscapeEnd, iPos);
				if (iEnd > iPos)
				{
					markup = markup.Substring(0, iPos) +
						markup.Substring(iPos + NoWikiEscapeStart.Length, iEnd - (iPos + NoWikiEscapeStart.Length)) +
						markup.Substring(iEnd + 3);

					iPos = markup.IndexOf(NoWikiEscapeStart, iPos);
				}
				else
					break;
			}
			return markup;
		}
		#endregion

		/// <summary>
		/// Raises the <see cref="ImageParsed"/> event.
		/// </summary>
		/// <param name="e">The event data. </param>
		protected void OnImageParsed(ImageEventArgs e)
		{
			if (ImageParsed != null)
				ImageParsed(this, e);
		}

		/// <summary>
		/// Raises the <see cref="LinkParsed"/> event.
		/// </summary>
		/// <param name="e">The event data. </param>
		protected void OnLinkParsed(LinkEventArgs e)
		{
			if (LinkParsed != null)
				LinkParsed(this, e);
		}
	}
}