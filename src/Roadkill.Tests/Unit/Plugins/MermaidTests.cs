using NUnit.Framework;
using Roadkill.Plugins.Text.BuiltIn;

namespace Roadkill.Tests.Unit.Plugins
{
	[TestFixture]
	[Category("Unit")]
	public class MermaidTests
	{
		[Test]
		public void afterparse_should_turn_the_mermaid_pre_blocks_into_divs_and_leave_other_pre_blocks()
		{
			// Arrange
			Mermaid mermaid = new Mermaid();
			string html = "<pre class=\"mermaid\">graph TD\n  A --&gt; B\n</pre>\n<pre><code>code</code></pre>\n<pre class=\"mermaid\">pie\n</pre>";

			// Act
			string actualHtml = mermaid.AfterParse(html);

			// Assert
			Assert.That(actualHtml, Is.EqualTo("<div class=\"mermaid\">graph TD\n  A --&gt; B\n</div>\n<pre><code>code</code></pre>\n<div class=\"mermaid\">pie\n</div>"));
		}

		[Test]
		public void headcontent_should_contain_the_css_and_the_script()
		{
			// Arrange
			Mermaid mermaid = new Mermaid();

			// Act
			string headContent = mermaid.GetHeadContent();

			// Assert
			Assert.That(headContent, Does.Contain("mermaid.css"));
			Assert.That(headContent, Does.Contain("mermaid.min.js"));
			Assert.That(headContent, Does.Contain("theme: roadkillMermaidTheme(element)"));
		}
	}
}
