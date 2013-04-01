using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Converters;
using Roadkill.Core.Text;
using Roadkill.Core.Text.ToC;

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

		private string GetBigHeaderList()
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
		public void Should_Have_Correct_Tree_Structure()
		{
			// Arrange
			TocParser tocParser = new TocParser();
			string html = GetHtml();

			// Act
			tocParser.InsertToc(html);

			// Assert
			Item root = tocParser.Tree.Root;
			Assert.That(root, Is.Not.Null);
			Assert.That(root.Level, Is.EqualTo(0));

			List<Item> allH1s = root.Children.ToList();
			Assert.That(allH1s.Count, Is.EqualTo(2));

			Item firstH1 = allH1s[0];
			Item firstH2 = firstH1.GetChild(0);
			Item secondH2 = firstH1.GetChild(1);	
			
			Assert.That(firstH2.Children.Count(), Is.EqualTo(1));
			Assert.That(secondH2.Children.Count(), Is.EqualTo(2));

			Item firstH3 = firstH2.GetChild(0);
			Item secondH3 = secondH2.GetChild(0);
			Item thirdH3 = secondH2.GetChild(1);

			Assert.That(firstH3.Children.Count(), Is.EqualTo(0));
			Assert.That(secondH2.Children.Count(), Is.EqualTo(2));

			Assert.That(thirdH3.Children.Count(), Is.EqualTo(1));

			Item secondH1 = allH1s[1];
			Assert.That(secondH1.Children.Count(), Is.EqualTo(0));
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
			List<Item> allH1s = root.Children.ToList();

			Item firstH1 = allH1s[0];
			Item firstH2 = firstH1.GetChild(0);
			Item firstH3 = firstH2.GetChild(0);
			Item thirdH3 = allH1s[0].GetChild(1).GetChild(1);
			Item H4 = thirdH3.GetChild(0);

			Assert.That(firstH1.Title, Is.EqualTo("First h1"));
			Assert.That(firstH2.Title, Is.EqualTo("First h2"));
			Assert.That(firstH3.Title, Is.EqualTo("First h3"));
			Assert.That(H4.Title, Is.EqualTo("Lonely h4"));
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
			TocParser tocParser = new TocParser();
			string html = GetBigHeaderList();

			// Act
			string actual = tocParser.InsertToc(html);

			// Assert
			// (really basic asserts, as the alternative is to just copy the HTML)
			Assert.That(actual, Is.StringContaining("1.2.52&nbsp;Yet Another h3"));
			Assert.That(actual, Is.StringContaining("1.2.52.1&nbsp;Lonely h4"));
		}

		[Test]
		public void Should_Ignore_Multiple_Curlies()
		{
			TocParser tocParser = new TocParser();
			string html = "Give me a {{TOC}} and a {{{{TOC}}}} - the should not render a TOC";
			string expected = html;

			// Act
			string actual = tocParser.InsertToc(html);

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}
