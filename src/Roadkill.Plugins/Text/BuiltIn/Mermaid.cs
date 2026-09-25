using System.Text.RegularExpressions;
using Roadkill.Core.Plugins;

namespace Roadkill.Plugins.Text.BuiltIn
{
	/// <summary>
	/// Renders the Markdown ```mermaid code blocks as diagrams, using the Mermaid javascript library (stored locally in
	/// the /Plugins/Mermaid folder, so no internet access is needed).
	/// </summary>
	public class Mermaid : TextPlugin
	{
		public override string Id
		{
			get
			{
				return "Mermaid";
			}
		}

		public override string Name
		{
			get
			{
				return "Mermaid diagrams";
			}
		}

		public override string Description
		{
			get
			{
				return "Draws diagrams (flowcharts, sequence diagrams, gantt charts...) from Markdown mermaid code blocks, using Mermaid (https://mermaid.js.org). Example:\n\n" +
						"```mermaid\ngraph TD\n    A[Start] --> B{Is it working?}\n    B -->|Yes| C[Great]\n```";
			}
		}

		public override string Version
		{
			get
			{
				return "1.0";
			}
		}

		// The Markdown parser writes the diagrams as <pre class="mermaid">, which the themes and Bootstrap style as code blocks
		// (dark or light background, borders, font size, word breaking...): a div keeps the diagrams out of these styles.
		private static readonly Regex PreRegex = new Regex(@"<pre class=""mermaid"">(?<diagram>.*?)</pre>", RegexOptions.Singleline | RegexOptions.Compiled);

		public Mermaid()
		{
			AddScript("mermaid.min.js", "mermaid");

			// Once the DOM is ready (the script can finish loading before the page body is parsed), the diagrams are drawn
			// with the Mermaid "dark" theme if the page background behind them is dark, otherwise with the default theme.
			SetHeadJsOnLoadedFunction(
				"var roadkillMermaidTheme = function(element) { " +
					"for (var node = element; node && node.nodeType === 1; node = node.parentElement) { " +
						"var rgba = (window.getComputedStyle(node).backgroundColor.match(/[\\d.]+/g) || []).map(Number); " +
						"if (rgba.length >= 3 && (rgba.length < 4 || rgba[3] > 0)) { return (0.299 * rgba[0] + 0.587 * rgba[1] + 0.114 * rgba[2]) < 128 ? 'dark' : 'default'; } " +
					"} " +
					"return 'default'; " +
				"}; " +
				"var roadkillMermaid = function() { " +
					"var element = document.querySelector('.mermaid') || document.getElementById('preview') || document.body; " +
					"mermaid.initialize({ startOnLoad: false, theme: roadkillMermaidTheme(element) }); " +
					"mermaid.run({ querySelector: '.mermaid' }); " +
				"}; " +
				"if (document.readyState === 'loading') { document.addEventListener('DOMContentLoaded', roadkillMermaid); } else { roadkillMermaid(); }");
		}

		public override string AfterParse(string html)
		{
			return PreRegex.Replace(html, "<div class=\"mermaid\">${diagram}</div>");
		}

		public override string GetHeadContent()
		{
			return GetCssLink("mermaid.css") + GetJavascriptHtml();
		}
	}
}
