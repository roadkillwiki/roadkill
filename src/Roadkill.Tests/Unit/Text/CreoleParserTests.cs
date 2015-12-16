using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;

// Reference:
// http://www.wikicreole.org/attach/Creole1.0TestCases/creole1.0test.txt
// http://www.wikicreole.org/wiki/JSPWikiTestCases
namespace Roadkill.Tests.Unit.Text
{
	[TestFixture]
	[Category("Unit")]
	public class CreoleParserTests
	{
		private ApplicationSettings _applicationSettings;
		private SiteSettings _siteSettings;
		private CreoleParser _parser;

		[SetUp]
		public void Setup()
		{
			_applicationSettings = new ApplicationSettings();
			_siteSettings = new SiteSettings();
			_parser = new CreoleParser(_applicationSettings, _siteSettings);
		}

		[Test]
		public void tilde_should_escape_text()
		{
			// Some very simplistic tests based off
			// http://www.wikicreole.org/wiki/EscapeCharacterProposa

			// Arrange
			string creoleText = @"This isn't a ~-list item.";
			string expectedHtml = "<p>This isn't a -list item.\n</p>";
			string creoleText2 = @"No bold ~**this isn't bold~** test";
			string expectedHtml2 = "<p>No bold **this isn't bold** test\n</p>";

			// Act		
			string actualHtml = _parser.Transform(creoleText);
			string actualHtml2 = _parser.Transform(creoleText2);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
			Assert.That(actualHtml2, Is.EqualTo(expectedHtml2));
		}

