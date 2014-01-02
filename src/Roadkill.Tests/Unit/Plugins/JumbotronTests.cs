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
using Roadkill.Core.Plugins.SpecialPages.BuiltIn;
using Roadkill.Core.Plugins.Text.BuiltIn;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Plugins
{
	[TestFixture]
	[Category("Unit")]
	public class JumbotronTests
	{
		private MocksAndStubsContainer _container;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
		}

		[Test]
		public void Should_Remove_Jumbotron_Tag_From_Markup()
		{
			// Arrange
			string markup = "Here is some ===Heading 1=== markup \n[[[jumbotron=\n==Welcome==\n==This the subheading==]]]";
			Jumbotron jumbotron = new Jumbotron(_container.MarkupConverter);

			// Act
			string actualMarkup = jumbotron.BeforeParse(markup);

			// Assert
			Assert.That(actualMarkup, Is.EqualTo("Here is some ===Heading 1=== markup \n"));
		}

		[Test]
		public void Should_Parse_And_Fill_PreContainerHtml()
		{
			// Arrange
			string markup = "Here is some ===Heading 1=== markup \n[[[jumbotron==Welcome=\n==This the subheading==]]]";
			string expectedHtml = Jumbotron.HTMLTEMPLATE.Replace("${inner}", "<p><h1>Welcome</h1><h2>This the subheading</h2></p>");

			Jumbotron jumbotron = new Jumbotron(_container.MarkupConverter);

			// Act
			jumbotron.BeforeParse(markup);
			string actualHtml = jumbotron.GetPreContainerHtml();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
