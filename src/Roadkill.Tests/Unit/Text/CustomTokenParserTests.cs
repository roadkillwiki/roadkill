using System;
using System.IO;
using NUnit;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;

namespace Roadkill.Tests.Unit.Text
{
	[TestFixture]
	[Category("Unit")]
	public class CustomTokenParserTests
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			CustomTokenParser.CacheTokensFile = false;
		}

		[Test]
		public void should_contain_empty_list_when_tokens_file_not_found()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.CustomTokensPath = Path.Combine(TestConstants.WEB_PATH, "doesntexist.xml");
			CustomTokenParser parser = new CustomTokenParser(settings);

			string expectedHtml = "@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@";

			// Act
			string actualHtml = parser.ReplaceTokensAfterParse("@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_contain_empty_list_when_when_deserializing_bad_xml_file()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.CustomTokensPath = Path.Combine(TestConstants.ROOT_FOLDER, "readme.md"); // use a markdown file
			string expectedHtml = "@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@";

			// Act
			CustomTokenParser parser = new CustomTokenParser(settings);
			string actualHtml = parser.ReplaceTokensAfterParse("@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void warningbox_token_should_return_html_fragment()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.CustomTokensPath = Path.Combine(TestConstants.WEB_PATH, "App_Data", "customvariables.xml");
			CustomTokenParser parser = new CustomTokenParser(settings);

			string expectedHtml = @"<div class=""alert alert-warning"">ENTER YOUR CONTENT HERE {{some link}}</div><br style=""clear:both""/>";

			// Act
			string actualHtml = parser.ReplaceTokensAfterParse("@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