		[Test]
		public void tilde_should_be_escapeable()
		{
			// Bug #80
			
			// Arrange
			string creoleText = @"Escaping a ~~tilde test";
			string expectedHtml = "<p>Escaping a ~tilde test\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);
			
			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void text_should_appear_after_images()
		{
			// Bug #85

			// Arrange
			string creoleText = "{{image01.jpg|alt text}} Any text here WILL BE shown on webpage";
			string expectedHtml = @"<p><div class=""floatnone""><div class=""image_frame""><img src=""image01.jpg"" alt=""alt text"" title=""alt text"" border=""0"" />"+
				@"<div class=""caption"">alt text</div></div></div> Any text here WILL BE shown on webpage" + "\n</p>";
			
			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void tag_link_should_be_replaced()
		{
			// Arrange
			string creoleText = "Here are all the tags for animals: [[tag:animal|Animals]]";
			string expectedHtml = "<p>Here are all the tags for animals: <a rel=\"nofollow\" href=\"/pages/tag/animal\">Animals</a>\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void tables_should_be_created_without_header()
		{
			// Issue #95

			// Arrange
			string creoleText = @"|Cell 1.1 | Cell 1.2 |";
			string expectedHtml = "<table class=\"wikitable\"><tr><td>Cell 1.1</td><td>Cell 1.2</td></tr>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Contains.Substring(expectedHtml));
		}

		[Test]
		public void should_render_h1_html_for_header1_markup()
		{
			// Arrange
			string creoleText = @"=Top-level heading (1)";
			string expectedHtml = "<p><h1>Top-level heading (1)</h1></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_h2_html_for_header2_markup()
		{
			// Arrange
			string creoleText = @"==This a test for creole 0.1 (2)";
			string expectedHtml = "<p><h2>This a test for creole 0.1 (2)</h2></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_h3_html_for_header3_markup()
		{
			// Arrange
			string creoleText = @"===This is a Subheading (3)";
			string expectedHtml = "<p><h3>This is a Subheading (3)</h3></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_h4_html_for_header4_markup()
		{
			// Arrange
			string creoleText = @"====Subsub (4)";
			string expectedHtml = "<p><h4>Subsub (4)</h4></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_h5_html_for_header5_markup()
		{
			// Arrange
			string creoleText = @"=====Subsubsub (5)";
			string expectedHtml = "<p><h5>Subsubsub (5)</h5></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_h1_html_for_header1_markup_with_ending_equal_sign()
		{
			// Arrange
			string creoleText = @"=Top-level heading (1)=";
			string expectedHtml = "<p><h1>Top-level heading (1)</h1></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_h2_html_for_header2_markup_with_ending_equal_sign()
		{
			// Arrange
			string creoleText = @"==This a test for creole 0.1 (2)==";
			string expectedHtml = "<p><h2>This a test for creole 0.1 (2)</h2></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_h3_html_for_header3_markup_with_ending_equal_sign()
		{
			// Arrange
			string creoleText = @"===This is a Subheading (3)===";
			string expectedHtml = "<p><h3>This is a Subheading (3)</h3></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_h4_html_for_header4_markup_with_ending_equal_sign()
		{
			// Arrange
			string creoleText = @"====Subsub (4)====";
			string expectedHtml = "<p><h4>Subsub (4)</h4></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_h5_html_for_header5_markup_with_ending_equal_sign()
		{
			// Arrange
			string creoleText = @"=====Subsubsub (5)=====";
			string expectedHtml = "<p><h5>Subsubsub (5)</h5></p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_bold_italic_and_combined()
		{
			// Arrange
			string creoleText = "You can make things **bold** or //italic// or **//both//** or //**both**//.";
			string expectedHtml = "<p>You can make things <strong>bold</strong> or <em>italic</em> or <strong><em>both</em></strong> or <em><strong>both</strong></em>.\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_render_bold_across_line_breaks_but_not_paragraphs()
		{
			// Arrange
			string creoleText = @"Character formatting extends across line breaks: **bold,
this is still bold. This line deliberately does not end in star-star.

Not bold. Character formatting does not cross paragraph boundaries.";

			// CreoleParser doesn't inject \n on the 1st line - is this right?
			string expectedHtml = "<p>Character formatting extends across line breaks: <strong>bold,\r "+
 "this is still bold. This line deliberately does not end in star-star.</strong>\n"+
"</p>\n"+
"<p>Not bold. Character formatting does not cross paragraph boundaries.\n"+
"</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void internal_and_external_links_should_render_as_anchor_tags()
		{
			// Arrange
			string creoleText = "You can use [[internal links]] or [[http://www.wikicreole.org|external links]], give the link a [[internal links|different]] name.";
			string expectedHtml = "<p>You can use <a rel=\"nofollow\" href=\"internal links\">internal links</a> or <a rel=\"nofollow\" href=\"http://www.wikicreole.org\">external links</a>, give the link a <a rel=\"nofollow\" href=\"internal links\">different</a> name.\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void internal_links_with_quote_and_external_link_should_render_as_anchor_tags()
		{
			// Arrange
			string creoleText = "Here's another sentence: This wisdom is taken from [[Ward Cunningham's]] [[http://www.c2.com/doc/wikisym/WikiSym2006.pdf|Presentation at the Wikisym 06]].";
			string expectedHtml = "<p>Here's another sentence: This wisdom is taken from <a rel=\"nofollow\" href=\"Ward Cunningham&#x27;s\">Ward Cunningham's</a> <a rel=\"nofollow\" href=\"http://www.c2.com/doc/wikisym/WikiSym2006.pdf\">Presentation at the Wikisym 06</a>.\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void external_link_with_no_description_should_render_plain_link_in_anchor_tag()
		{
			// Arrange
			string creoleText = "Here's a external link without a description: [[http://www.wikicreole.org]]";
			string expectedHtml = "<p>Here's a external link without a description: <a rel=\"nofollow\" href=\"http://www.wikicreole.org\">http://www.wikicreole.org</a>\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void links_within_italic_markup_should_render_em_tag_and_anchor_tag()
		{
			// Arrange
			string creoleText = "Be careful that italic links are rendered properly:  //[[http://my.book.example/|My Book Title]]//";
			string expectedHtml = "<p>Be careful that italic links are rendered properly:  <em><a rel=\"nofollow\" href=\"http://my.book.example/\">My Book Title</a></em>\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void free_links_with_no_square_brackets_should_render_as_anchor_tags()
		{
			// Arrange
			string creoleText = "Free links without braces should be rendered as well, like http://www.wikicreole.org/ and http://www.wikicreole.org/users/";
			string expectedHtml = "<p>Free links without braces should be rendered as well, like <a target=\"_blank\" href=\"http://www.wikicreole.org/\">http://www.wikicreole.org/</a> and <a target=\"_blank\" href=\"http://www.wikicreole.org/users/\">http://www.wikicreole.org/users/</a>\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		[Ignore("Currently failing")]
		public void Links_With_Tilde_Should_Not_Be_Escaped()
		{
			// Arrange
			string creoleText = "Links with tildes should not be escaped, http://www.wikicreole.org/users/~example.";
			string expectedHtml = "<p>Links with tildes should not be escaped, <a target=\"_blank\" href=\"http://www.wikicreole.org/users/~example\">http://www.wikicreole.org/users/~example</a>.</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void unknown_protocols_should_render_as_italic()
		{
			// Arrange
			string creoleText = "Creole1.0 specifies that http://bar and ftp://bar should not render italic, something like foo://bar should render as italic.";
			string expectedHtml = "<p>Creole1.0 specifies that <a target=\"_blank\" href=\"http://bar\">http://bar</a> and <a target=\"_blank\" href=\"ftp://bar\">ftp://bar</a> should not render italic, something like foo:<em>bar should render as italic.</em>\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void four_dashes_should_draw_hr_tag()
		{
			// Arrange
			string creoleText = @"You can use this to draw a line to separate the page:
----";
			string expectedHtml = "<p>You can use this to draw a line to separate the page:\n<hr/>\n</p>";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void template()
		{
			// Arrange
			string creoleText = "";
			string expectedHtml = "";

			// Act
			string actualHtml = _parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
