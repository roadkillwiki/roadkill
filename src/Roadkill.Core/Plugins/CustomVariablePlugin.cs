using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using StructureMap;

namespace Roadkill.Core.Plugins
{
	/// <summary>
	/// Work in progress
	/// </summary>
	public abstract class CustomVariablePlugin
	{
		public static readonly string PARSER_IGNORE_STARTTOKEN = "{{{roadkillinternal";
		public static readonly string PARSER_IGNORE_ENDTOKEN = "roadkillinternal}}}";

		/// <summary>
		/// The unique ID for the plugin, which is also the directory it's stored in inside the /Plugins/ directory.
		/// </summary>
		public abstract string Id { get; }
		public abstract string Name { get; }
		public abstract string Description { get; }
		public ApplicationSettings ApplicationSettings { get; set; }
		public SiteSettings SiteSettings { get; set; }
		protected List<PluginSetting> Settings { get; set; }

		/// <summary>
		/// The virtual path for the plugin, e.g. ~/Plugins/MyPlugin/. Contains a trailing slash.
		/// </summary>
		protected string PluginVirtualPath
		{
			get
			{
				return "~/Plugins/" + Id;
			}
		}

		public CustomVariablePlugin()
		{
			// TODO: setter injection
			ApplicationSettings = ObjectFactory.GetInstance<ApplicationSettings>();
		}

		public virtual string GetHeadContent(UrlHelper urlHelper)
		{
			return "";
		}

		public virtual string GetFooterContent(UrlHelper urlHelper)
		{
			return "";
		}

		/// <summary>
		/// Gets the HTML for a javascript link for the plugin, assuming the javascript is stored in the /Plugins/pluginID/javascript/ folder.
		/// </summary>
		public string GetScriptLink(UrlHelper urlHelper, string filename)
		{
			// Two tab stops to match HeadContent.cshtml
			string jsScript = "\t\t<script src=\"{0}/javascript/{1}\" type=\"text/javascript\"></script>\n";
			string html = string.Format(jsScript, urlHelper.Content(PluginVirtualPath), filename);

			return html;
		}

		/// <summary>
		/// Gets the HTML for a CSS link for the plugin, assuming the CSS is stored in the /Plugins/pluginID/css/ folder.
		/// </summary>
		public string GetCssLink(UrlHelper urlHelper, string filename)
		{
			string cssLink = "\t\t<link href=\"{0}/css/{1}\" rel=\"stylesheet\" type=\"text/css\" />\n";
			string html = string.Format(cssLink, urlHelper.Content(PluginVirtualPath), filename);

			return html;
		}

		public virtual string BeforeParse(string markupText)
		{
			return markupText;
		}

		public virtual string AfterParse(string html)
		{
			html = RemoveParserIgnoreTokens(html);

			// Undo the HTML sanitizer's attribute cleaning.
			html = html.Replace("<pre class=\"brush&#x3A;&#x20;c&#x23;", "<pre class=\"brush: c#");
			html = html.Replace("<pre class=\"brush&#x3A;&#x20;", "<pre class=\"brush: ");
			return html;
		}

		public virtual string RemoveParserIgnoreTokens(string html)
		{
			html = html.Replace(PARSER_IGNORE_STARTTOKEN, "");
			html = html.Replace(PARSER_IGNORE_ENDTOKEN, "");

			return html;
		}

		public static string AddParserIgnoreTokens(string replacementRegex)
		{
			// The new lines are important for the current Creole parser to recognise the ignore token.
			return "\n" + PARSER_IGNORE_STARTTOKEN + " \n" + replacementRegex + "\n" + PARSER_IGNORE_ENDTOKEN + "\n";
		}
	}
}
