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
		public void Should_Contain_Empty_List_When_Tokens_File_Not_Found()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.CustomTokensPath = Path.Combine(Settings.WEB_PATH, "doesntexist.xml");
			CustomTokenParser parser = new CustomTokenParser(settings);

			string expectedHtml = "@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@";

			// Act
			string actualHtml = parser.ReplaceTokensAfterParse("@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Should_Contain_Empty_List_When_When_Deserializing_Bad_Xml_File()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.CustomTokensPath = Path.Combine(Settings.ROOT_FOLDER, "readme.md"); // use a markdown file
			string expectedHtml = "@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@";

			// Act
			CustomTokenParser parser = new CustomTokenParser(settings);
			string actualHtml = parser.ReplaceTokensAfterParse("@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void WarningBox_Token_Should_Return_Html_Fragment()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.CustomTokensPath = Path.Combine(Settings.WEB_PATH, "App_Data", "customvariables.xml");
			CustomTokenParser parser = new CustomTokenParser(settings);

			string expectedHtml = @"<div class=""alert alert-warning"">ENTER YOUR CONTENT HERE {{some link}}</div><br style=""clear:both""/>";

			// Act
			string actualHtml = parser.ReplaceTokensAfterParse("@@warningbox:ENTER YOUR CONTENT HERE {{some link}}@@");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
