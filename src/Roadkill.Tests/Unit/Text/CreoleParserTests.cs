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
	public class CreoleParserTests
	{
		[Test]
		public void Tilde_Should_Escape_Text()
		{
			// Some very simplistic tests based off
			// http://www.wikicreole.org/wiki/EscapeCharacterProposa

			// Arrange
			CreoleParser parser = new CreoleParser(new ConfigurationContainerStub());
			string creoleText = @"This isn't a ~-list item.";
			string expectedHtml = "<p>This isn't a -list item.\n</p>";
			string creoleText2 = @"No bold ~**this isn't bold~** test";
			string expectedHtml2 = "<p>No bold **this isn't bold** test\n</p>";

			// Act		
			string actualHtml = parser.Transform(creoleText);
			string actualHtml2 = parser.Transform(creoleText2);

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
			CreoleParser parser = new CreoleParser(new ConfigurationContainerStub());
			string actualHtml = parser.Transform(creoleText);
			
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
			CreoleParser parser = new CreoleParser(new ConfigurationContainerStub());
			string actualHtml = parser.Transform(creoleText);

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
			CreoleParser parser = new CreoleParser(new ConfigurationContainerStub());
			string actualHtml = parser.Transform(creoleText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
