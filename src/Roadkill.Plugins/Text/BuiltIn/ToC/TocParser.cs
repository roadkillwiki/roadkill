using System;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;

namespace Roadkill.Plugins.Text.BuiltIn.ToC
{
	/// <summary>
	/// Parses a HTML document for H (e.g. H1,H2) elements and produces a table of contents.
	/// Anchor tags (&lt;a name="") are then inserted into the HTML document.
	/// </summary>
	public class TocParser
	{
		private Tree _tree;
		private readonly StringTemplate _template;
		private static readonly Regex _regex = new Regex(@"(?<!{)(?:\{TOC\})(?!})", RegexOptions.Compiled);

		internal Tree Tree
		{
			get { return _tree; }
		}

		public TocParser()
		{
			_template = new StringTemplate()
			{
				ItemStart = "<li>",
				ItemFormat = @"<a href=""#{id}"">{levels}{itemnumber}&nbsp;{title}</a>",
				ItemEnd = "</li>",
				LevelStart = "<ul>",
				LevelEnd = "</ul>"
			};
		}

		/// <summary>
		/// Replaces all {TOC} tokens with the HTML for the table of contents. This method also inserts
		/// anchored name tags before each H1,H2,H3 etc. tag that the contents references.
		/// </summary>
		public string InsertToc(string html)
		{
			if (!html.Contains("{TOC}"))
				return html;

			_tree = new Tree(_template);

			// Parse the HTML for H tags
			var parser = new HtmlParser();
			IHtmlDocument document = parser.Parse(html);
			ParseHTagsAndAddAnchors(document, document.QuerySelector("body"));

			string outputHtml = GenerateHtml();
			string innerHtml = document.QuerySelector("body").InnerHtml;

			// make sure {{TOC}} or {{{{TOC}}}} aren't matched
			innerHtml = _regex.Replace(innerHtml, outputHtml);
			return document.DocumentElement.InnerHtml = innerHtml;
		}

		private string GenerateHtml()
		{
			string treeHtml = _tree.CreateHtmlForItems();

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<div class=\"toc\">");
			builder.AppendLine("<div class=\"toc-title\">Contents [<a class=\"toc-showhide\" href=\"javascript:;\">hide</a>]</div>");
			builder.AppendLine("<div class=\"toc-list\">");
			builder.AppendLine("<ul>");
			builder.AppendLine(treeHtml);
			builder.AppendLine("</ul>");
			builder.AppendLine("</div>");
			builder.AppendLine("</div>");

			return builder.ToString();
		}

		private void ParseHTagsAndAddAnchors(IHtmlDocument document, IElement parentElement)
		{
			foreach (IElement element in parentElement.Children)
			{
				string tagName = element.NodeName.ToLower();
				string title = element.TextContent;

				if (tagName.StartsWith("h") && tagName.Length == 2)
				{
					// Use the H number (e.g. 2 for H2) as the current level in the tree
					int level = 0;
					int.TryParse(tagName.ToLower().Replace("h", ""), out level);

					// Level sanity check for bad markup
					if (level > 1)
					{
						Item item = _tree.AddItemAtLevel(level, title);

						// Insert an achor tag after the header as a reference
						IElement anchor = document.CreateElement("a");
						anchor.SetAttribute("name", item.Id);

						element.InnerHtml = anchor.OuterHtml + element.InnerHtml;
					}
				}
				else if (element.HasChildNodes)
				{
					ParseHTagsAndAddAnchors(document, element);
				}
			}
		}
	}
}
