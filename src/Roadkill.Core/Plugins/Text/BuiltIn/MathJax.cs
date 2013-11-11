using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Core.Plugins.Text.BuiltIn
{
	public class MathJax : TextPlugin
	{
		private static readonly string _token = "[[[mathjax]]]";
		private static readonly string _parserSafeToken;

		public override bool IsEnabled
		{
			get
			{
				return false;
			}
		}

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
			_parserSafeToken = ParserSafeToken(_token);
		}

		public override string BeforeParse(string markupText)
		{
			return markupText.Replace(_token, _parserSafeToken);
		}

		public override string AfterParse(string html)
		{
			if (html.Contains(_token))
			{
				return html.Replace(_token, "");
			}
			else
			{
				return html;
			}
		}

		public override string GetHeadContent()
		{
			return "\t\t<script type=\"text/javascript\" src=\"http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML\"></script>\n";
		}
	}
}