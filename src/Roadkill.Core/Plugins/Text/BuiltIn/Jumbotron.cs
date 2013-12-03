using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;

namespace Roadkill.Core.Plugins.Text.BuiltIn
{
	public class Jumbotron : TextPlugin
	{
		internal static readonly string _regexString = @"\[\[\[jumbotron=(?'inner'.*?)\]\]\]";
		internal static readonly Regex _variableRegex = new Regex(_regexString, RegexOptions.Singleline | RegexOptions.Compiled);

		private string _preContainerHtml = "";
		private string _htmlTemplate = @"<div id=""roadkill-jumbotron"" class=""jumbotron""><div id=""inner"">${inner}</div></div>";
		private MarkupConverter _converter;

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
				return "Adds a giant image to the top of the page, with markdown overlayed ontop. Usage: [[[jumbotron=your markdown here]]]";
			}
		}

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		public Jumbotron(MarkupConverter converter) : base()
		{
			_converter = converter;
			_preContainerHtml = "";
		}

		public override string BeforeParse(string markupText)
		{
			// Check for the jumbotron token
			if (_variableRegex.IsMatch(markupText))
			{
				MatchCollection matches = _variableRegex.Matches(markupText);

				// All instances of the token
				if (matches.Count > 0)
				{
					Match match = matches[0];

					// Grab the markdown after the [[[jumbotron=..]]] and parse it,
					// and put it back in.
					string innerMarkDown = match.Groups["inner"].Value;
					string html = _converter.ToHtml(innerMarkDown);

					// _preContainerHtml is returned later and it contains the HTML that lives 
					// outside the container, that this plugin provides.
					_preContainerHtml = _htmlTemplate.Replace("${inner}", html);
				}
				
				// Remove the token from the markdown/creole
				markupText = Regex.Replace(markupText, _regexString, "", _variableRegex.Options);
			}

			return markupText;
		}

		public override string GetPreContainerHtml()
		{
			return _preContainerHtml;
		}

		public override string GetHeadContent()
		{
			return GetCssLink("jumbotron.css");
		}
	}
}