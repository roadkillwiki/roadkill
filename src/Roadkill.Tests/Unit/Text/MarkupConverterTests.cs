using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Converters;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class MarkupConverterTests
	{
		private ConfigurationContainerStub _config;
		private MarkupConverter _converter;

		[SetUp]
		public void Setup()
		{
			_config = new ConfigurationContainerStub();
			_config.ApplicationSettings.UseHtmlWhiteList = true;
			_converter = new MarkupConverter(_config, null);
		}

		[Test]
		public void Parser_Should_Not_Be_Null_For_MarkupTypes()
		{
			// Arrange, act
			_config.SitePreferences.MarkupType = "Creole";
			_converter = new MarkupConverter(_config, null);

			// Assert
			Assert.NotNull(_converter.Parser);

			_config.SitePreferences.MarkupType = "Markdown";
			_converter = new MarkupConverter(_config, null);
			Assert.NotNull(_converter.Parser);

			_config.SitePreferences.MarkupType = "Mediawiki";
			_converter = new MarkupConverter(_config, null);
			Assert.NotNull(_converter.Parser);
		}

		[Test]
		public void ImageParsed_Should_Convert_To_Absolute_Path()
		{
			// Arrange
			
			_config.SitePreferences.MarkupType = "Markdown";
			_converter = new MarkupConverter(_config, null);

			_converter.AbsolutePathConverter = (string path) => 
			{ 
				return path + "123"; 
			};
			_converter.InternalUrlForTitle = (int pageId, string path) => { return path; };
			_converter.NewPageUrlForTitle = (string path) => { return path; };

			bool wasCalled = false;
			_converter.Parser.ImageParsed += (object sender, ImageEventArgs e) =>
			{
				wasCalled = (e.Src == "/DSC001.jpg123");
			};

			// Act
			_converter.ToHtml("![Image title](/DSC001.jpg)");
			

			// Assert
			Assert.True(wasCalled);
		}

		[Test]
		public void ImageParsed_Should_Ignore_Images_Starting_With_Http()
		{
			// Arrange
			_config.SitePreferences.MarkupType = "Markdown";
			_converter = new MarkupConverter(_config, null);

			_converter.AbsolutePathConverter = (string path) =>
			{
				return path + "123";
			};
			_converter.InternalUrlForTitle = (int pageId, string path) => { return path; };
			_converter.NewPageUrlForTitle = (string path) => { return path; };

			bool wasCalled = false;
			_converter.Parser.ImageParsed += (object sender, ImageEventArgs e) =>
			{
				wasCalled = (e.Src == "http://i223.photobucket.com/albums/dd45/wally2603/91e7840f.jpg");
			};

			// Act
			_converter.ToHtml("![Image title](http://i223.photobucket.com/albums/dd45/wally2603/91e7840f.jpg)");

			// Assert
			Assert.True(wasCalled);
		}

		[Test]
		public void ImageParsed_Should_Ignore_Images_Starting_With_Www()
		{
			// Arrange
			_config.SitePreferences.MarkupType = "Markdown";
			_converter = new MarkupConverter(_config, null);

			_converter.AbsolutePathConverter = (string path) =>
			{
				return path + "123";
			};
			_converter.InternalUrlForTitle = (int pageId, string path) => { return path; };
			_converter.NewPageUrlForTitle = (string path) => { return path; };

			bool wasCalled = false;
			_converter.Parser.ImageParsed += (object sender, ImageEventArgs e) =>
			{
				wasCalled = (e.Src == "www.photobucket.com/albums/dd45/wally2603/91e7840f.jpg");
			};

			// Act
			_converter.ToHtml("![Image title](www.photobucket.com/albums/dd45/wally2603/91e7840f.jpg)");

			// Assert
			Assert.True(wasCalled);
		}

		[Test]
		public void Should_Remove_Script_Link_Iframe_Frameset_Frame_Applet_Tags_From_Text()
		{
			// Arrange
			_config.SitePreferences.MarkupType = "Markdown";
			string markdown = " some text <script type=\"text/html\">while(true)alert('lolz');</script>" +
				"<iframe src=\"google.com\"></iframe><frame>blah</frame> <applet code=\"MyApplet.class\" width=100 height=140></applet>" +
				"<frameset src='new.html'></frameset>";

			string expectedHtml = "<p> some text blah \n</p>";

			// Act
			string actualHtml = _converter.ToHtml(markdown);

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Links_Starting_With_Https_Or_Hash_Are_Ignored()
		{
			// Arrange
			_config.SitePreferences.MarkupType = "Creole";

			string expectedHtml = "<p><a href=\"&#x23;myanchortag\">hello world</a> <a href=\"https&#x3A;&#x2F;&#x2F;www&#x2E;google&#x2E;com\">google</a>\n</p>";
			MarkupConverter converter = new MarkupConverter(_config, null);

			// Act
			string actualHtml = converter.ToHtml("[[#myanchortag|hello world]] [[https://www.google.com|google]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Links_Starting_With_Http_Www_Mailto_Tag_Are_Ignored()
		{
			// Arrange
			_config.SitePreferences.MarkupType = "Creole";

			string expectedHtml = "<p><a href=\"http&#x3A;&#x2F;&#x2F;www&#x2E;blah&#x2E;com\">link1</a> <a href=\"www&#x2E;blah&#x2E;com\">link2</a> <a href=\"mailto&#x3A;spam&#x40;gmail&#x2E;com\">spam</a>\n</p>";
			MarkupConverter converter = new MarkupConverter(_config, null);

			// Act
			string actualHtml = converter.ToHtml("[[http://www.blah.com|link1]] [[www.blah.com|link2]] [[mailto:spam@gmail.com|spam]]");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Html_Should_Not_Be_Sanitized_If_UseHtmlWhiteList_Setting_Is_False()
		{
			// Arrange
			_config.SitePreferences.MarkupType = "Creole";
			_config.ApplicationSettings.UseHtmlWhiteList = false;

			string htmlFragment = "<div onclick=\"javascript:alert('ouch');\">test</div>";
			MarkupConverter converter = new MarkupConverter(_config, null);

			// Act
			string actualHtml = converter.ToHtml(htmlFragment);

			// Assert
			string expectedHtml = "<p>" +htmlFragment+ "\n</p>";
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
