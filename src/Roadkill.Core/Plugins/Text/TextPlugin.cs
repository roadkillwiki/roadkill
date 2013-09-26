using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using StructureMap;

namespace Roadkill.Core.Plugins
{
	/// <summary>
	/// Work in progress
	/// </summary>
	public abstract class TextPlugin
	{
		public static readonly string PARSER_IGNORE_STARTTOKEN = "{{{roadkillinternal";
		public static readonly string PARSER_IGNORE_ENDTOKEN = "roadkillinternal}}}";

		private List<string> _scriptFiles;
		private string _onLoadFunction;
		private Guid _objectId;
		public Settings Settings { get; set; }

		/// <summary>
		/// The unique ID for the plugin, which is also the directory it's stored in inside the /Plugins/ directory.
		/// </summary>
		public abstract string Id { get; }
		public abstract string Name { get; }
		public abstract string Description { get; }
		public abstract string Version { get; }

		public ApplicationSettings ApplicationSettings { get; set; }
		public SiteSettings SiteSettings { get; set; }
		public virtual bool IsCacheable { get; set; }

		/// <summary>
		/// The virtual path for the plugin, e.g. ~/Plugins/Text/MyPlugin/. Does not contain a trailing slash.
		/// </summary>
		protected string PluginVirtualPath
		{
			get
			{
				return "~/Plugins/Text/" +Id;
			}
		}

		public Guid DatabaseId
		{
			get
			{
				// Generate an ID for use in the database, that's tied to this object,
				// in other words not globally unique, but it doesn't matter.
				if (_objectId == Guid.Empty)
				{
					int firstPart = Id.GetHashCode();

					// Next 2 sequence of numbers
					int hashCode = this.GetHashCode();
					short shortHashCode = (short)hashCode;

					// Final 8 numbers
					byte[] shortBytes = BitConverter.GetBytes(hashCode);
					byte[] lastPart = new byte[8];
					lastPart = shortBytes.Concat(shortBytes).ToArray();

					_objectId = new Guid(firstPart, shortHashCode, shortHashCode, lastPart);
				}

				return _objectId;
			}
		}

		public TextPlugin(ApplicationSettings applicationSettings, IRepository repository)
		{
			_scriptFiles = new List<string>();

			ApplicationSettings = applicationSettings;
			IsCacheable = true;
			Settings = new Settings();

			if (repository != null)
				SiteSettings = repository.GetSiteSettings();
		}

		public virtual string BeforeParse(string markupText)
		{
			return markupText;
		}

		public virtual string AfterParse(string html)
		{
			return html;
		}

		public virtual void SaveSettings(string filePath = "")
		{
			if (string.IsNullOrEmpty(filePath))
			{
				if (HttpContext.Current != null)
				{
					filePath = HttpContext.Current.Server.MapPath(PluginVirtualPath);
				}
				else
				{
					throw new ArgumentNullException(filePath);
				}
			}

			string json = GetSettingsJson();
			File.WriteAllText(filePath, json);
		}

		public string GetSettingsJson()
		{
			return JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
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
		/// Gets the HTML for a javascript link for the plugin, assuming the javascript is stored in the /Plugins/ID/ folder.
		/// </summary>
		public string GetScriptHtmlWithHeadJS()
		{
			string headScript = "<script type=\"text/javascript\">";
			headScript += "head.js(";
			headScript += string.Join(",\n", _scriptFiles);
			headScript += ",function() { " +_onLoadFunction+ " })";
			headScript += "</script>\n";

			return headScript;
		}

		public void AddOnLoadedFunction(string functionBody)
		{
			_onLoadFunction = functionBody;
		}

		public void AddScriptWithHeadJS(string filename, string name = "")
		{
			string fileLink = "{ \"[name]\", \"[filename]\" }";
			if (string.IsNullOrEmpty(name))
			{
				fileLink = "\"[filename]\"";
			}

			if (HttpContext.Current != null)
			{
				UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				filename = string.Concat(urlHelper.Content(PluginVirtualPath), "/", filename);
			}

			fileLink = fileLink.Replace("[name]", name);
			fileLink = fileLink.Replace("[filename]", filename);
			_scriptFiles.Add(fileLink);
		}

		/// <summary>
		/// Gets the HTML for a CSS link for the plugin, assuming the CSS is stored in the /Plugins/ID/ folder.
		/// </summary>
		public string GetCssLink(string filename)
		{
			string cssLink = "\t\t<link href=\"{0}/{1}\" rel=\"stylesheet\" type=\"text/css\" />\n";
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
