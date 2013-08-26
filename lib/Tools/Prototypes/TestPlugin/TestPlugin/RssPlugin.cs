using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Plugins;
using System.ServiceModel.Syndication;
using System.Xml;

namespace TestPlugin
{
    public class RssPlugin : CustomVariablePlugin
    {
		private static string _token = "[[[rss]]]";
		private static string _parserSafeToken;

		public override string Description
		{
			get { return "RSS test"; }
		}

		public override string Id
		{
			get { return "rssid"; }
		}

		public override string Name
		{
			get { return "rss"; }
		}

		static RssPlugin()
		{
			_parserSafeToken = ParserSafeToken(_token);
		}

		public override string BeforeParse(string markupText)
		{
			return markupText.Replace(_token, _parserSafeToken);
		}

		public override string AfterParse(string html)
		{
			XmlReader reader = XmlReader.Create("http://bitbucket.org/mrshrinkray/roadkill/rss");
			SyndicationFeed feed = SyndicationFeed.Load(reader);
			string itemFormat = "<li><a href=\"{0}\" target=\"_blank\">{1}</a></li>";

			StringBuilder builder = new StringBuilder();
			string day = "";

			builder.AppendLine("<h4>Roadkill commit log</h4>");

			foreach (SyndicationItem item in feed.Items)
			{
				string itemDay = item.PublishDate.ToString("ddd dd MMM yyy");
				if (day != itemDay)
				{
					if (!string.IsNullOrEmpty(day))
						builder.AppendLine("</ul>");

					builder.AppendLine("<b>" +itemDay+"</b><br/>");
					builder.AppendLine("<ul>");
				}

				string text = string.Format(itemFormat, item.Id, item.Title.Text);
				builder.AppendLine(text);

				day = itemDay;
			}
			builder.AppendLine("</ul>");

			return html.Replace(_token, builder.ToString()); 
		}
	}
}
