using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Converters;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class TocParserTests
	{
		private ConfigurationContainerStub _config;
		private TocParser _tocParser;

		[SetUp]
		public void Setup()
		{
			_tocParser = new TocParser();
		}

		[Test]
		[Ignore]
		public void Basic_H1_Should_Render_Stuff()
		{
			// Arrange
			string htmlFragment = "{TOC}<h1>Item One<h1><p>Some text about item 1</p>";
			string expected = @"<div class=""toc"">
<div class=""toc-title"">Contents [<a class=""toc-showhide"" href=""#"">hide</a>]</div>
<div class=""toc-list"">
<ul>
<li>
<a href=""#Item-OneSome-text-about-item-1xScHyu2OYECemhhcxnyrgg"">1&nbsp;Item OneSome text about item 1</a></li>
</ul>
</div>
</div>
<h1><a name=""Item-OneSome-text-about-item-1xScHyu2OYECemhhcxnyrgg""></a>Item One<h1><p>Some text about item 1</p></h1></h1>";

			// Act
			string actual = _tocParser.InsertToc(htmlFragment);

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}
