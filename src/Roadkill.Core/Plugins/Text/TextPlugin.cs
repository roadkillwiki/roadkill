using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DI;
using Roadkill.Core.Logging;
using StructureMap;
using StructureMap.Attributes;

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
		private Guid _databaseId;
		private string _pluginVirtualPath;
		protected internal Settings _settings;

		/// <summary>
		/// The unique ID for the plugin, which is also the directory it's stored in inside the /Plugins/ directory.
		/// This should not be case sensitive.
		/// </summary>
		public abstract string Id { get; }
		public abstract string Name { get; }
		public abstract string Description { get; }
		public abstract string Version { get; }
		
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		// Setter injected at creation time by the DI manager
		internal IPluginCache PluginCache { get; set; }
		internal IRepository Repository { get; set; }

		public virtual bool IsCacheable { get; set; }
		public virtual bool IsEnabled { get; set; }

		public Settings Settings
		{
			get
			{
				EnsureSettings();
				return _settings;
			}
		}

		public SiteSettings SiteSettings
		{
			get
			{
				if (Repository != null)
					return Repository.GetSiteSettings();
				else
					return null;
			}
		}

		/// <summary>
		/// The virtual path for the plugin, e.g. ~/Plugins/Text/MyPlugin/. Does not contain a trailing slash.
		/// </summary>
		public string PluginVirtualPath
		{
			get
			{
				if (_pluginVirtualPath == null)
				{
					EnsureIdIsValid();
					_pluginVirtualPath = "~/Plugins/Text/" + Id;
				}

				return _pluginVirtualPath;
			}
		}

		/// <summary>
		/// Used as the PK in the site_configuration table to store the plugin settings.
		/// </summary>
		public Guid DatabaseId
		{
			get
			{
				// Generate an ID for use in the database in the format:
				// {aaaaaaaa-bbbb-0000-0000-000000000000}
				// Where 
				//		a = hashcode of the plugin id
				//		b = hashcode of version number 
				// 
				// It's not globally unique, but it doesn't matter as it's 
				// being used for the site_configuration database table only. The only 
				// way the Guid could clash is if two plugins have the same ID.
				// This should never happen, as the IDs will be like nuget ids.
				//
				if (_databaseId == Guid.Empty)
				{
					EnsureIdIsValid();
					string version = EnsureValidVersion();

					int firstPart = Id.GetHashCode();
					short versionNum = (short)version.GetHashCode();

					short zero = (short)0;
					byte[] lastChunk = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

					_databaseId = new Guid(firstPart, versionNum, zero, lastChunk);
				}

				return _databaseId;
			}
		}

		public TextPlugin()
		{
			_scriptFiles = new List<string>();
			IsCacheable = true;
		}

		internal TextPlugin(IRepository repository, SiteCache siteCache) : this()
		{
			Repository = repository;
			PluginCache = siteCache;
		}

		private void EnsureSettings()
		{
			if (_settings == null)
			{
				// Guard for null SiteCache
				if (PluginCache == null)
				{
					throw new PluginException(null, "The PluginCache property is null for {0} when it should be injected by the DI container. " +
											  "If you're unit testing, set the PluginCache and Repository properties with stubs before calling the Settings properties.", GetType().FullName);
				}

				_settings = PluginCache.GetPluginSettings(this);
				if (_settings == null)
				{
					// Guard for null Repository
					if (Repository == null)
					{
						throw new PluginException(null, "The Repository property is null for {0} and it wasn't found in the cache - it should be injected by the DI container. " +
											  "If you're unit testing, set the PluginCache and Repository properties with stubs before calling the Settings properties.", GetType().FullName);
					}

					// Load from the database
					_settings = Repository.GetTextPluginSettings(this.DatabaseId);

					// If this is the first time the plugin has been used, new up the settings
					if (_settings == null)
					{
						EnsureIdIsValid();
						string version = EnsureValidVersion();
						_settings = new Settings(Id, version);

						// Allow derived classes to add custom setting values
						OnInitializeSettings(_settings);

						// Update the repository
						Repository.SaveTextPluginSettings(this);
					}

					// Cache the settings
					PluginCache.UpdatePluginSettings(this);
				}
			}
		}
		
		public virtual void OnInitializeSettings(Settings settings)
		{
		}

		public virtual string BeforeParse(string markupText)
		{
			return markupText;
		}

		public virtual string AfterParse(string html)
		{
			return html;
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
		public string GetJavascriptHtml()
		{
			string headScript = "<script type=\"text/javascript\">";
			headScript += "head.js(";
			headScript += string.Join(",\n", _scriptFiles);
			headScript += ",function() { " +_onLoadFunction+ " })";
			headScript += "</script>\n";

			return headScript;
		}

		public void SetHeadJsOnLoadedFunction(string functionBody)
		{
			_onLoadFunction = functionBody;
		}

		public void AddScript(string filename, string name = "", bool useHeadJs = true)
		{
			if (useHeadJs)
			{
				string fileLink = "{ \"[name]\", \"[filename]\" }";
				if (string.IsNullOrEmpty(name))
				{
					fileLink = "\"[filename]\"";
				}

				// Get the server path
				if (HttpContext.Current != null)
				{
					UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
					filename = string.Concat(urlHelper.Content(PluginVirtualPath), "/", filename);
				}

				fileLink = fileLink.Replace("[name]", name);
				fileLink = fileLink.Replace("[filename]", filename);

				_scriptFiles.Add(fileLink);
			}
			else
			{
				Log.Error("Only Head JS is currently supported for plugin Javascript links");
			}
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

		private void EnsureIdIsValid()
		{
			if (string.IsNullOrEmpty(Id))
				throw new PluginException(null, "The ID is empty or null for plugin {0}. Please remove this plugin from the bin and plugins folder.", this.GetType().Name);
		}

		private string EnsureValidVersion()
		{
			if (string.IsNullOrWhiteSpace(Version))
				return "1.0";
			else
				return Version;
		}
	}
}