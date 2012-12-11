using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace Roadkill.Core
{
	/// <summary>
	/// Parses a HTML document for Hx (e.g. H1,H2) elements and produces a table of contents.
	/// Anchor tags (&lt;a name="") are then inserted into the HTML document.
	/// </summary>
	public class TocParser
	{
		private Header _previousHeader;

		/// <summary>
		/// Replaces all {TOC} tokens with the HTML for the table of contents. This method also inserts
		/// anchored name tags before each H1,H2,H3 etc. tag that the contents references.
		/// </summary>
		public string InsertToc(string html)
		{
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(html);
			HtmlNodeCollection elements = document.DocumentNode.ChildNodes;

			// Parse the HTML document for all Hx elements
			List<Header> rootHeaders = new List<Header>();
			ParseHtmlAddAnchors(document.DocumentNode, rootHeaders, "h1");

			// If no H1 headers are found (as H1 is technically the page title) try parsing all H2 headers
			if (rootHeaders.Count == 0)
				ParseHtmlAddAnchors(document.DocumentNode, rootHeaders, "h2");

			// Add a fake root for the tree
			Header rootHeader = new Header("","h0");
			rootHeader.Children.AddRange(rootHeaders);
			foreach (Header header in rootHeaders)
			{
				header.Parent = rootHeader;
			}

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<div class=\"toc\">");
			builder.AppendLine("<div class=\"toc-title\">Contents [<a class=\"toc-showhide\" href=\"#\">hide</a>]</div>");
			builder.AppendLine("<div class=\"toc-list\">");
			builder.AppendLine("<ul>");
			GenerateTocList(rootHeader, builder);
			builder.AppendLine("</ul>");
			builder.AppendLine("</div>");
			builder.AppendLine("</div>");

			return document.DocumentNode.InnerHtml.Replace("{TOC}",builder.ToString());
		}

		/// <summary>
		/// Generates the ToC contents HTML for the using the StringBuilder.
		/// </summary>
		private void GenerateTocList(Header parentHeader, StringBuilder htmlBuilder)
		{
			// Performs a level order traversal of the H1 (or H2) trees
			foreach (Header header in parentHeader.Children)
			{
				htmlBuilder.AppendLine("<li>");
				htmlBuilder.AppendFormat(@"<a href=""#{0}"">{1}&nbsp;{2}</a>", header.Id, header.GetTocNumber(), header.Title);
				
				if (header.Children.Count > 0)
				{
					htmlBuilder.AppendLine("<ul>");
					GenerateTocList(header, htmlBuilder);
					htmlBuilder.AppendLine("</ul>");
				}

				htmlBuilder.AppendLine("</li>");
			}
		}	

		/// <summary>
		/// Parses the HTML for H1,H2, H3 etc. elements, and adds them as Header trees, where
		/// rootHeaders contains the H1 root nodes.
		/// </summary>
		private void ParseHtmlAddAnchors(HtmlNode parentNode, List<Header> rootHeaders, string rootTag)
		{
			foreach (HtmlNode node in parentNode.ChildNodes)
			{
				if (node.Name.StartsWith("h"))
				{
					Header header = new Header(node.InnerText,node.Name);

					if (_previousHeader != null && header.Level > _previousHeader.Level)
					{
						// Add as a new child
						header.Parent = _previousHeader;
						_previousHeader.Children.Add(header);
					}
					else if (_previousHeader != null)
					{
						// Add as a sibling
						while (_previousHeader.Parent != null && _previousHeader.Level > header.Level)
						{
							_previousHeader = _previousHeader.Parent;
						}

						header.Parent = _previousHeader.Parent;

						if (header.Parent != null)
							header.Parent.Children.Add(header);
					}

					// Add an achor tag after the header as a reference
					HtmlNode anchor = HtmlNode.CreateNode(string.Format(@"<a name=""{0}""></a>",header.Id));
					node.PrependChild(anchor);

					if (node.Name == rootTag)
						rootHeaders.Add(header);

					_previousHeader = header;
				}
				else if (node.HasChildNodes)
				{
					ParseHtmlAddAnchors(node, rootHeaders, rootTag);
				}
			}
		}

		/// <summary>
		/// Represents a header and its child headers, a tree with many branches.
		/// </summary>
		private class Header
		{
			public string Id { get; set; }
			public string Tag { get; set; }
			public int Level { get; private set; }
			public List<Header> Children { get; set; }
			public Header Parent { get; set; }
			public string Title { get; set; }

			public Header(string title, string tag)
			{
				Children = new List<Header>();
				Title = title;
				Tag = tag;

				int level = 0;
				int.TryParse(tag.Replace("h", ""),out level); // lazy (aka hacky) way of tracking the level using HTML H number
				Level = level;

				ShortGuid guid = ShortGuid.NewGuid();
				Id = string.Format("{0}{1}", Title.EncodeTitle(), guid);
			}

			public string GetTocNumber()
			{
				string result = SiblingNumber().ToString();

				if (Parent != null && Level > 1)
				{
					Header parent = Parent;

					while (parent != null && parent.Level > 0)
					{
						result += "." + parent.SiblingNumber();
						parent = parent.Parent;
					}
				}

				return new String(result.ToArray().Reverse().ToArray<char>());
			}

			public int SiblingNumber()
			{
				if (Parent != null)
				{
					for (int i = 0; i < Parent.Children.Count; i++)
					{
						if (Parent.Children[i] == this)
							return i +1;
					}
				}

				return 1;
			}

			public override bool Equals(object obj)
			{
				Header header = obj as Header;
				if (header == null)
					return false;

				return header.Id.Equals(Id);
			}

			public override int GetHashCode()
			{
				return Id.GetHashCode();
			}
		}
	}
}
