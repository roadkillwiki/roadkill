using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Core.Plugins.BuiltIn
{
	public class MathJax : CustomVariablePlugin
	{
		private static readonly string _token = "[[[mathjax]]]";
		private static readonly string _parserSafeToken;
		
		// Set it site wide for now, until the plugin architecture is finished.
		private static bool _hasMathJaxTag = false;

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
				return "Enables MathJax (www.mathjax.org) support on the page. Any content inside $$ $$ will then be converted, e.g. $$x = {-b \\pm \\sqrt{b^2-4ac} \\over 2a}.$$";
			}
		}

		static MathJax()
		{
			_parserSafeToken = ParserSafeToken(_token);
		}

		public MathJax(ApplicationSettings applicationSettings, IRepository repository)
			: base(applicationSettings, repository)
		{
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