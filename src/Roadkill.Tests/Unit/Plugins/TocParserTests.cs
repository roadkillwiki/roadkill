using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Converters;
using Roadkill.Core.Text;
using Roadkill.Core.Plugins.Text.BuiltIn.ToC;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class TocParserTests
	{
		private string GetHtml()
		{
			string html = "{TOC} <p>some text</p>";
			html += "<h1>First h1</h1>";
			html += "	<h2>First h2</h2>";
			html += "		<h3>First h3</h3>";
			html += "		<div><p>sometext</p></div>";
			html += "	<h2>Second h2</h2>";
			html += "		<h3>Second h3</h3>";
			html += "		<h3>Third h3</h3>";
			html += "			<h4>Lonely h4</h4>";
			html += "<h1>Second h1</h1>";

			return html;
		}

		private string GetLotsOfHeaders()
		{
			string html = "{TOC} <p>some text</p>";
			html += "<h1>First h1</h1>";
			html += "	<h2>First h2</h2>";
			html += "		<h3>First h3</h3>";
			html += "		<div><p>sometext</p></div>";
			html += "	<h2>Second h2</h2>";

			for (int i = 0; i < 50; i++)
			{
				html += "		<h3>h3 number #" +i+ "</h3>";
			}

			html += "		<h3>Another h3</h3>";
			html += "		<h3>Yet Another h3</h3>";
			html += "			<h4>Lonely h4</h4>";
			html += "<h1>Second h1</h1>";

			return html;
		}

		[Test]
		public void Should_Have_Correct_Tree_Structure_From_Basic_Html()
		{
			// Arrange
			TocParser tocParser = new TocParser();
			string html = GetHtml();

			// Act
			tocParser.InsertToc(html);

			// Assert
			Item root = tocParser.Tree.Root;
			Assert.That(root, Is.Not.Null);
			Assert.That(root.Level, Is.EqualTo(Tree.DEFAULT_LEVEL_ZERO_BASED));

			List<Item> allH2s = root.Children.ToList();
			Assert.That(allH2s.Count, Is.EqualTo(2));

			// <h2>First h2</h2>
			Item firstH2 = allH2s[0];
			Assert.That(firstH2.Children.Count(), Is.EqualTo(1));
			Item firstH3 = firstH2.GetChild(0);
			Assert.That(firstH3.Children.Count(), Is.EqualTo(0));

			// <h2>Second h2</h2>
			Item secondH2 = allH2s[1];
			Assert.That(secondH2.Children.Count(), Is.EqualTo(2));
			
			Item secondH3 = secondH2.GetChild(0);
			Assert.That(secondH3.Children.Count(), Is.EqualTo(0));
			
			Item thirdH3 = secondH2.GetChild(1);
			Assert.That(thirdH3.Children.Count(), Is.EqualTo(1));

			Item firstH4 = thirdH3.GetChild(0);
			Assert.That(firstH4.Children.Count(), Is.EqualTo(0));			
		}

		[Test]
		public void Should_Have_Correct_Titles()
		{
			// Arrange
			TocParser tocParser = new TocParser();
			string html = GetHtml();

			// Act
			tocParser.InsertToc(html);

			// Assert
			Item root = tocParser.Tree.Root;
			List<Item> allH2s = root.Children.ToList();

			Item firstH2 = allH2s[0];
			Item firstH3 = firstH2.GetChild(0);
			Item secondH2 = allH2s[1];
			Item thirdH3 = secondH2.GetChild(1);
			Item firstH4 = thirdH3.GetChild(0);

			Assert.That(firstH2.Title, Is.EqualTo("First h2"));
			Assert.That(firstH3.Title, Is.EqualTo("First h3"));
			Assert.That(secondH2.Title, Is.EqualTo("Second h2"));
			Assert.That(thirdH3.Title, Is.EqualTo("Third h3"));
			Assert.That(firstH4.Title, Is.EqualTo("Lonely h4"));
		}

		[Test]
		public void Should_Have_Named_Anchors_Inserted_By_Headers_In_Html()
		{
			// Arrange
			TocParser tocParser = new TocParser();
			string html = GetHtml();

			// Act
			string actual = tocParser.InsertToc(html);

			// Assert
			StringAssert.IsMatch(@"<h2><a name=\"".*?""></a>First h2</h2>", actual);
		}

		[Test]
		public void Should_Have_Correct_Section_Numbering_For_Large_Lists()
		{
			// Arrnage
			TocParser tocParser = new TocParser();
			string html = GetLotsOfHeaders();

			// Act
			string actual = tocParser.InsertToc(html);

			// Assert
			// (really basic asserts, as the alternative is to just copy the HTML)
			Assert.That(actual, Is.Not.StringContaining("1&nbsp;First h1"));
			Assert.That(actual, Is.StringContaining("1.&nbsp;First h2"));
			Assert.That(actual, Is.StringContaining("2.&nbsp;Second h2"));
			Assert.That(actual, Is.StringContaining("2.1&nbsp;h3 number #0"));
			Assert.That(actual, Is.StringContaining("2.47&nbsp;h3 number #46"));
			Assert.That(actual, Is.StringContaining("2.52&nbsp;Yet Another h3"));
			Assert.That(actual, Is.StringContaining("2.52.1&nbsp;Lonely h4"));
		}

		[Test]
		public void Should_Ignore_Multiple_Curlies()
		{
			// Arrange
			TocParser tocParser = new TocParser();
			string html = "Give me a {{TOC}} and a {{{{TOC}}}} - the should not render a TOC";
			string expected = html;

			// Act
			string actual = tocParser.InsertToc(html);

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void Should_Have_Correct_Html_Nesting_And_Warning_Titles_When_Missing_Levels()
		{
			// From issue #177
			// Arrange
			TocParser tocParser = new TocParser();
			string html = "{TOC} <p>some text</p>";
			html += "<h1>h1</h1>"; // deliberately removed
			//html += "	<h2>h2</h2>"; // deliberately removed
			html += "		<h3>h3a</h3>";
			html += "			<h4>h4a</h4>";
			html += "			<h4>h4b</h4>";
			html += "		<h3>h3b</h3>";
			html += "			<h4>h4c</h4>";
			html += "				<h5>h5a</h5>";
			html += "				<h5>h5b</h5>";
			html += "		<h3>h3c</h3>";
			html += "			<h4>h4d</h4>";
			html += "	<h2>h2b</h2>";
			//html += "		<h3>h3</h3>"; // deliberately removed
			//html += "			<h4>h4</h4>"; // deliberately removed
			html += "				<h5>h5c</h5>";

			// Act
			string actual = tocParser.InsertToc(html);

			// Assert
			Assert.That(actual, Is.StringContaining("(Missing level 2 header)"));
			Assert.That(actual, Is.StringContaining("(Missing level 3 header)"));
			Assert.That(actual, Is.StringContaining("(Missing level 4 header)"));


			Assert.That(actual, Is.StringContaining("1.3&nbsp;h3c"));
			Assert.That(actual, Is.StringContaining("1.3.1&nbsp;h4d"));
		}
	}
}
