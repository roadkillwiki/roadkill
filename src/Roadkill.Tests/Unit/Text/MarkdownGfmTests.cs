using System.Collections.Generic;
using NUnit.Framework;
using Roadkill.Core.Converters;

namespace Roadkill.Tests.Unit.Text
{
	/// <summary>
	/// Tests for the Markdig based Markdown parser: GitHub Flavored Markdown, Mermaid, and the Roadkill 2.x (MarkdownSharp)
	/// syntax that is still supported.
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class MarkdownGfmTests
	{
		private MarkdownParser _parser;
		private List<LinkEventArgs> _links;
		private List<ImageEventArgs> _images;

		[SetUp]
		public void Setup()
		{
			_links = new List<LinkEventArgs>();
			_images = new List<ImageEventArgs>();

			_parser = new MarkdownParser();
			_parser.LinkParsed += (sender, e) =>
			{
				_links.Add(e);
				if (!e.OriginalHref.StartsWith("http"))
				{
					e.IsInternalLink = true;
					e.Href = "/wiki/1/" + e.OriginalHref;
				}
				else
				{
					e.CssClass = "external-link";
				}
			};
			_parser.ImageParsed += (sender, e) =>
			{
				_images.Add(e);
				e.Src = "/Attachments/" + e.OriginalSrc;
			};
		}

		[Test]
		public void pipe_tables_should_be_rendered_with_alignment_and_wikitable_class()
		{
			// Arrange
			string markdown = "| Name | Qty |\n|:-----|----:|\n| **Apple** | 3 |";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(html, Does.Contain("<table class=\"wikitable\">"));
			Assert.That(html, Does.Contain("<th style=\"text-align: left;\">Name</th>"));
			Assert.That(html, Does.Contain("<th style=\"text-align: right;\">Qty</th>"));
			Assert.That(html, Does.Contain("<td style=\"text-align: left;\"><strong>Apple</strong></td>"));
		}

		[Test]
		public void links_and_images_inside_tables_should_raise_events()
		{
			// Arrange
			string markdown = "| Link | Image |\n|---|---|\n| [Page](My-page) | ![alt](a.png) |";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(_links.Count, Is.EqualTo(1));
			Assert.That(_images.Count, Is.EqualTo(1));
			Assert.That(html, Does.Contain("<a href=\"/wiki/1/My-page\">Page</a>"));
			Assert.That(html, Does.Contain("<img src=\"/Attachments/a.png\""));
		}

		[Test]
		public void strikethrough_tasklists_and_autolinks_should_be_rendered()
		{
			// Arrange
			string markdown = "~~old~~\n\n- [x] done\n- [ ] todo\n\nSee www.example.com";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(html, Does.Contain("<del>old</del>"));
			Assert.That(html, Does.Contain("<input disabled=\"disabled\" type=\"checkbox\" checked=\"checked\" /> done"));
			Assert.That(html, Does.Contain("<input disabled=\"disabled\" type=\"checkbox\" /> todo"));
			Assert.That(html, Does.Contain("<a href=\"http://www.example.com\" rel=\"nofollow\" class=\"external-link\">www.example.com</a>"));
		}

		[Test]
		public void fenced_code_blocks_should_have_language_class_and_encoded_html()
		{
			// Arrange
			string markdown = "```sql\nselect * from x where a < 5\n```";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(html, Is.EqualTo("<pre><code class=\"language-sql\">select * from x where a &lt; 5\n</code></pre>\n"));
		}

		[Test]
		public void mermaid_code_blocks_should_be_output_for_the_mermaid_javascript()
		{
			// Arrange
			string markdown = "```mermaid\ngraph TD\n    A --> B\n```";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(html, Does.StartWith("<pre class=\"mermaid\">graph TD\n    A --> B"));
		}

		[Test]
		[TestCase("#Heading 1#", "<h1>Heading 1</h1>")]
		[TestCase("##Heading 2##", "<h2>Heading 2</h2>")]
		[TestCase("###Heading 3", "<h3>Heading 3</h3>")]
		[TestCase("# Standard heading", "<h1>Standard heading</h1>")]
		[TestCase("# Using C#", "<h1>Using C#</h1>")]
		[TestCase("> #Quoted", "<blockquote>\n<h1>Quoted</h1>\n</blockquote>")]
		public void headings_without_spaces_should_still_be_headings(string markdown, string expectedHtml)
		{
			// Arrange + Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(html.Trim(), Is.EqualTo(expectedHtml));
		}

		[Test]
		public void hashes_in_code_blocks_and_mid_line_should_not_be_headings()
		{
			// Arrange
			string markdown = "text #not heading\n\n```\n#region code\n```\n\n    #indented code";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(html, Does.Not.Contain("<h1>"));
			Assert.That(html, Does.Contain("<p>text #not heading</p>"));
			Assert.That(html, Does.Contain("#region code"));
			Assert.That(html, Does.Contain("#indented code"));
		}

		[Test]
		[TestCase("![Square](a.png =250x)", " width=\"250px\"", "")]
		[TestCase("![Rectangle](a.png =250x350)", " width=\"250px\"", " height=\"350px\"")]
		[TestCase("![Tall](a.png \"title\" =x350)", "", " height=\"350px\"")]
		public void images_should_support_roadkill_dimensions(string markdown, string expectedWidth, string expectedHeight)
		{
			// Arrange + Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(_images.Count, Is.EqualTo(1));
			Assert.That(html, Does.Contain("class=\"img-responsive\""));
			Assert.That(html, Does.Contain(expectedWidth + expectedHeight));
		}

		[Test]
		public void image_dimensions_syntax_should_not_be_parsed_in_code()
		{
			// Arrange
			string markdown = "`![Square](a.png =250x)`";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(_images.Count, Is.EqualTo(0));
			Assert.That(html, Does.Contain("<code>![Square](a.png =250x)</code>"));
		}

		[Test]
		public void external_links_should_have_nofollow_and_internal_links_should_not()
		{
			// Arrange
			string markdown = "[internal](My-page) [external](https://example.com \"title\")";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(html, Is.EqualTo("<p><a href=\"/wiki/1/My-page\">internal</a> <a href=\"https://example.com\" rel=\"nofollow\" title=\"title\" class=\"external-link\">external</a></p>\n"));
		}

		[Test]
		public void links_with_spaces_should_not_be_links()
		{
			// Arrange (as with Roadkill 2.x, "-" is used for spaces in internal links)
			string markdown = "[Link](My first page)";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(_links.Count, Is.EqualTo(0));
			Assert.That(html, Is.EqualTo("<p>[Link](My first page)</p>\n"));
		}

		[Test]
		public void plugin_tokens_should_be_left_for_the_plugins()
		{
			// Arrange
			string markdown = "{TOC}\n\n(warningbox:hello)\n\n[[[code lang=sql|select 1]]]";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(html, Does.Contain("{TOC}"));
			Assert.That(html, Does.Contain("(warningbox:hello)"));
			Assert.That(html, Does.Contain("[[[code lang=sql|select 1]]]"));
		}

		[Test]
		public void raw_html_should_be_kept()
		{
			// Arrange (it's removed by the HTML whitelist sanitizer afterwards, if enabled)
			string markdown = "<sup>sup</sup>\n\n<pre>not **bold**</pre>";

			// Act
			string html = _parser.Transform(markdown);

			// Assert
			Assert.That(html, Does.Contain("<sup>sup</sup>"));
			Assert.That(html, Does.Contain("not **bold**"));
		}
	}
}
