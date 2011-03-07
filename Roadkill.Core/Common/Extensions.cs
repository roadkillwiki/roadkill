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
			MarkupConverter converter = new MarkupConverter();
			return converter.ToHtml(markup);
		}

		public static string AsValidPath(this string title)
		{
			// This needs to be a lot more complete
			return title.Replace(" ", "-");
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

		/// <summary>
		/// Takes a string of tags: "tagone;tagtwo;tag3;" and returns "tagone tagtwo tag3"
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static string SpaceDelimitTags(this string content)
		{
			string result = "";

			if (!string.IsNullOrWhiteSpace(content))
			{
				string[] parts = content.Split(';');
				result = string.Join(" ", parts);
			}

			return result;
		}
	}
}
