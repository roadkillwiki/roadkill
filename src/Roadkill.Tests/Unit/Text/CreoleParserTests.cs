using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;

// Reference:
// http://www.wikicreole.org/attach/Creole1.0TestCases/creole1.0test.txt
// http://www.wikicreole.org/wiki/JSPWikiTestCases
namespace Roadkill.Tests.Unit
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
		public void Tilde_Should_Escape_Text()
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
		public void Tilde_Should_Be_Escapeable()
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
		public void Text_should_Appear_After_Images()
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
		public void Tag_Link_Should_Be_Replaced()
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
		public void Tables_Should_Be_Created_Without_Header()
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
		public void Should_Render_H1_Html_For_Header1_Markup()
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
		public void Should_Render_H2_Html_For_Header2_Markup()
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
		public void Should_Render_H3_Html_For_Header3_Markup()
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
		public void Should_Render_H4_Html_For_Header4_Markup()
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
		public void Should_Render_H5_Html_For_Header5_Markup()
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
		public void Should_Render_H1_Html_For_Header1_Markup_With_Ending_Equal_Sign()
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
		public void Should_Render_H2_Html_For_Header2_Markup_With_Ending_Equal_Sign()
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
		public void Should_Render_H3_Html_For_Header3_Markup_With_Ending_Equal_Sign()
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
		public void Should_Render_H4_Html_For_Header4_Markup_With_Ending_Equal_Sign()
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
		public void Should_Render_H5_Html_For_Header5_Markup_With_Ending_Equal_Sign()
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
		public void Should_Render_Bold_Italic_And_Combined()
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
		public void Should_Render_Bold_Across_Line_Breaks_But_Not_Paragraphs()
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
		public void Internal_And_External_Links_Should_Render_As_Anchor_Tags()
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
		public void Internal_Links_With_Quote_And_External_Link_Should_Render_As_Anchor_Tags()
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
		public void External_Link_With_No_Description_Should_Render_Plain_Link_In_Anchor_Tag()
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
		public void Links_Within_Italic_Markup_Should_Render_Em_Tag_And_Anchor_Tag()
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
		public void Free_Links_With_No_Square_Brackets_Should_Render_As_Anchor_Tags()
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
		public void Unknown_Protocols_Should_Render_As_Italic()
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
		public void Four_Dashes_Should_Draw_Hr_Tag()
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
		public void Template()
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
