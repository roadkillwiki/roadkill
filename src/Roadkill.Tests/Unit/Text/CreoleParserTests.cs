using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	//http://www.wikicreole.org/attach/Creole1.0TestCases/creole1.0test.txt
	//http://www.wikicreole.org/wiki/JSPWikiTestCases
	public class CreoleParserTests
	{
		private ConfigurationContainerStub _config;
		private CreoleParser _parser;

		[SetUp]
		public void Setup()
		{
			_config = new ConfigurationContainerStub();
			_parser = new CreoleParser(_config);
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
			string expectedHtml = "<p>Here are all the tags for animals: <a href=\"/pages/tag/animal\">Animals</a>\n</p>";

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
			string expectedHtml = "<table><tr><td>Cell 1.1</td><td>Cell 1.2</td></tr>";

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
	}
}
