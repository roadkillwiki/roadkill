using System;
using System.IO;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Text
{
	[TestFixture]
	[Category("Unit")]
	public class MarkupConverterTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private SettingsRepositoryMock _settingsRepository;
		private PageRepositoryMock _pageRepository;
		private PluginFactoryMock _pluginFactory;
		private MarkupConverter _markupConverter;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.UseHtmlWhiteList = true;
			_applicationSettings.CustomTokensPath = Path.Combine(TestConstants.WEB_PATH, "App_Data", "customvariables.xml");

			_settingsRepository = _container.SettingsRepository;
			_pageRepository = _container.PageRepository;

			_pluginFactory = _container.PluginFactory;
			_markupConverter = _container.MarkupConverter;
			_markupConverter.UrlResolver = new UrlResolverMock();
		}

		[Test]
		public void parser_should_not_be_null_for_markuptypes()
		{
			// Arrange, act
			_settingsRepository.SiteSettings.MarkupType = "Creole";

			// Assert
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);
			Assert.NotNull(_markupConverter.Parser);

			_settingsRepository.SiteSettings.MarkupType = "Markdown";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);
			Assert.NotNull(_markupConverter.Parser);
		}

		[Test]
		[ExpectedException(typeof(NotImplementedException))]
		public void Parser_Should_Throw_Exception_For_MediaWiki()
		{
			// Arrange, act + assert
			_settingsRepository.SiteSettings.MarkupType = "MediaWiki";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);
		}

		[Test]
		public void imageparsed_should_convert_to_absolute_path()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Markdown";
			UrlResolverMock resolver = new UrlResolverMock();
			resolver.AbsolutePathSuffix = "123";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);
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
			_settingsRepository.SiteSettings.MarkupType = "Markdown";
			UrlResolverMock resolver = new UrlResolverMock();
			resolver.AbsolutePathSuffix = "123";

			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);
			_markupConverter.UrlResolver = resolver;

			bool wasCalled = false;
			_markupConverter.Parser.ImageParsed += (object sender, ImageEventArgs e) =>
			{
				wasCalled = (e.Src == imageUrl);
			};

			// Act
			_markupConverter.ToHtml("![Image title](" + imageUrl + ")");

			// Assert
			Assert.True(wasCalled);
		}

		[Test]
		public void should_remove_script_link_iframe_frameset_frame_applet_tags_from_text()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);
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
		public void links_starting_with_https_or_hash_are_not_rewritten_as_internal()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a rel=\"nofollow\" href=\"&#x23;myanchortag\">hello world</a> <a rel=\"nofollow\" href=\"https&#x3A;&#x2F;&#x2F;www&#x2E;google&#x2E;com\" class=\"external&#x2D;link\">google</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[#myanchortag|hello world]] [[https://www.google.com|google]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void links_with_dashes_or_23_are_rewritten_and_not_parsed_as_encoded_hashes()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a rel=\"nofollow\" href=\"&#x23;myanchortag\">hello world</a> <a rel=\"nofollow\" href=\"https&#x3A;&#x2F;&#x2F;www&#x2E;google&#x2E;com&#x2F;some&#x2D;page&#x2D;23\" class=\"external&#x2D;link\">google</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[#myanchortag|hello world]] [[https://www.google.com/some-page-23|google]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void links_to_named_anchors_should_not_have_external_css_class()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a rel=\"nofollow\" href=\"&#x23;myanchortag\">hello world</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[#myanchortag|hello world]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void links_starting_with_tilde_should_resolve_as_attachment_paths()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;Attachments&#x2F;my&#x2F;folder&#x2F;image1&#x2E;jpg\">hello world</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[~/my/folder/image1.jpg|hello world]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void external_links_with_anchor_tag_should_retain_the_anchor()
		{
			// Issue #172
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_pageRepository.AddNewPage(new Page() { Id = 1, Title = "foo" }, "foo", "admin", DateTime.Today);
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a rel=\"nofollow\" href=\"http&#x3A;&#x2F;&#x2F;www&#x2E;google&#x2E;com&#x2F;&#x3F;blah&#x3D;xyz&#x23;myanchor\" class=\"external&#x2D;link\">Some link text</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[http://www.google.com/?blah=xyz#myanchor|Some link text]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void internal_wiki_page_link_should_not_have_nofollow_attribute()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_pageRepository.AddNewPage(new Page() { Id = 1, Title = "foo-page" }, "foo", "admin", DateTime.Today);
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;1&#x2F;foo&#x2D;page\">Some link text</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[foo-page|Some link text]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void attachment_link_should_not_have_nofollow_attribute()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;Attachments&#x2F;folder&#x2F;myfile&#x2E;jpg\">Some link text</a> <a href=\"&#x2F;Attachments&#x2F;folder2&#x2F;myfile&#x2E;jpg\">Some link text</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[~/folder/myfile.jpg|Some link text]] [[attachment:/folder2/myfile.jpg|Some link text]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void specialurl_link_should_not_have_nofollow_attribute()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;Special&#x3A;Random\">Some link text</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[Special:Random|Some link text]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void internal_links_with_anchor_tag_should_retain_the_anchor()
		{
			// Issue #172
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_pageRepository.AddNewPage(new Page() { Id = 1, Title = "foo" }, "foo", "admin", DateTime.Today);
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;1&#x2F;foo&#x23;myanchor\">Some link text</a>\n</p>"; // use /index/ as no routing exists

			// Act
			string actualHtml = _markupConverter.ToHtml("[[foo#myanchor|Some link text]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void internal_links_with_urlencoded_anchor_tag_should_retain_the_anchor()
		{
			// Issue #172
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_pageRepository.AddNewPage(new Page() { Id = 1, Title = "foo" }, "foo", "admin", DateTime.Today);
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;1&#x2F;foo&#x25;23myanchor\">Some link text</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[foo%23myanchor|Some link text]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void internal_links_with_anchor_tag_should_retain_the_anchor_with_markdown()
		{
			// Issue #172
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Markdown";
			_pageRepository.AddNewPage(new Page() { Id = 1, Title = "foo" }, "foo", "admin", DateTime.Today);
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;1&#x2F;foo&#x23;myanchor\">Some link text</a></p>\n"; // use /index/ as no routing exists

			// Act
			string actualHtml = _markupConverter.ToHtml("[Some link text](foo#myanchor)");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void links_with_the_word_script_in_url_should_not_be_cleaned()
		{
			// Issue #159
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a rel=\"nofollow\" href=\"http&#x3A;&#x2F;&#x2F;msdn&#x2E;microsoft&#x2E;com&#x2F;en&#x2D;us&#x2F;library&#x2F;system&#x2E;componentmodel&#x2E;descriptionattribute&#x2E;aspx\" class=\"external&#x2D;link\">ComponentModel.Description</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[http://msdn.microsoft.com/en-us/library/system.componentmodel.descriptionattribute.aspx|ComponentModel.Description]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void links_with_angle_brackets_and_quotes_should_be_encoded()
		{
			// Issue #159
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a rel=\"nofollow\" href=\"http&#x3A;&#x2F;&#x2F;www&#x2E;google&#x2E;com&#x2F;&#x22;&#x3E;javascript&#x3A;alert&#x28;&#x27;hello&#x27;&#x29;\" class=\"external&#x2D;link\">ComponentModel</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[http://www.google.com/\">javascript:alert('hello')|ComponentModel]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}


		[Test]
		public void links_starting_with_attachmentcolon_should_resolve_as_attachment_paths()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;Attachments&#x2F;my&#x2F;folder&#x2F;image1&#x2E;jpg\">hello world</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[attachment:/my/folder/image1.jpg|hello world]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void links_starting_with_specialcolon_should_resolve_as_full_specialpage()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a href=\"&#x2F;wiki&#x2F;Special&#x3A;Foo\">My special page</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[Special:Foo|My special page]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void links_starting_with_http_www_mailto_tag_are_no_rewritten_as_internal()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string expectedHtml = "<p><a rel=\"nofollow\" href=\"http&#x3A;&#x2F;&#x2F;www&#x2E;blah&#x2E;com\" class=\"external&#x2D;link\">link1</a> <a rel=\"nofollow\" href=\"www&#x2E;blah&#x2E;com\" class=\"external&#x2D;link\">link2</a> <a rel=\"nofollow\" href=\"mailto&#x3A;spam&#x40;gmail&#x2E;com\" class=\"external&#x2D;link\">spam</a>\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml("[[http://www.blah.com|link1]] [[www.blah.com|link2]] [[mailto:spam@gmail.com|spam]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void html_should_not_be_sanitized_if_usehtmlwhitelist_setting_is_false()
		{
			// Arrange
			_applicationSettings.UseHtmlWhiteList = false;
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			string htmlFragment = "<div onclick=\"javascript:alert('ouch');\">test</div>";
			MarkupConverter converter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);

			// Act
			string actualHtml = converter.ToHtml(htmlFragment);

			// Assert
			string expectedHtml = "<p>" + htmlFragment + "\n</p>";
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_not_render_toc_with_multiple_curlies()
		{
			// Arrange
			_settingsRepository.SiteSettings.MarkupType = "Creole";
			_markupConverter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);
			_markupConverter.UrlResolver = new UrlResolverMock();

			string htmlFragment = "Give me a {{TOC}} and a {{{TOC}}} - the should not render a TOC";
			string expected = @"<p>Give me a <div class=""floatnone""><div class=""image&#x5F;frame""><img src=""&#x2F;Attachments&#x2F;TOC""></div></div> and a TOC - the should not render a TOC"
				+ "\n</p>";

			// Act
			string actualHtml = _markupConverter.ToHtml(htmlFragment);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expected));
		}

		[Test]
		public void warningbox_token_with_nowiki_adds_pre_and_renders_token_html()
		{
			// Arrange..make sure expectedHtml uses \n and not \r\n
			string expectedHtml = @"<p><div class=""alert alert-warning"">ENTER YOUR CONTENT HERE 
<pre>here is my C#code
</pre>
</p>
<p></div><br style=""clear:both""/>
</p>";

			expectedHtml = expectedHtml.Replace("\r\n", "\n"); // fix line ending issues

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
		public void should_ignore_textplugins_beforeparse_when_isenabled_is_false()
		{
			// Arrange
			string markupFragment = "This is my ~~~usertoken~~~";
			string expectedHtml = "<p>This is my <span>usertoken</span>\n</p>";

			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = new SettingsRepositoryMock();
			plugin.PluginCache = new SiteCache(CacheMock.RoadkillCache);
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			string actualHtml = _markupConverter.ToHtml(markupFragment);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_ignore_textplugins_afterparse_when_isenabled_is_false()
		{
			// Arrange
			string markupFragment = "Here is some markup **some bold**";
			string expectedHtml = "<p>Here is some markup <strong style='color:green'><iframe src='javascript:alert(test)'>some bold</strong>\n</p>";

			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = new SettingsRepositoryMock();
			plugin.PluginCache = new SiteCache(CacheMock.RoadkillCache);
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			string actualHtml = _markupConverter.ToHtml(markupFragment);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_fire_beforeparse_in_textplugin()
		{
			// Arrange
			string markupFragment = "This is my ~~~usertoken~~~";
			string expectedHtml = "<p>This is my <span>usertoken</span>\n</p>";

			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = new SettingsRepositoryMock();
			plugin.PluginCache = new SiteCache(CacheMock.RoadkillCache);
			plugin.Settings.IsEnabled = true;
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			string actualHtml = _markupConverter.ToHtml(markupFragment);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_fire_afterparse_in_textplugin_and_output_should_not_be_cleaned()
		{
			// Arrange
			string markupFragment = "Here is some markup **some bold**";
			string expectedHtml = "<p>Here is some markup <strong style='color:green'><iframe src='javascript:alert(test)'>some bold</strong>\n</p>";

			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = new SettingsRepositoryMock();
			plugin.PluginCache = new SiteCache(CacheMock.RoadkillCache);
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
