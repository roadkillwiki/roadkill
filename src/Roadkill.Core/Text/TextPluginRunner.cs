using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Text
{
	/// <summary>
	/// Runs the BeforeParse and AfterParse methods on all TextPlugins, and determines if 
	/// the the HTML can be cached or not based on the plugins run.
	/// </summary>
	internal class TextPluginRunner
	{
		private IEnumerable<TextPlugin> _plugins;
		private IPluginFactory _pluginFactory;
		public bool IsCacheable { get; set; }

		public TextPluginRunner(IPluginFactory pluginFactory)
		{
			if (pluginFactory == null)
				throw new ArgumentNullException("pluginFactory");

			_pluginFactory = pluginFactory;
			_plugins = GetPlugins();
		}

		private IEnumerable<TextPlugin> GetPlugins()
		{
			IEnumerable<TextPlugin> plugins = new List<TextPlugin>();
			try
			{
				plugins = _pluginFactory.GetEnabledTextPlugins();
			}
			catch (Exception e)
			{
				plugins = new List<TextPlugin>();
				Log.Error(e, "An exception occurred with getting the text plugins from the plugin factory.");
			}

			return plugins;
		}

		public string BeforeParse(string text, PageHtml pageHtml)
		{
			bool isCacheable = true;

			foreach (TextPlugin plugin in _plugins)
			{
				try
				{
					string previousText = text;
					text = plugin.BeforeParse(text);

					if (previousText != text)
					{
						// Determine if the plugin thinks the page is still cacheable (provided the plugin has changed the HTML).
						// Cacheable is true by default, so make sure if one plugin marks it as false the false value is kept.
						// TODO: if there are performance issues here, the plugin should report if it ran a transformation or not.
						if (isCacheable == true)
						{
							isCacheable = plugin.IsCacheable;
						}
					}

					pageHtml.HeadHtml += plugin.GetHeadContent();
					pageHtml.FooterHtml += plugin.GetFooterContent();
				}
				catch (Exception e)
				{
					Log.Error(e, "An exception occurred with the plugin {0} when calling BeforeParse()", plugin.Id);
				}
			}

			IsCacheable = isCacheable;
			return text;
		}

		public string AfterParse(string html)
		{
			bool isCacheable = true;

			foreach (TextPlugin plugin in _plugins)
			{
				try
				{
					string previousHtml = html;
					html = plugin.AfterParse(html);

					if (html != previousHtml && isCacheable == true)
						isCacheable = plugin.IsCacheable;
				}
				catch (Exception e)
				{
					Log.Error(e, "An exception occurred with the plugin {0} when calling AfterParse()", plugin.Id);
				}
			}

			IsCacheable = isCacheable;
			return html;
		}

		public string PreContainerHtml()
		{
			StringBuilder htmlBuilder = new StringBuilder();

			foreach (TextPlugin plugin in _plugins)
			{
				try
				{
					htmlBuilder.Append(plugin.GetPreContainerHtml());
				}
				catch (Exception e)
				{
					Log.Error(e, "An exception occurred with the plugin {0} when calling GetPreContainerHtml()", plugin.Id);
				}
			}

			return htmlBuilder.ToString();
		}

		public string PostContainerHtml()
		{
			StringBuilder htmlBuilder = new StringBuilder();

			foreach (TextPlugin plugin in _plugins)
			{
				try
				{
					htmlBuilder.Append(plugin.GetPostContainerHtml());
				}
				catch (Exception e)
				{
					Log.Error(e, "An exception occurred with the plugin {0} when calling GetPostContainerHtml()", plugin.Id);
				}
			}

			return htmlBuilder.ToString();
		}
	}
}
