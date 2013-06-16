using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Roadkill.Core.Plugins.BuiltIn
{
	public class MathJax : CustomVariablePlugin
	{
		internal static readonly Regex _variableRegex = new Regex(@"\[\[\[mathjax]\]\]", RegexOptions.Singleline | RegexOptions.Compiled);
		private bool _hasMathJaxTag = false;

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
				return "Mathjax page";
			}
		}

		public override string Description
		{
			get
			{
				return "Enables MathJax (www.mathjax.org) support on the page. Any $$ content will then be converted, e.g. $$x = {-b \\pm \\sqrt{b^2-4ac} \\over 2a}.$$";
			}
		}

		public override string BeforeParse(string text)
		{
			if (_variableRegex.IsMatch(text))
			{
				MatchCollection matches = _variableRegex.Matches(text);
				foreach (Match match in matches)
				{
					text = _variableRegex.Replace(text, "");
					_hasMathJaxTag = true;
				}
			}

			return text;
		}

		public override string GetHeadContent(UrlHelper urlHelper)
		{
			if (_hasMathJaxTag)
				return "\t\t<script type=\"text/javascript\" src=\"http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML\"></script>\n";
			else
				return "";
		}
	}
}