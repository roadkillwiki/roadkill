using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;
using Roadkill.Core.Text;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class MarkupConverterTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private RepositoryMock _repository;
		private PluginFactoryMock _pluginFactory;
		private MarkupConverter _markupConverter;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.UseHtmlWhiteList = true;
			_applicationSettings.CustomTokensPath = Path.Combine(Settings.WEB_PATH, "App_Data", "customvariables.xml");

			_pluginFactory = _container.PluginFactory;
			_repository = _container.Repository;
			_markupConverter = _container.MarkupConverter;
			_markupConverter.UrlResolver = new UrlResolverMock();
		}

		[Test]
		public void Parser_Should_Not_Be_Null_For_MarkupTypes()
		{
			// Arrange, act
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			// Assert
			Assert.NotNull(_markupConverter.Parser);

			_repository.SiteSettings.MarkupType = "Markdown";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);
			Assert.NotNull(_markupConverter.Parser);

			_repository.SiteSettings.MarkupType = "Mediawiki";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);
			Assert.NotNull(_markupConverter.Parser);
		}

		[Test]
		public void ImageParsed_Should_Convert_To_Absolute_Path()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Markdown";
			UrlResolverMock resolver = new UrlResolverMock();
			resolver.AbsolutePathSuffix = "123";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);
			_markupConverter.UrlResolver = resolver; 

			// Act
			bool wasCalled = false;
			_markupConverter.Parser.ImageParsed += (object sender, ImageEventArgs e) =>
			{
				wasCalled = (e.Src == "/Attachments/DSC001.jpg123");
			};

			_markupConverter.ToHtml("![Image title](/DSC001.jpg)");
			
			// Assert
			Assert.True(wasCalled, "ImageParsed.ImageEventArgs.Src did not match.");
		}

		[Test]
		[TestCase("http://i223.photobucket.com/albums/dd45/wally2603/91e7840f.jpg")]
		[TestCase("https://i223.photobucket.com/albums/dd45/wally2603/91e7840f.jpg")]
		[TestCase("www.photobucket.com/albums/dd45/wally2603/91e7840f.jpg")]
		public void ImageParsed_Should_Not_Rewrite_Images_As_Internal_That_Start_With_Known_Prefixes(string imageUrl)
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Markdown";
			UrlResolverMock resolver = new UrlResolverMock();
			resolver.AbsolutePathSuffix = "123";

			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);
			_markupConverter.UrlResolver = resolver;

			bool wasCalled = false;
			_markupConverter.Parser.ImageParsed += (object sender, ImageEventArgs e) =>
			{
				wasCalled = (e.Src == imageUrl);
			};

			// Act
			_markupConverter.ToHtml("![Image title](" +imageUrl+ ")");

			// Assert
			Assert.True(wasCalled);
		}

		[Test]
		public void Should_Remove_Script_Link_Iframe_Frameset_Frame_Applet_Tags_From_Text()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);
			string markdown = " some text <script type=\"text/html\">while(true)alert('lolz');</script>" +
				"<iframe src=\"google.com\"></iframe><frame>blah</frame> <applet code=\"MyApplet.class\" width=100 height=140></applet>" +
				"<frameset src='new.html'></frameset>";

			string expectedHtml = "<p> some text blah \n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml(markdown);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Links_Starting_With_Https_Or_Hash_Are_Not_Rewritten_As_Internal()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x23;myanchortag\">hello world</a> <a href=\"https&#x3A;&#x2F;&#x2F;www&#x2E;google&#x2E;com\">google</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[#myanchortag|hello world]] [[https://www.google.com|google]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Links_With_Dashes_Or_23_Are_Rewritten_And_Not_Parsed_As_Encoded_Hashes()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x23;myanchortag\">hello world</a> <a href=\"https&#x3A;&#x2F;&#x2F;www&#x2E;google&#x2E;com&#x2F;some&#x2D;page&#x2D;23\">google</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[#myanchortag|hello world]] [[https://www.google.com/some-page-23|google]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Links_Starting_With_Tilde_Should_Resolve_As_Attachment_Paths()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;Attachments&#x2F;my&#x2F;folder&#x2F;image1&#x2E;jpg\">hello world</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[~/my/folder/image1.jpg|hello world]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void External_Links_With_Anchor_Tag_Should_Retain_The_Anchor()
		{
			// Issue #172
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_repository.AddNewPage(new Page() { Id = 1, Title = "foo" }, "foo", "admin", DateTime.Today);
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"http&#x3A;&#x2F;&#x2F;www&#x2E;google&#x2E;com&#x2F;&#x3F;blah&#x3D;xyz&#x23;myanchor\">Some link text</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[http://www.google.com/?blah=xyz#myanchor|Some link text]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Internal_Links_With_Anchor_Tag_Should_Retain_The_Anchor()
		{
			// Issue #172
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_repository.AddNewPage(new Page() { Id = 1, Title = "foo" }, "foo", "admin", DateTime.Today);
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;1&#x2F;foo&#x23;myanchor\">Some link text</a>\n</p>"; // use /index/ as no routing exists

			// Act
			string actualHtml = _markupConverter.ToHtml("[[foo#myanchor|Some link text]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Internal_Links_With_UrlEncoded_Anchor_Tag_Should_Retain_The_Anchor()
		{
			// Issue #172
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_repository.AddNewPage(new Page() { Id = 1, Title = "foo" }, "foo", "admin", DateTime.Today);
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;1&#x2F;foo&#x25;23myanchor\">Some link text</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[foo%23myanchor|Some link text]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Internal_Links_With_Anchor_Tag_Should_Retain_The_Anchor_With_Markdown()
		{
			// Issue #172
			// Arrange
			_repository.SiteSettings.MarkupType = "Markdown";
			_repository.AddNewPage(new Page() { Id = 1, Title = "foo" }, "foo", "admin", DateTime.Today);
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;1&#x2F;foo&#x23;myanchor\">Some link text</a></p>\n"; // use /index/ as no routing exists

			// Act
			string actualHtml = _markupConverter.ToHtml("[Some link text](foo#myanchor)");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Links_With_The_Word_Script_In_Url_Should_Not_Be_Cleaned()
		{
			// Issue #159
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"http&#x3A;&#x2F;&#x2F;msdn&#x2E;microsoft&#x2E;com&#x2F;en&#x2D;us&#x2F;library&#x2F;system&#x2E;componentmodel&#x2E;descriptionattribute&#x2E;aspx\">ComponentModel.Description</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[http://msdn.microsoft.com/en-us/library/system.componentmodel.descriptionattribute.aspx|ComponentModel.Description]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Links_With_Angle_Brackets_And_Quotes_Should_Be_Encoded()
		{
			// Issue #159
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"http&#x3A;&#x2F;&#x2F;www&#x2E;google&#x2E;com&#x2F;&#x22;&#x3E;javascript&#x3A;alert&#x28;&#x27;hello&#x27;&#x29;\">ComponentModel</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[http://www.google.com/\">javascript:alert('hello')|ComponentModel]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}
	

		[Test]
		public void Links_Starting_With_AttachmentColon_Should_Resolve_As_Attachment_Paths()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;Attachments&#x2F;my&#x2F;folder&#x2F;image1&#x2E;jpg\">hello world</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[attachment:/my/folder/image1.jpg|hello world]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Links_Starting_With_SpecialColon_Should_Resolve_As_Full_SpecialPage()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;Special&#x3A;Foo\">My special page</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[Special:Foo|My special page]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Links_Starting_With_Http_Www_Mailto_Tag_Are_No_Rewritten_As_Internal()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string expectedHtml = "<p><a href=\"http&#x3A;&#x2F;&#x2F;www&#x2E;blah&#x2E;com\">link1</a> <a href=\"www&#x2E;blah&#x2E;com\">link2</a> <a href=\"mailto&#x3A;spam&#x40;gmail&#x2E;com\">spam</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[http://www.blah.com|link1]] [[www.blah.com|link2]] [[mailto:spam@gmail.com|spam]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Html_Should_Not_Be_Sanitized_If_UseHtmlWhiteList_Setting_Is_False()
		{
			// Arrange
			_applicationSettings.UseHtmlWhiteList = false;
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			string htmlFragment = "<div onclick=\"javascript:alert('ouch');\">test</div>";
			MarkupConverter converter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);

			// Act
			string actualHtml = converter.ToHtml(htmlFragment);

			// Assert
			string expectedHtml = "<p>" +htmlFragment+ "\n</p>";
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Should_Not_Render_ToC_With_Multiple_Curlies()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _repository, _pluginFactory);
			_markupConverter.UrlResolver = new UrlResolverMock();

			string htmlFragment = "Give me a {{TOC}} and a {{{TOC}}} - the should not render a TOC";
			string expected = @"<p>Give me a <div class=""floatnone""><div class=""image&#x5F;frame""><img src=""&#x2F;Attachments&#x2F;TOC""></div></div> and a TOC - the should not render a TOC"
				+"\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml(htmlFragment);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expected));
		}

		[Test]
		public void WarningBox_Token_With_NoWiki_Adds_Pre_And_Renders_Token_HTML()
		{
			// Arrange
			string expectedHtml = @"<p><div class=""alert alert-warning"">ENTER YOUR CONTENT HERE 
<pre>here is my C#code
</pre>
</p>
<p></div><br style=""clear:both""/>
</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml(@"@@warningbox:ENTER YOUR CONTENT HERE 
{{{
here is my C#code
}}} 

@@");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Should_Ignore_TextPlugins_BeforeParse_When_IsEnabled_Is_False()
		{
			// Arrange
			string markupFragment = "This is my ~~~usertoken~~~";
			string expectedHtml = "<p>This is my <span>usertoken</span>\n</p>";

			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = new RepositoryMock();
			plugin.PluginCache = new SiteCache(new ApplicationSettings(), CacheMock.RoadkillCache);
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			string actualHtml = _markupConverter.ToHtml(markupFragment);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Should_Ignore_TextPlugins_AfterParse_When_IsEnabled_Is_False()
		{
			// Arrange
			string markupFragment = "Here is some markup **some bold**";
			string expectedHtml = "<p>Here is some markup <strong style='color:green'><iframe src='javascript:alert(test)'>some bold</strong>\n</p>";

			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = new RepositoryMock();
			plugin.PluginCache = new SiteCache(new ApplicationSettings(), CacheMock.RoadkillCache);
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			string actualHtml = _markupConverter.ToHtml(markupFragment);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Should_Fire_BeforeParse_In_TextPlugin()
		{
			// Arrange
			string markupFragment = "This is my ~~~usertoken~~~";
			string expectedHtml = "<p>This is my <span>usertoken</span>\n</p>";
			
			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = new RepositoryMock();
			plugin.PluginCache = new SiteCache(new ApplicationSettings(), CacheMock.RoadkillCache);
			plugin.Settings.IsEnabled = true;
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			string actualHtml = _markupConverter.ToHtml(markupFragment);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Should_Fire_AfterParse_In_TextPlugin_And_Output_Should_Not_Be_Cleaned()
		{
			// Arrange
			string markupFragment = "Here is some markup **some bold**";
			string expectedHtml = "<p>Here is some markup <strong style='color:green'><iframe src='javascript:alert(test)'>some bold</strong>\n</p>";
			
			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = new RepositoryMock();
			plugin.PluginCache = new SiteCache(new ApplicationSettings(), CacheMock.RoadkillCache);
			plugin.Settings.IsEnabled = true;
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			string actualHtml = _markupConverter.ToHtml(markupFragment);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		// TODO:
		// ContainsPageLink - 
		// ReplacePageLinks - Refactor into seperate class

		// TOCParser
		// Creole tests
	}
}
