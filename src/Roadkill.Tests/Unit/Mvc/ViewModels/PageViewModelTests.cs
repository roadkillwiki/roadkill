using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PageViewModelTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _appSettings;
		private MarkupConverter _markupConverter;
		private RepositoryMock _repository;
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_pluginFactory = _container.PluginFactory;
			_appSettings = _container.ApplicationSettings;
			_appSettings.Installed = true;
			_repository = _container.Repository;
			_markupConverter = _container.MarkupConverter;
			_markupConverter.UrlResolver = new UrlResolverMock();
		}

		[Test]
		public void Empty_Constructor_Should_Fill_Property_Defaults()
		{
			// Arrange + act
			PageViewModel model = new PageViewModel();

			// Assert
			Assert.That(model.IsCacheable, Is.True);
			Assert.That(model.PluginHeadHtml, Is.EqualTo(""));
			Assert.That(model.PluginFooterHtml, Is.EqualTo(""));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PageContent_Constructor_Should_Throw_Exception_When_PageContent_IsNull()
		{
			// Arrange + Act + Assert
			PageViewModel model = new PageViewModel(null, _markupConverter);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PageContent_Constructor_Should_Throw_Exception_When_PageContent_Page_IsNull()
		{
			// Arrange
			PageContent content = new PageContent();
			content.Page = null;

			// Act + Assert
			PageViewModel model = new PageViewModel(content, _markupConverter);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PageContent_Constructor_Should_Throw_Exception_When_MarkupConverter_IsNull()
		{
			// Arrange
			PageContent content = new PageContent();
			content.Page = new Page();

			// Act + Assert
			PageViewModel model = new PageViewModel(content, null);
		}

		[Test]
		public void Page_Constructor_Should_Fill_Properties()
		{
			// Arrange
			Page page = new Page();
			page.Id = 3;
			page.Title = "my title";
			page.CreatedBy = "me";
			page.CreatedOn = DateTime.Now;
			page.IsLocked = true;
			page.ModifiedBy = "me2";
			page.ModifiedOn = DateTime.Now.AddDays(1);
			page.Tags = "tag1,tag2,tag3";

			// Act
			PageViewModel model = new PageViewModel(page);

			// Assert
			Assert.That(model.Id, Is.EqualTo(page.Id));
			Assert.That(model.Title, Is.EqualTo(page.Title));
			Assert.That(model.CreatedBy, Is.EqualTo(page.CreatedBy));
			Assert.That(model.ModifiedBy, Is.EqualTo(page.ModifiedBy));
			Assert.That(model.CreatedOn, Is.EqualTo(page.CreatedOn));
			Assert.That(model.CreatedOn.Kind, Is.EqualTo(DateTimeKind.Utc));
			Assert.That(model.ModifiedOn, Is.EqualTo(page.ModifiedOn));
			Assert.That(model.ModifiedOn.Kind, Is.EqualTo(DateTimeKind.Utc));

			Assert.That(model.Tags.Count(), Is.EqualTo(3));
			Assert.That(model.Tags, Contains.Item("tag1"));
			Assert.That(model.Tags, Contains.Item("tag2"));
			Assert.That(model.Tags, Contains.Item("tag3"));
		}

		[Test]
		public void PageContent_Constructor_Should_Fill_Properties_And_Parse_Markup()
		{
			// Arrange
			PageContent content = new PageContent();
			content.Page = new Page();
			content.Page.Id = 3;
			content.Page.Title = "my title";
			content.Page.CreatedBy = "me";
			content.Page.CreatedOn = DateTime.Now;
			content.Page.IsLocked = true;
			content.Page.ModifiedBy = "me2";
			content.Page.ModifiedOn = DateTime.Now.AddDays(1);
			content.Page.Tags = "tag1,tag2,tag3";
			content.Text = "some text **in bold**";
			content.VersionNumber = 5;

			TextPluginStub plugin = new TextPluginStub();
			plugin.IsCacheable = false;
			plugin.HeadContent = "head content";
			plugin.FooterContent = "footer content";
			plugin.PreContainerHtml = "pre container";
			plugin.PostContainerHtml = "post container";
			plugin.PluginCache = new SiteCache(_appSettings, CacheMock.RoadkillCache);
			plugin.Repository = _repository;
			plugin.Settings.IsEnabled = true;
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			PageViewModel model = new PageViewModel(content, _markupConverter);

			// Assert
			Assert.That(model.Id, Is.EqualTo(content.Page.Id));
			Assert.That(model.Title, Is.EqualTo(content.Page.Title));
			Assert.That(model.CreatedBy, Is.EqualTo(content.Page.CreatedBy));
			Assert.That(model.ModifiedBy, Is.EqualTo(content.Page.ModifiedBy));
			Assert.That(model.VersionNumber, Is.EqualTo(content.VersionNumber));
			Assert.That(model.Content, Is.EqualTo(content.Text));

			Assert.That(model.CreatedOn, Is.EqualTo(content.Page.CreatedOn));
			Assert.That(model.CreatedOn.Kind, Is.EqualTo(DateTimeKind.Utc));
			Assert.That(model.ModifiedOn, Is.EqualTo(content.Page.ModifiedOn));
			Assert.That(model.ModifiedOn.Kind, Is.EqualTo(DateTimeKind.Utc));

			Assert.That(model.Tags.Count(), Is.EqualTo(3));
			Assert.That(model.Tags, Contains.Item("tag1"));
			Assert.That(model.Tags, Contains.Item("tag2"));
			Assert.That(model.Tags, Contains.Item("tag3"));

			// (this extra html is from the plugin)
			Assert.That(model.ContentAsHtml, Is.EqualTo("<p>some text <strong style='color:green'><iframe src='javascript:alert(test)'>in bold</strong>\n</p>"), model.ContentAsHtml);
			
			Assert.That(model.IsCacheable, Is.EqualTo(plugin.IsCacheable));
			Assert.That(model.PluginHeadHtml, Is.EqualTo(plugin.HeadContent));
			Assert.That(model.PluginFooterHtml, Is.EqualTo(plugin.FooterContent));
			Assert.That(model.PluginPreContainer, Is.EqualTo(plugin.PreContainerHtml));
			Assert.That(model.PluginPostContainer, Is.EqualTo(plugin.PostContainerHtml));

		}

		[Test]
		public void Content_Should_Be_Empty_And_Not_Null_When_Set_To_Null()
		{
			// Arrange
			PageViewModel model = new PageViewModel();			

			// Act
			model.Content = null;

			// Assert
			Assert.That(model.Content, Is.EqualTo(string.Empty));
		}

		[Test]
		public void IsNew_Should_Be_True_When_Id_Is_Not_Set()
		{
			// Arrange
			PageViewModel model = new PageViewModel();

			// Act
			model.Id = 0;
			
			// Assert
			Assert.That(model.IsNew, Is.True);
		}

		[Test]
		public void RawTags_Should_Be_Csv_Parsed_When_Set()
		{
			// Arrange
			PageViewModel model = new PageViewModel();

			// Act
			model.RawTags = "tag1, tag2, tag3";

			// Assert
			Assert.That(model.Tags.Count(), Is.EqualTo(3));
			Assert.That(model.Tags, Contains.Item("tag1"));
			Assert.That(model.Tags, Contains.Item("tag2"));
			Assert.That(model.Tags, Contains.Item("tag3"));
		}

		[Test]
		public void CommaDelimitedTags_Should_Return_Tags_In_Csv_Form()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "tag1, tag2, tag3";

			// Act
			string joinedTags = model.CommaDelimitedTags();

			// Assert
			Assert.That(joinedTags, Is.EqualTo("tag1,tag2,tag3"));
		}

		[Test]
		public void SpaceDelimitedTags_Should_Return_Tags_Space_Separated()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "tag1, tag2, tag3";

			// Act
			string joinedTags = model.SpaceDelimitedTags();

			// Assert
			Assert.That(joinedTags, Is.EqualTo("tag1 tag2 tag3"));
		}

		[Test]
		public void ParseTags_Should_Remove_Trailing_Whitespace_And_Empty_Elements()
		{
			// Arrange + Act
			IEnumerable<string> tags = PageViewModel.ParseTags("tag1, tag2, ,,    tag3      ,tag4");

			// Assert
			Assert.That(tags.Count(), Is.EqualTo(4));
			Assert.That(tags, Contains.Item("tag1"));
			Assert.That(tags, Contains.Item("tag2"));
			Assert.That(tags, Contains.Item("tag3"));
			Assert.That(tags, Contains.Item("tag4"));
		}

		[Test]
		public void JavascriptArrayForAllTags_Should_Return_Valid_Javascript_Array()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.AllTags.Add(new TagViewModel("tag1"));
			model.AllTags.Add(new TagViewModel("tag2"));
			model.AllTags.Add(new TagViewModel("tag3"));

			string expectedJavascript = "\"tag1\", \"tag2\", \"tag3\"";

			// Act
			string actualJavascript = model.JavascriptArrayForAllTags();

			// Assert
			Assert.That(actualJavascript, Is.EqualTo(expectedJavascript));
		}
	}
}
