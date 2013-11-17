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
	public class Jumbotron : TextPlugin
	{
		internal static readonly string _regexString = @"\[\[\[jumbotron=(?'inner'.*?)\]\]\]";
		internal static readonly Regex _variableRegex = new Regex(_regexString, RegexOptions.Singleline | RegexOptions.Compiled);

		private string _preContainerHtml = @"<div id=""roadkill-jumbotron"" class=""jumbotron""><div id=""inner"">${inner}</div></div>";
		private bool _hasJumbotronTag = false;

		public override string Id
		{
			get 
			{ 
				return "Jumbotron";	
			}
		}

		public override string Name
		{
			get
			{
				return "Jumbotron";
			}
		}

		public override string Description
		{
			get
			{
				return "Adds a giant image to the top of the page, with custom HTML overlayed ontop. Usage: [[[jumbotron=your HTML here]]]";
			}
		}

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		public override string BeforeParse(string markupText)
		{
			if (_variableRegex.IsMatch(markupText))
			{
				MatchCollection matches = _variableRegex.Matches(markupText);

				foreach (Match match in matches)
				{
					string inner = match.Groups["inner"].Value;
					_preContainerHtml = _preContainerHtml.Replace("${inner}", inner);
					_hasJumbotronTag = true;
					break;
				}
				
				// Remove the token it from the markdown/creole
				markupText = Regex.Replace(markupText, _regexString, "", _variableRegex.Options);
			}

			return markupText;
		}

		public override string GetPreContainerHtml()
		{
			if (_hasJumbotronTag)
				return _preContainerHtml;
			else
				return "";
		}

		public override string GetHeadContent()
		{
			return GetCssLink("jumbotron.css");
		}
	}
}