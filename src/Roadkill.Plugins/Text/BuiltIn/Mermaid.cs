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

		public Mermaid()
		{
			AddScript("mermaid.min.js", "mermaid");
			// The script can finish loading before the page body is parsed, so the diagrams are drawn once the DOM is ready.
			SetHeadJsOnLoadedFunction(
				"mermaid.initialize({ startOnLoad: false }); " +
				"var roadkillMermaid = function() { mermaid.run({ querySelector: 'pre.mermaid' }); }; " +
				"if (document.readyState === 'loading') { document.addEventListener('DOMContentLoaded', roadkillMermaid); } else { roadkillMermaid(); }");
		}

		public override string GetHeadContent()
		{
			return GetJavascriptHtml();
		}
	}
}
