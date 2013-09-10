using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Plugins;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Runtime.Caching;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace TestPlugin
{
    public class RssPlugin : CustomVariablePlugin
    {
		private static readonly string _token = "[[[rss]]]";
		private static readonly string _parserSafeToken;

		public override string Id
		{
			get { return "RssPlugin"; }
		}

		public override string Name
		{
			get { return "An RSS Plugin"; }
		}

		public override string Description
		{
			get { return "Gets the commit log from Roadkill's Bitbucket RSS feed."; }
		}

		public override bool IsCacheable
		{
			get
			{
				// This property is true by default in the base class. Setting it to false tells Roadkill
				// that any page that this plugin runs on should not be cached by the browser, or in memory on the server.
				return false;
			}
			set
			{
				
			}
		}

		static RssPlugin()
		{
			// This is a micro-optimization: this parser safe token should only be created once.
			_parserSafeToken = ParserSafeToken(_token);
		}

		/// <summary>
		/// This constructor is required as the two parameters are filled in at runtime. You can safely create 
		/// an instance of your plugin by passing null for both parameters, there are no side effects from this.
		/// </summary>
		public RssPlugin(ApplicationSettings applicationSettings, IRepository repository)
			: base(applicationSettings, repository)
		{
		}

		public override string BeforeParse(string markupText)
		{
			// This ensures that our token [[[rss]]] isn't parsed by the Creole parser.
			// The "_parserSafeToken" is something like '{{{roadkillinternal [[[rss]]] roadkillinternal}}}',
			// and gets turned back into the [[[rss]]] token after the parser has finished doing its thing.
			return markupText.Replace(_token, _parserSafeToken);
		}

		public override string AfterParse(string html)
		{
			// This check is important for performance: don't replace anything if the HTML source doesn't have the plugin's token.
			if (html.Contains(_token))
			{
				// Look in the cache first
				string cachedHtml = MemoryCache.Default.Get(Id) as string;
				if (!string.IsNullOrEmpty(cachedHtml))
					return cachedHtml;

				string rssHtml = GetRssHtml();
				html = html.Replace(_token, rssHtml);

				// Cache the HTML for 3 hours, although app pool recycles will trump this.
				CacheItemPolicy policy = new CacheItemPolicy();
				policy.AbsoluteExpiration = DateTime.UtcNow.AddHours(3);
				MemoryCache.Default.Add(Id, html, policy);

				return html;
			}
			else
			{
				return html;
			}
		}

		/// <summary>
		/// This generates a basic ordered list with all the RSS items from the feed.
		/// The HTML generated looks like this:
		/// 
		/// <![CDATA[
		/// <b>Day of items</b>
		/// <ul>
		///		<li><a href="alink">RSS Item title</a></li>
		///	</ul>
		/// ]]>
		/// 
		/// </summary>
		private string GetRssHtml()
		{
			XmlReader reader = XmlReader.Create("http://bitbucket.org/mrshrinkray/roadkill/rss");
			SyndicationFeed feed = SyndicationFeed.Load(reader);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<h4>Roadkill commit log</h4>");

			string itemFormat = "<li><a href=\"{0}\" target=\"_blank\">{1}</a></li>";
			string day = "";
			foreach (SyndicationItem item in feed.Items)
			{
				// Group by the day
				string itemDay = item.PublishDate.ToString("ddd dd MMM yyy");
				if (day != itemDay)
				{
					if (!string.IsNullOrEmpty(day))
						builder.AppendLine("</ul>");

					builder.AppendLine("<b>" + itemDay + "</b><br/>");
					builder.AppendLine("<ul>");
				}

				string text = string.Format(itemFormat, item.Id, item.Title.Text);
				builder.AppendLine(text);

				day = itemDay;
			}

			builder.AppendLine("</ul>");
			builder.Append("Last updated: " + DateTime.Now.ToString());

			return builder.ToString();
		}
	}
}
