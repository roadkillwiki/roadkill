using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	public static class Extensions
	{
		public static string WikiMarkupToHtml(this string markup)
		{
			return ConverterFactory.Converter.ToHtml(markup);
		}

		public static string CleanTags(this string tags)
		{
			if (!string.IsNullOrWhiteSpace(tags))
			{
				// Remove any tags that are just ";"
				string[] parts = tags.Split(';');
				List<string> results = new List<string>();
				foreach (string item in parts)
				{
					if (item != ";")
						results.Add(item);
				}

				tags = string.Join(";",results);
				tags += ";";

				return tags.Replace(" ", "-");
			}
			else
			{
				if (tags != null)
					return tags.TrimEnd();
				else
					return "";
			}
		}
	}
}
