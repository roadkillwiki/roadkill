using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
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

		protected List<Setting> Settings { get; set; }

		/// <summary>
		/// The unique ID for the plugin, which is also the directory it's stored in inside the /Plugins/ directory.
		/// </summary>
		public abstract string Id { get; }
		public abstract string Name { get; }
		public abstract string Description { get; }

		public ApplicationSettings ApplicationSettings { get; set; }
		public SiteSettings SiteSettings { get; set; }
		public virtual bool IsCacheable { get; set; }

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

		public CustomVariablePlugin(ApplicationSettings applicationSettings, IRepository repository)
		{
			ApplicationSettings = applicationSettings;
			SiteSettings = repository.GetSiteSettings();
			IsCacheable = true;
			Settings = new List<Setting>();
		}

		public virtual string BeforeParse(string markupText)
		{
			return markupText;
		}

		public virtual string AfterParse(string html)
		{
			return html;
		}

		public virtual void SaveSettings(IEnumerable<Setting> settings)
		{

		}

		public virtual string RemoveParserIgnoreTokens(string html)
		{
			html = html.Replace(PARSER_IGNORE_STARTTOKEN, "");
			html = html.Replace(PARSER_IGNORE_ENDTOKEN, "");

			return html;
		}

		public virtual string GetHeadContent()
		{
			return "";
		}

		public virtual string GetFooterContent()
		{
			return "";
		}

		/// <summary>
		/// Gets the HTML for a javascript link for the plugin, assuming the javascript is stored in the /Plugins/pluginID/javascript/ folder.
		/// </summary>
		public string GetScriptLink(string filename)
		{
			// Two tab stops to match HeadContent.cshtml
			string jsScript = "\t\t<script src=\"{0}/javascript/{1}\" type=\"text/javascript\"></script>\n";
			string html = "";

			if (HttpContext.Current != null)
			{
				UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				html = string.Format(jsScript, urlHelper.Content(PluginVirtualPath), filename);
			}
			else
			{
				html = string.Format(jsScript, PluginVirtualPath, filename);
			}

			return html;
		}

		/// <summary>
		/// Gets the HTML for a CSS link for the plugin, assuming the CSS is stored in the /Plugins/pluginID/css/ folder.
		/// </summary>
		public string GetCssLink(string filename)
		{
			string cssLink = "\t\t<link href=\"{0}/css/{1}\" rel=\"stylesheet\" type=\"text/css\" />\n";
			string html = "";

			if (HttpContext.Current != null)
			{
				UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				html = string.Format(cssLink, urlHelper.Content(PluginVirtualPath), filename);
			}
			else
			{
				html = string.Format(cssLink, PluginVirtualPath, filename);
			}

			return html;
		}

		/// <summary>
		/// Adds a token to the start and end of the the provided token, so it'll be ignored 
		/// by the parser. This is necessary for tokens such as [[ and {{ which the parser will 
		/// try to parse.
		/// </summary>
		/// <param name="token">The token the plugin uses. This can be a regex.</param>
		public static string ParserSafeToken(string token)
		{
			// The new lines are important for the current Creole parser to recognise the ignore token.
			return "\n" + PARSER_IGNORE_STARTTOKEN + " \n" + token + "\n" + PARSER_IGNORE_ENDTOKEN + "\n";
		}
	}
}
