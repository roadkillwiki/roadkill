using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Converters
{
	public class ConverterFactory
	{
		private static MarkupConverter _converter;

		public static MarkupConverter Converter
		{
			get
			{
				if (_converter == null)
				{
					switch (RoadkillSettings.MarkupType.ToLower())
					{
						case "markdown":
							_converter = new MarkdownConverter();
							break;

						case "mediawiki":
							_converter = new MediaWikiConverter();
							break;

						case "creole":
						default:
							_converter = new CreoleConverter();
							break;
					}
				}

				return _converter;
			}
		}
	}
}
