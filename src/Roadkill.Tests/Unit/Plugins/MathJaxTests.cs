using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Plugins.Text.BuiltIn;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Plugins
{
	[TestFixture]
	[Category("Unit")]
	public class MathJaxTests
	{
		private MocksAndStubsContainer _container;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
		}

		[Test]
		public void should_replace_token_after_parse()
		{
			// Arrange
			string expectedHtml = "<p> $$e=mc2$$</p>";
			MathJax mathjax = new MathJax();

			// Act
			string actualHtml = mathjax.AfterParse("<p>[[[mathjax]]] $$e=mc2$$</p>");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void headcontent_should_contain_script_tag_and_cdn_src()
		{
			// Arrange
			string expectedScriptTag = "<script type=\"text/javascript\" src=\"http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML\"></script>";
			MathJax mathjax = new MathJax();

			// Act
			string scriptTag = mathjax.GetHeadContent();

			// Assert
			Assert.That(scriptTag, Is.StringContaining(expectedScriptTag));
		}
	}
}
