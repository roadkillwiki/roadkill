using Roadkill.Core.Plugins;

namespace Roadkill.Plugins.Text.BuiltIn
{
	public class MathJax : TextPlugin
	{
		internal static readonly string TOKEN = "[[[mathjax]]]";
		internal static readonly string PARSER_SAFE_TOKEN;

		public override string Id
		{
			get 
			{ 
				return "MathJax";	
			}
		}

		public override string Name
		{
			get
			{
				return "Mathjax";
			}
		}

		public override string Description
		{
			get
			{
				return "Enables MathJax (www.mathjax.org) support on the page. Any content inside $$ $$ will then be converted, for example: \n\n"+
						"$$x = {-b \\pm \\sqrt{b^2-4ac} \\over 2a}.$$";
			}
		}

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		static MathJax()
		{
			PARSER_SAFE_TOKEN = ParserSafeToken(TOKEN);
		}

		public override string BeforeParse(string markupText)
		{
			return markupText.Replace(TOKEN, PARSER_SAFE_TOKEN);
		}

		public override string AfterParse(string html)
		{
			if (html.Contains(TOKEN))
			{
				return html.Replace(TOKEN, "");
			}
			else
			{
				return html;
			}
		}

		public override string GetHeadContent()
		{
			// Mathjax runs on the server, so use the CDN.
			return "\t\t<script type=\"text/javascript\" src=\"http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML\"></script>\n";
		}
	}
}