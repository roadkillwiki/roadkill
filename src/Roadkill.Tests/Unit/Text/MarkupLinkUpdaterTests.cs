using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;

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
		public void ContainsPageLink_Should_Return_True_When_Title_Exists_In_Creole()
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
		public void ContainsPageLink_Should_Return_True_When_Title_Exists_In_Markdown()
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
		public void ContainsPageLink_Should_Return_False_When_Title_Has_No_Dashes_In_Markdown()
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
		public void ContainsPageLink_Should_Return_False_When_Title_Does_Not_Exist_In_Creole()
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
		public void ContainsPageLink_Should_Return_False_When_Title_Does_Not_Exist_In_Markdown()
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
	}
}
