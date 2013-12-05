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
using Roadkill.Core.Database;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class MarkdownParserTests
	{
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Setup()
		{
			_pluginFactory = new PluginFactoryMock();		
		}

		[Test]
		public void Internal_Links_Should_Resolve_With_Id()
		{
			// Bug #87

			// Arrange
			Page page = new Page() { Id = 1, Title = "My first page"};

			RepositoryMock repositoryStub = new RepositoryMock();
			repositoryStub.AddNewPage(page, "My first page", "admin", DateTime.UtcNow);
			repositoryStub.SiteSettings = new SiteSettings() { MarkupType = "Markdown" };

			ApplicationSettings settings = new ApplicationSettings();
			settings.Installed = true;
			settings.UpgradeRequired = false;

			UrlResolverMock resolver = new UrlResolverMock();
			resolver.InternalUrl = "blah";
			MarkupConverter converter = new MarkupConverter(settings, repositoryStub, _pluginFactory);
			converter.UrlResolver = resolver;
			
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

		[Test]
		public void Code_Blocks_Should_Allow_Quotes()
		{
			// Issue #82
			// Arrange
			Page page = new Page() { Id = 1, Title = "My first page" };

			RepositoryMock repositoryStub = new RepositoryMock();
			repositoryStub.AddNewPage(page, "My first page", "admin", DateTime.UtcNow);
			repositoryStub.SiteSettings = new SiteSettings() { MarkupType = "Markdown" };

			ApplicationSettings settings = new ApplicationSettings();
			settings.Installed = true;
			settings.UpgradeRequired = false;

			MarkupConverter converter = new MarkupConverter(settings, repositoryStub, _pluginFactory);

			string markdownText = "Here is some `// code with a 'quote' in it and another \"quote\"`\n\n" +
				"    var x = \"some tabbed code\";\n\n"; // 2 line breaks followed by 4 spaces (tab stop) at the start indicates a code block

			string expectedHtml = "<p>Here is some <code>// code with a 'quote' in it and another \"quote\"</code></p>\n\n"+
								"<pre><code>var x = \"some tabbed code\";\n" +
								"</code></pre>\n";

			// Act		
			string actualHtml = converter.ToHtml(markdownText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}