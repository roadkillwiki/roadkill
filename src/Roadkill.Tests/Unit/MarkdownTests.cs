using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Tests.Unit.Controllers;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class MarkdownParserTests
	{
		[Test]
		public void Internal_Links_Should_Resolve_With_Id()
		{
			// Bug #87

			// Arrange
			Page page = new Page() { Id = 1, Title = "My first page"};

			Mock<IRepository> mockRepository = new Mock<IRepository>();
			mockRepository.Setup(x => x.FindPageByTitle(page.Title)).Returns<string>(p => { return page; });

			IConfigurationContainer config = new RoadkillSettings();
			config.SitePreferences = new SitePreferences() { MarkupType = "Markdown" };
			MarkupConverter converter = new MarkupConverter(config, mockRepository.Object);
			converter.InternalUrlForTitle = (id, title) => { return "blah"; };

			
			string markdownText = "[Link](My-first-page)";
			string invalidMarkdownText = "[Link](My first page)";

			// Act
			string expectedHtml = "<p><a href=\"blah\">Link</a></p>\n";
			string expectedInvalidLinkHtml = "<p>[Link](My first page)</p>\n";

			string actualHtml = converter.ToHtml(markdownText);
			string actualHtmlInvalidLink = converter.ToHtml(invalidMarkdownText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
			Assert.That(actualHtmlInvalidLink, Is.EqualTo(expectedInvalidLinkHtml));
		}
	}
}
