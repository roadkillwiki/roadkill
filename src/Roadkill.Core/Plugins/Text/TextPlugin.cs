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
	/// Represents a plugin for supporting new wiki markup tokens that can be added to the page.
	/// </summary>
	public abstract class TextPlugin
	{
		/// <summary>
		/// The token injected before the token, to ensure the current markup parser doesn't parse the token.
		/// This is used in the <see cref="ParserSafeToken"/> method.
		/// </summary>
		public static readonly string PARSER_IGNORE_STARTTOKEN = "{{{roadkillinternal";

		/// <summary>
		/// The token injected after the token, to ensure the current markup parser doesn't parse the token.
		/// This is used in the <see cref="ParserSafeToken"/> method.
		/// </summary>
		public static readonly string PARSER_IGNORE_ENDTOKEN = "roadkillinternal}}}";

		private List<string> _scriptFiles;
		private string _onLoadFunction;
		private Guid _databaseId;
		private string _pluginVirtualPath;

		/// <summary>
		/// The backing <see cref="Settings"/> instance for the <see cref="TextPlugin.Settings"/> property.
		/// </summary>
		protected internal Settings _settings;

		/// <summary>
		/// The unique ID for the plugin, which is also the directory it's stored in inside the /Plugins/ directory.
		/// This should not be case sensitive.
		/// </summary>
		public abstract string Id { get; }

		/// <summary>
		/// The name of the plugin.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// A short description of the plugin, including example usage. Any HTML in the description is encoded.
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// The current plugin version, using the standard .NET version format.
		/// </summary>
		public abstract string Version { get; }
		
		/// <summary>
		/// Gets or sets the current Roadkill <see cref="ApplicationSettings"/>. This property is automatically filled by Roadkill 
		/// when the plugin is loaded.
		/// </summary>
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		// These are setter injected at creation time by the DI manager
		internal IPluginCache PluginCache { get; set; }
		internal IRepository Repository { get; set; }

		/// <summary>
		/// Gets or sets whether the plugin's HTML output can be cached by the inbuilt Roadkill caching.
		/// </summary>
		public virtual bool IsCacheable { get; set; }

		/// <summary>
		/// Gets the plugin's <see cref="Settings"/>. If cached, the settings are loaded from there, otherwise they 
		/// are loaded from the database. If no settings exist in the database for the plugin, a new <see cref="Settings"/> 
		/// instance is created, saved to the database and returned.
		/// </summary>
		public Settings Settings
		{
			get
			{
				EnsureSettings();
				return _settings;
			}
		}

		/// <summary>
		///  Gets the current Roadkill <see cref="SiteSettings"/> from the repository.
		/// </summary>
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
		/// Gets the virtual path for the plugin. This is in the format ~/Plugins/MyPlugin and does not contain a trailing slash.
		/// </summary>
		public string PluginVirtualPath
		{
			get
			{
				if (_pluginVirtualPath == null)
				{
					EnsureIdIsValid();
					_pluginVirtualPath = "~/Plugins/" + Id;
				}

				return _pluginVirtualPath;
			}
		}

		/// <summary>
		/// Gets the unique id used for storing the plugin settings in the database. This is a Guid generated based on the 
		/// plugin ID and version number.
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

		/// <summary>
		/// Initializes a new instance of the <see cref="TextPlugin"/> class.
		/// </summary>
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

		/// <summary>
		/// Called when settings are first initialized by the <see cref="Settings"/> property. When overriden this 
		/// method allows you add settings and their defaults that the plugin requires.
		/// </summary>
		/// <param name="settings">The plugin settings.</param>
		public virtual void OnInitializeSettings(Settings settings)
		{
		}

		/// <summary>
		/// Called before the page's markdown is parsed by the current <see cref="IMarkupParser"/>
		/// </summary>
		/// <param name="markupText">The page's markup text.</param>
		/// <returns>When overriden, this method should search the markup for the plugin's token, and replace it the parsed value. 
		/// The plugin's token should also be replaced using <see cref="ParserSafeToken"/> to ensure the current <see cref="IMarkupParser"/> doesn't try
		/// to parse any markdown inside it.</returns>
		public virtual string BeforeParse(string markupText)
		{
			return markupText;
		}

		/// <summary>
		/// Called after the page's markdown has been parsed by the current <see cref="IMarkupParser"/>.
		/// </summary>
		/// <param name="html">The page's HTML.</param>
		/// <returns>When overriden, this method can add any extra HTML to the page's content. This method shouldn't be used for adding 
		/// scripts or CSS unless you explicitly need to, use the <see cref="GetHeadContent"/> and related methods for this. 
		/// NOTE: the HTML you return does not have its content stripped of any unsafe HTML (such as script tags) so you are responsible for
		/// ensuring your HTML has no security exploits such as XSS attacks.</returns>
		public virtual string AfterParse(string html)
		{
			return html;
		}

		/// <summary>
		/// Gets plugin's current settings as a JSON string. This is a shortcut for <see cref="Settings.GetJson()"/>
		/// </summary>
		/// <returns></returns>
		public string GetSettingsJson()
		{
			return this.Settings.GetJson();
		}

		/// <summary>
		/// Removes the tokens that were added by the <see cref="ParserSafeToken"/> method.
		/// </summary>
		/// <param name="html">The page's HTML</param>
		/// <returns>The HTML with the tokens removed that were used to ensure the markdown parser didn't parse the token itself.</returns>
		public virtual string RemoveParserIgnoreTokens(string html)
		{
			html = html.Replace(PARSER_IGNORE_STARTTOKEN, "");
			html = html.Replace(PARSER_IGNORE_ENDTOKEN, "");

			return html;
		}

		/// <summary>
		/// When overriden by implementing plugins, gets any HTML that should be added to the current page's HTML head section.
		/// </summary>
		/// <returns>Additional head HTML for the page.</returns>
		public virtual string GetHeadContent()
		{
			return "";
		}

		/// <summary>
		/// When overriden by implementing plugins, gets any HTML that should be added to the current page's footer, before the closing body tag.
		/// </summary>
		/// <returns>Additional footer HTML for the page.</returns>
		public virtual string GetFooterContent()
		{
			return "";
		}

		/// <summary>
		/// When overriden by implementing plugins, gets any HTML that should be added to the current page just before the container 
		/// (&lt;div id="container&gt;) tag.
		/// </summary>
		/// <returns>Additional HTML.</returns>
		public virtual string GetPreContainerHtml()
		{
			return "";
		}

		/// <summary>
		/// When overriden by implementing plugins, gets any HTML that should be added to the current page just after the container 
		/// (&lt;div id="container&gt;) tag.
		/// </summary>
		/// <returns>Additional HTML.</returns>
		public virtual string GetPostContainerHtml()
		{
			return "";
		}

		/// <summary>
		/// Gets the HTML for a javascript link required by the plugin, assuming the javascript is stored in the /Plugins/ID/ folder.
		/// This will return all Javascript files that are to be added to the page that the plugin adds using the <see cref="AddScript"/> method.
		/// This method uses Head.JS to load the script so any page loading speed is not effected by the script request.
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

		/// <summary>
		/// Sets the Head.JS OnLoaded function for the plugin, which allows the plugin to make additional Javascript calls once any 
		/// Javascript script files have been loaded.
		/// </summary>
		/// <param name="functionBody">The Javascript function body.</param>
		public void SetHeadJsOnLoadedFunction(string functionBody)
		{
			_onLoadFunction = functionBody;
		}

		/// <summary>
		/// Adds a Javascript file to the page, assuming the script file is stored in the {siteroot}/Plugins/ID/ folder.
		/// </summary>
		/// <param name="filename">The filename of the Javascript file. If this contains a path, it should be relative to the plugin directory, 
		/// for example 'scripts/morescripts/myfile.js'.</param>
		/// <param name="name">A unique name for the script file (used by Head.JS). For example this could be 'angular' for AngularJs.</param>
		/// <param name="useHeadJs">if set to <c>true</c> then the script will be loaded using Head.JS. Currently only Head.JS is supported.</param>
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
		/// Gets the HTML for a CSS link for the plugin, assuming the CSS is stored in the {siteroot}/Plugins/ID/ folder.
		/// </summary>
		public string GetCssLink(string filename)
		{
			string cssLink = "\t\t<link href=\"{0}/{1}?version={2}\" rel=\"stylesheet\" type=\"text/css\" />\n";
			string html = "";

			if (HttpContext.Current != null)
			{
				UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				html = string.Format(cssLink, urlHelper.Content(PluginVirtualPath), filename, Version);
			}
			else
			{
				html = string.Format(cssLink, PluginVirtualPath, filename, Version);
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