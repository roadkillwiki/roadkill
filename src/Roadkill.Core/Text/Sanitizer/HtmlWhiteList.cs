using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Text.Sanitizer
{
	public class HtmlWhiteList
	{
		public List<HtmlElement> ElementWhiteList { get; set; }

		public static HtmlWhiteList Deserialize(ApplicationSettings settings)
		{
			if (string.IsNullOrEmpty(settings.HtmlElementWhiteListPath) || !File.Exists(settings.HtmlElementWhiteListPath))
			{
				if (!string.IsNullOrEmpty(settings.HtmlElementWhiteListPath))
					Log.Warn("The custom HTML white list tokens file does not exist in path '{0}' - using Default white list.", settings.HtmlElementWhiteListPath);
				
				return CreateDefaultWhiteList();
			}

			try
			{
				using (FileStream stream = new FileStream(settings.HtmlElementWhiteListPath, FileMode.Open, FileAccess.Read))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(HtmlWhiteList));
					HtmlWhiteList whiteList = (HtmlWhiteList)serializer.Deserialize(stream);

					if (whiteList == null)
						return CreateDefaultWhiteList();
					else
						return whiteList;
				}
			}
			catch (IOException e)
			{
				Log.Warn(e, "An IO error occurred loading the html element white list file {0}", settings.HtmlElementWhiteListPath);
				return CreateDefaultWhiteList();
			}
			catch (FormatException e)
			{
				Log.Warn(e, "A FormatException error occurred loading the html element white list file {0}", settings.HtmlElementWhiteListPath);
				return CreateDefaultWhiteList();
			}
			catch (Exception e)
			{
				Log.Warn(e, "An unhandled exception error occurred loading the html element white list file {0}", settings.HtmlElementWhiteListPath);
				return CreateDefaultWhiteList();
			}
		}

		private static HtmlWhiteList CreateDefaultWhiteList()
		{
			List<HtmlElement> tagList = new List<HtmlElement>();

			tagList.Add(new HtmlElement("strong", new string[] { "style", }));
			tagList.Add(new HtmlElement("b", new string[] { "style" }));
			tagList.Add(new HtmlElement("em", new string[] { "style" }));
			tagList.Add(new HtmlElement("i", new string[] { "style" }));
			tagList.Add(new HtmlElement("u", new string[] { "style" }));
			tagList.Add(new HtmlElement("strike", new string[] { "style" }));
			tagList.Add(new HtmlElement("sub", new string[] { }));
			tagList.Add(new HtmlElement("sup", new string[] { }));
			tagList.Add(new HtmlElement("p", new string[] { "style", "align", "dir" }));
			tagList.Add(new HtmlElement("ol", new string[] { }));
			tagList.Add(new HtmlElement("li", new string[] { }));
			tagList.Add(new HtmlElement("ul", new string[] { }));
			tagList.Add(new HtmlElement("font", new string[] { "style", "color", "face", "size" }));
			tagList.Add(new HtmlElement("blockquote", new string[] { "style", "dir" }));
			tagList.Add(new HtmlElement("hr", new string[] { "size", "width" }));
			tagList.Add(new HtmlElement("img", new string[] { "src" }));
			tagList.Add(new HtmlElement("div", new string[] { "style", "align", "class" }));
			tagList.Add(new HtmlElement("span", new string[] { "style", "class" }));
			tagList.Add(new HtmlElement("br", new string[] { "style" }));
			tagList.Add(new HtmlElement("center", new string[] { "style" }));
			tagList.Add(new HtmlElement("a", new string[] { "href" }));
			tagList.Add(new HtmlElement("pre", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("code", new string[] { "id", "class" }));

			tagList.Add(new HtmlElement("h1", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("h2", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("h3", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("h4", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("h5", new string[] { "id", "class" }));

			tagList.Add(new HtmlElement("table", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("thead", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("th", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("tbody", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("tr", new string[] { "id", "class" }));
			tagList.Add(new HtmlElement("td", new string[] { "id", "class" }));

			return new HtmlWhiteList() { ElementWhiteList = tagList };
		}
	}
}
