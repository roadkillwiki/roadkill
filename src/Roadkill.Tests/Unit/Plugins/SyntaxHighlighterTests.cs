using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins.SpecialPages.BuiltIn;
using Roadkill.Core.Plugins.Text.BuiltIn;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Plugins
{
	[TestFixture]
	[Category("Unit")]
	public class SyntaxHighlighterTests
	{
		private MocksAndStubsContainer _container;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
		}

		[Test]
		public void BeforeParse_Should_Replace_Token_With_Html_Pre_Tag_And_Surround_With_Ignore_Tokens()
		{
			// Arrange
			string expectedParsedMarkup = "here is some code that mimics our beautiful C#: \n"+
									SyntaxHighlighter.PARSER_IGNORE_STARTTOKEN + " \n" +
									"<pre class=\"brush: java\">\npublic static void main(String args)\n{\n/* do something */\n}\n</pre>\n" +
									SyntaxHighlighter.PARSER_IGNORE_ENDTOKEN +"\n";

			string markup = "here is some code that mimics our beautiful C#: [[[code lang=java|\npublic static void main(String args)\n{\n/* do something */\n}\n]]]";
			SyntaxHighlighter highlighter = new SyntaxHighlighter();

			// Act
			string actualMarkup = highlighter.BeforeParse(markup);

			// Assert
			Assert.That(actualMarkup, Is.EqualTo(expectedParsedMarkup), actualMarkup);
		}

		[Test]
		public void AfterParse_Should_Remove_Ignore_Tokens()
		{
			// Arrange
			string expectedHtml = "here is some code that mimics our beautiful C#: \n \n" +
									"<pre class=\"brush: java\">\npublic static void main(String args)\n{\n/* do something */\n}\n</pre>\n\n"; // extra \n for the tokens

			string markup = "here is some code that mimics our beautiful C#: [[[code lang=java|\npublic static void main(String args)\n{\n/* do something */\n}\n]]]";
			SyntaxHighlighter highlighter = new SyntaxHighlighter();

			// Act
			string html = highlighter.BeforeParse(markup);
			string parsedHtml = highlighter.AfterParse(html);

			// Assert
			Assert.That(parsedHtml, Is.EqualTo(expectedHtml), parsedHtml);
		}

		[Test]
		public void GetHeadContent_Should_Use_HeadJs_And_Load_ShCore_First()
		{
			// Arrange
			SyntaxHighlighter highlighter = new SyntaxHighlighter();

			// Act
			string headContent = highlighter.GetHeadContent();

			// Assert
			Assert.That(headContent, Is.StringContaining("head.js(\"javascript/shCore.js\""), headContent);
		}

		[Test]
		public void GetHeadContent_Should_Contain_SyntaxHighlighterAll_In_HeadJS_Load_Function()
		{
			// Arrange
			SyntaxHighlighter highlighter = new SyntaxHighlighter();

			// Act
			string headContent = highlighter.GetHeadContent();

			// Assert
			Assert.That(headContent, Is.StringContaining("function() { SyntaxHighlighter.all() }"), headContent);
		}
	}
}
