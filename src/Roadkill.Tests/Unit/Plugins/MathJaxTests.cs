using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public void headcontent_should_load_the_local_mathjax_script_and_no_cdn()
		{
			// Arrange
			MathJax mathjax = new MathJax();

			// Act
			string headContent = mathjax.GetHeadContent();

			// Assert
			Assert.That(headContent, Does.Contain("tex-mml-chtml.js"));
			Assert.That(headContent, Does.Not.Contain("cdn.mathjax.org"));
		}
	}
}
