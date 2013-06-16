using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Plugins
{
	public abstract class CustomVariablePlugin
	{
		public static readonly string PARSER_IGNORE_STARTTOKEN = "{{{roadkillinternal";
		public static readonly string PARSER_IGNORE_ENDTOKEN = "roadkillinternal}}}";

		public static string AddParserIgnoreTokens(string replacementRegex)
		{
			// The new lines are important for the current Creole parser to recognise the ignore token.
			return "n" + PARSER_IGNORE_STARTTOKEN + " \n" + replacementRegex + "\n" + PARSER_IGNORE_ENDTOKEN + "\n";
		}

		public abstract string Name { get; }
		public abstract string Description { get; }
		public ApplicationSettings ApplicationSettings { get; set; }
		public SiteSettings SiteSettings { get; set; }
		protected List<PluginSetting> Settings { get; set; }

		public virtual string GetHeadContent(UrlHelper urlHelper)
		{
			return "";
		}

		public abstract string BeforeParse(string markupText);

		public virtual string AfterParse(string html)
		{
			html = RemoveParserIgnoreTokens(html);
			return html;
		}

		public virtual string RemoveParserIgnoreTokens(string html)
		{
			html = html.Replace(PARSER_IGNORE_STARTTOKEN, "");
			html = html.Replace(PARSER_IGNORE_ENDTOKEN, "");

			return html;
		}
	}
}
