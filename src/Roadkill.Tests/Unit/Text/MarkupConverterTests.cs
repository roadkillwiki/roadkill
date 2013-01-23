using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Converters;

namespace Roadkill.Tests.Unit
{
	public class MarkupConverterTests
	{
		[Test]
		public void Links_Starting_With_Https_Or_Hash_Are_Ignored()
		{
			// Arrange
			ConfigurationContainerStub config = new ConfigurationContainerStub();
			config.SitePreferences.MarkupType = "Creole";

			string expectedHtml = "<p><a href=\"#myanchortag\">hello world</a> <a href=\"https://www.google.com\">google</a>\n</p>";
			MarkupConverter converter = new MarkupConverter(config, null);

			// Act
			string actualHtml = converter.ToHtml("[[#myanchortag|hello world]] [[https://www.google.com|google]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Links_Starting_With_Http_Www_Mailto_Tag_Are_Ignored()
		{
			// Arrange
			ConfigurationContainerStub config = new ConfigurationContainerStub();
			config.SitePreferences.MarkupType = "Creole";

			string expectedHtml = "<p><a href=\"http://www.blah.com\">link1</a> <a href=\"www.blah.com\">link2</a> <a href=\"mailto:spam@gmail.com\">spam</a>\n</p>";
			MarkupConverter converter = new MarkupConverter(config, null);

			// Act
			string actualHtml = converter.ToHtml("[[http://www.blah.com|link1]] [[www.blah.com|link2]] [[mailto:spam@gmail.com|spam]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
