using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters;
using System.Web.Security;

namespace Roadkill.Core
{
	public static class Extensions
	{
		public static string WikiMarkupToHtml(this string markup)
		{
			MarkupConverter converter = new MarkupConverter();
			return converter.ToHtml(markup);
		}

		public static string ForUrl(this string title)
		{
			if (string.IsNullOrEmpty(title))
				return "";

			return title.ToLower().Replace(" ", "-");
		}

		public static string AsValidFilename(this string title)
		{
			if (string.IsNullOrEmpty(title))
				return "";

			// This needs to be a lot more complete
			return title.Replace(" ", "-");
		}

		public static string ToBase64(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return "";

			return Convert.ToBase64String(Encoding.Default.GetBytes(text));
		}

		public static string FromBase64(this string base64Text)
		{
			if (string.IsNullOrEmpty(base64Text))
				return "";
			else
				return Encoding.Default.GetString(Convert.FromBase64String(base64Text));
		}

		public static string CleanTags(this string tags)
		{
			if (!string.IsNullOrEmpty(tags))
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
		/// <param name="tags"></param>
		/// <returns></returns>
		public static string SpaceDelimitTags(this string tags)
		{
			if (string.IsNullOrEmpty(tags))
				return "";

			string result = "";

			if (!string.IsNullOrWhiteSpace(tags))
			{
				string[] parts = tags.Split(';');
				result = string.Join(" ", parts);
			}

			return result;
		}

		public static string HashForPassword(this string password)
		{
			return FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");
		}
	}
}
