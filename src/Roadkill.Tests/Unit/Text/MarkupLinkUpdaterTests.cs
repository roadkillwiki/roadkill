using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Text
{
	[TestFixture]
	[Category("Unit")]
	public class MarkupLinkUpdaterTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private RepositoryMock _repository;
		private PluginFactoryMock _pluginFactory;
		private MarkupConverter _markupConverter;
		private SiteSettings _siteSettings;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_siteSettings = _container.SettingsService.GetSiteSettings();

			_pluginFactory = _container.PluginFactory;
			_repository = _container.Repository;
		}

		[Test]
		public void containspagelink_should_return_true_when_title_exists_in_creole()
		{
			// Arrange
			CreoleParser parser = new CreoleParser(_applicationSettings, _siteSettings);
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = "here is a nice [[the internal wiki page title|the link text]]";

			// Act
			bool hasLink = updater.ContainsPageLink(text, "the internal wiki page title");

			// Assert
			Assert.That(hasLink, Is.True);
		}

		[Test]
		public void containspagelink_should_return_true_when_title_exists_in_markdown()
		{
			// Arrange
			MarkdownParser parser = new MarkdownParser();
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = "here is a nice [the link text](the-internal-wiki-page-title)";

			// Act
			bool hasLink = updater.ContainsPageLink(text, "the internal wiki page title");

			// Assert
			Assert.That(hasLink, Is.True);
		}

		[Test]
		public void containspagelink_should_return_false_when_title_has_no_dashes_in_markdown()
		{
			// Arrange
			MarkdownParser parser = new MarkdownParser();
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = "here is a nice [the link text](Markdown enforces dashes for spaces in urls)";

			// Act
			bool hasLink = updater.ContainsPageLink(text, "Markdown enforces dashes for spaces in urls");

			// Assert
			Assert.That(hasLink, Is.False);
		}

		[Test]
		public void containspagelink_should_return_false_when_title_does_not_exist_in_creole()
		{
			// Arrange
			CreoleParser parser = new CreoleParser(_applicationSettings, _siteSettings);
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = "here is a nice [[the internal wiki page title|the link text]]";

			// Act
			bool hasLink = updater.ContainsPageLink(text, "page title");

			// Assert
			Assert.That(hasLink, Is.False);
		}

		[Test]
		public void containspagelink_should_return_false_when_title_does_not_exist_in_markdown()
		{
			// Arrange
			MarkdownParser parser = new MarkdownParser();
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = "here is a nice [the link text](the-internal-wiki-page-title)";

			// Act
			bool hasLink = updater.ContainsPageLink(text, "page title");

			// Assert
			Assert.That(hasLink, Is.False);
		}

		[Test]
		public void replacepagelinks_should_rename_basic_creole_title()
		{
			// Arrange
			CreoleParser parser = new CreoleParser(_applicationSettings, _siteSettings);
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = "here is a nice [[the internal wiki page title|the link text]]";
			string expectedMarkup = "here is a nice [[buy stuff online|the link text]]";

			// Act
			string actualMarkup = updater.ReplacePageLinks(text, "the internal wiki page title", "buy stuff online");

			// Assert
			Assert.That(actualMarkup, Is.EqualTo(expectedMarkup), actualMarkup);
		}

		[Test]
		public void replacepagelinks_should_rename_multiple_creole_titles()
		{
			// Arrange
			CreoleParser parser = new CreoleParser(_applicationSettings, _siteSettings);
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = @"here is a nice [[the internal wiki page title|the link text]] and 
                            another one: here is a nice [[the internal wiki page title|the link text]] and 
							a different one: here is a nice [[different title|the link text]]";

			string expectedMarkup = @"here is a nice [[buy stuff online|the link text]] and 
                            another one: here is a nice [[buy stuff online|the link text]] and 
							a different one: here is a nice [[different title|the link text]]";

			// Act
			string actualMarkup = updater.ReplacePageLinks(text, "the internal wiki page title", "buy stuff online");

			// Assert
			Assert.That(actualMarkup, Is.EqualTo(expectedMarkup), actualMarkup);
		}

		[Test]
		public void replacepagelinks_should_rename_title_inside_creole_markup_block()
		{
			// Arrange
			CreoleParser parser = new CreoleParser(_applicationSettings, _siteSettings);
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = @"//here is a nice **[[the internal wiki page title|the link text]]** and// 
                            another one: *here is a nice [[the internal wiki page title|the link text]] and 
							*a different one: here is a nice [[different title|the link text]]";

			string expectedMarkup = @"//here is a nice **[[buy stuff online|the link text]]** and// 
                            another one: *here is a nice [[buy stuff online|the link text]] and 
							*a different one: here is a nice [[different title|the link text]]";

			// Act
			string actualMarkup = updater.ReplacePageLinks(text, "the internal wiki page title", "buy stuff online");

			// Assert
			Assert.That(actualMarkup, Is.EqualTo(expectedMarkup), actualMarkup);
		}

		[Test]
		public void replacepagelinks_should_not_rename_title_that_is_not_found_in_creole()
		{
			// Arrange
			CreoleParser parser = new CreoleParser(_applicationSettings, _siteSettings);
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = @"here is a nice [[the internal wiki page title|the link text]] and 
                            another one: here is a nice [[the internal wiki page title|the link text]] and 
							a different one: here is a nice [[different title|the link text]]";

			// Act
			string actualMarkup = updater.ReplacePageLinks(text, "page title", "buy stuff online");

			// Assert
			Assert.That(actualMarkup, Is.EqualTo(text), actualMarkup);
		}
		
		// ReplacePageLinks:
		//	- x Should rename basic creole title
		//	- x Should rename multiple creole titles
		//  - x Should rename title inside creole markup block
		//	- Should not replace title that's not found
		//  (Repeat for markdown)

		[Test]
		public void replacepagelinks_should_rename_basic_markdown_title()
		{
			// Arrange
			MarkdownParser parser = new MarkdownParser();
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = "here is a nice [the link text](the-internal-wiki-page-title)";
			string expectedMarkup = "here is a nice [the link text](buy-stuff-online)";

			// Act
			string actualMarkup = updater.ReplacePageLinks(text, "the internal wiki page title", "buy stuff online");

			// Assert
			Assert.That(actualMarkup, Is.EqualTo(expectedMarkup), actualMarkup);
		}

		[Test]
		public void replacepagelinks_should_rename_multiple_markdown_titles()
		{
			// Arrange
			MarkdownParser parser = new MarkdownParser();
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = @"here is a nice [the link text](the-internal-wiki-page-title) and 
                            another one: here is a nice [the link text](the-internal-wiki-page-title) and 
							a different one: here is a nice [the link text](different-title)";

			string expectedMarkup = @"here is a nice [the link text](buy-stuff-online) and 
                            another one: here is a nice [the link text](buy-stuff-online) and 
							a different one: here is a nice [the link text](different-title)";

			// Act
			string actualMarkup = updater.ReplacePageLinks(text, "the internal wiki page title", "buy stuff online");

			// Assert
			Assert.That(actualMarkup, Is.EqualTo(expectedMarkup), actualMarkup);
		}

		[Test]
		public void replacepagelinks_should_rename_title_inside_markdown_block()
		{
			// Arrange
			MarkdownParser parser = new MarkdownParser();
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = @"here is a nice [the link text](the-internal-wiki-page-title) and 
                            another one: here is a nice [the link text](the-internal-wiki-page-title) and 
							a different one: here is a nice [the link text](different-title)";

			string expectedMarkup = @"here is a nice [the link text](buy-stuff-online) and 
                            another one: here is a nice [the link text](buy-stuff-online) and 
							a different one: here is a nice [the link text](different-title)";

			// Act
			string actualMarkup = updater.ReplacePageLinks(text, "the internal wiki page title", "buy stuff online");

			// Assert
			Assert.That(actualMarkup, Is.EqualTo(expectedMarkup), actualMarkup);
		}

		[Test]
		public void replacepagelinks_should_not_rename_title_that_is_not_found_in_markdown()
		{
			// Arrange
			MarkdownParser parser = new MarkdownParser();
			MarkupLinkUpdater updater = new MarkupLinkUpdater(parser);

			string text = @"*here* is a nice **[the link text](the-internal-wiki-page-title)** and 
                            another one: *here is a nice [the link text](the-internal-wiki-page-title) and 
							a different one: *here is a nice [the link text](different-title)";

			// Act
			string actualMarkup = updater.ReplacePageLinks(text, "page title", "buy stuff online");

			// Assert
			Assert.That(actualMarkup, Is.EqualTo(text), actualMarkup);
		}
	}
}
