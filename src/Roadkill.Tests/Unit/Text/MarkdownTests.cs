using System;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Text
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
		public void internal_links_should_resolve_with_id()
		{
			// Bug #87

			// Arrange
			Page page = new Page() { Id = 1, Title = "My first page" };

			var settingsRepository = new SettingsRepositoryMock();
			settingsRepository.SiteSettings = new SiteSettings() { MarkupType = "Markdown" };

			PageRepositoryMock pageRepositoryStub = new PageRepositoryMock();
			pageRepositoryStub.AddNewPage(page, "My first page", "admin", DateTime.UtcNow);

			ApplicationSettings settings = new ApplicationSettings();
			settings.Installed = true;

			UrlResolverMock resolver = new UrlResolverMock();
			resolver.InternalUrl = "blah";
			MarkupConverter converter = new MarkupConverter(settings, settingsRepository, pageRepositoryStub, _pluginFactory);
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
		public void code_blocks_should_allow_quotes()
		{
			// Issue #82
			// Arrange
			Page page = new Page() { Id = 1, Title = "My first page" };

			PageRepositoryMock pageRepositoryStub = new PageRepositoryMock();
			pageRepositoryStub.AddNewPage(page, "My first page", "admin", DateTime.UtcNow);

			var settingsRepository = new SettingsRepositoryMock();
			settingsRepository.SiteSettings = new SiteSettings() { MarkupType = "Markdown" };

			ApplicationSettings settings = new ApplicationSettings();
			settings.Installed = true;

			MarkupConverter converter = new MarkupConverter(settings, settingsRepository, pageRepositoryStub, _pluginFactory);

			string markdownText = "Here is some `// code with a 'quote' in it and another \"quote\"`\n\n" +
				"    var x = \"some tabbed code\";\n\n"; // 2 line breaks followed by 4 spaces (tab stop) at the start indicates a code block

			string expectedHtml = "<p>Here is some <code>// code with a 'quote' in it and another \"quote\"</code></p>\n\n" +
								"<pre><code>var x = \"some tabbed code\";\n" +
								"</code></pre>\n";

			// Act		
			string actualHtml = converter.ToHtml(markdownText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void images_should_support_dimensions()
		{
			// Arrange
			Page page = new Page() { Id = 1, Title = "My first page" };

			PageRepositoryMock pageRepositoryStub = new PageRepositoryMock();
			pageRepositoryStub.AddNewPage(page, "My first page", "admin", DateTime.UtcNow);

			var settingsRepository = new SettingsRepositoryMock();
			settingsRepository.SiteSettings = new SiteSettings() { MarkupType = "Markdown" };

			ApplicationSettings settings = new ApplicationSettings();
			settings.Installed = true;

			MarkupConverter converter = new MarkupConverter(settings, settingsRepository, pageRepositoryStub, _pluginFactory);

			string markdownText = "Here is an image:![Image](/Image1.png) \n\n" +
								  "And another with equal dimensions ![Square](/Image1.png =250x) \n\n" +
								  "And this one is a rectangle ![Rectangle](/Image1.png =250x350)";

			string expectedHtml = "<p>Here is an image:<img src=\"/Attachments/Image1.png\" class=\"img-responsive\" border=\"0\" alt=\"Image\" width=\"\" height=\"\" /> </p>\n\n" +
									"<p>And another with equal dimensions <img src=\"/Attachments/Image1.png\" class=\"img-responsive\" border=\"0\" alt=\"Square\" width=\"250px\" height=\"\" /> </p>\n\n" +
									"<p>And this one is a rectangle <img src=\"/Attachments/Image1.png\" class=\"img-responsive\" border=\"0\" alt=\"Rectangle\" width=\"250px\" height=\"350px\" /></p>\n";


			// Act		
			string actualHtml = converter.ToHtml(markdownText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void images_should_support_dimensions_and_titles()
		{
			// Arrange
			Page page = new Page() { Id = 1, Title = "My first page" };

			PageRepositoryMock pageRepositoryStub = new PageRepositoryMock();
			pageRepositoryStub.AddNewPage(page, "My first page", "admin", DateTime.UtcNow);

			var settingsRepository = new SettingsRepositoryMock();
			settingsRepository.SiteSettings = new SiteSettings() { MarkupType = "Markdown" };

			ApplicationSettings settings = new ApplicationSettings();
			settings.Installed = true;

			MarkupConverter converter = new MarkupConverter(settings, settingsRepository, pageRepositoryStub, _pluginFactory);

			string markdownText = "Here is an image with a title:![Image](/Image1.png \"Image\") \n\n" +
								  "And another with equal dimensions ![Square](/Image1.png \"Square\" =250x) \n\n" +
								  "And this one is a rectangle ![Rectangle](/Image1.png \"Rectangle\" =250x350)";

			string expectedHtml = "<p>Here is an image with a title:<img src=\"/Attachments/Image1.png\" class=\"img-responsive\" border=\"0\" alt=\"Image\" width=\"\" height=\"\" title=\"Image\" /> </p>\n\n" +
									"<p>And another with equal dimensions <img src=\"/Attachments/Image1.png\" class=\"img-responsive\" border=\"0\" alt=\"Square\" width=\"250px\" height=\"\" title=\"Square\" /> </p>\n\n" +
									"<p>And this one is a rectangle <img src=\"/Attachments/Image1.png\" class=\"img-responsive\" border=\"0\" alt=\"Rectangle\" width=\"250px\" height=\"350px\" title=\"Rectangle\" /></p>\n";


			// Act		
			string actualHtml = converter.ToHtml(markdownText);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}