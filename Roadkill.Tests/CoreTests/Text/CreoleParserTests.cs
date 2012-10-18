using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Converters;

namespace Roadkill.Tests.CoreTests
{
	[TestFixture]
	public class CreoleParserTests
	{
		[Test]
		public void Tilde_Should_Escape_Text()
		{
			CreoleParser parser = new CreoleParser();

			// Some very simplistic tests based off
			// http://www.wikicreole.org/wiki/EscapeCharacterProposal
			string creoleText = @"This isn't a ~-list item.";
			string expectedHtml = "<p>This isn't a -list item.\n</p>";
			string actualHtml = parser.Transform(creoleText);
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));

			creoleText = @"No bold ~**this isn't bold~** test";
			expectedHtml = "<p>No bold **this isn't bold** test\n</p>";
			actualHtml = parser.Transform(creoleText);
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Tilde_Should_Be_Escapeable()
		{
			CreoleParser parser = new CreoleParser();

			string creoleText = @"Escaping a ~~tilde test";
			string expectedHtml = "<p>Escaping a ~tilde test\n</p>";
			string actualHtml = parser.Transform(creoleText);
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
