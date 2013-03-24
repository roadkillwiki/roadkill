using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters;
using System.Web.Security;
using System.IO;
using System.Web.Mvc;
using System.Web;

namespace Roadkill.Core
{
	/// <summary>
	/// A set of common extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Removes all invalid characters from a title so it can be used as a filename to save to disk.
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public static string AsValidFilename(this string title)
		{
			if (string.IsNullOrEmpty(title))
				return "";

			// Simply replace invalid path characters with a '-'
			char[] invalidChars = Path.GetInvalidFileNameChars();
			foreach (char item in invalidChars)
			{
				title = title.Replace(item, '-');
			}

			return title;
		}

		/// <summary>
		/// Converts the text provided to Base64 format.
		/// </summary>
		/// <param name="text">Plain text</param>
		/// <returns>The string, Base64 encoded</returns>
		public static string ToBase64(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return "";

			return Convert.ToBase64String(Encoding.Default.GetBytes(text));
		}

		/// <summary>
		/// Attempts to convert the text provided from Base64 format to plain text.
		/// </summary>
		/// <param name="text">Base64 text text</param>
		/// <returns>The plain text</returns>
		public static string FromBase64(this string base64Text)
		{
			if (string.IsNullOrEmpty(base64Text))
				return "";
			else
				return Encoding.Default.GetString(Convert.FromBase64String(base64Text));
		}


		/// <summary>
		/// Takes a string of tags: "tagone,tagtwo,tag3 " and returns a list.
		/// </summary>
		/// <param name="tags"></param>
		/// <returns></returns>
		public static IEnumerable<string> ParseTags(this string tags)
		{
			List<string> tagList = new List<string>();
			char delimiter = ',';

			if (!string.IsNullOrEmpty(tags))
			{
				// For the legacy tag seperator format
				if (tags.IndexOf(";") != -1)
					delimiter = ';';

				if (tags.IndexOf(delimiter) != -1)
				{
					string[] parts = tags.Split(delimiter);
					foreach (string item in parts)
					{
						if (item != ",")
							tagList.Add(item);
					}
				}
				else
				{
					tagList.Add(tags.TrimEnd());
				}
			}

			return tagList;
		}

		/// <summary>
		/// Gets a 304 HTTP response if there is a "If-Modified-Since" header and it matches 
		/// the fileDate. Otherwise a 200 OK is given.
		/// </summary>
		/// <param name="context"></param>
		public static int GetStatusCodeForCache(this HttpContext context, DateTime fileDate)
		{
			if (context == null)
				return 200;

			int status = 200;
			if (context.Request.Headers["If-Modified-Since"] != null)
			{
				// When If-modified is sent (never when it's incognito mode), it matches the 
				// the write time you send back for the file. So 1st Jan 2001, it will send back
				// 1st Jan 2001 for If-Modified.
				status = 304;
				DateTime modifiedSinceDate = DateTime.UtcNow;
				if (DateTime.TryParse(context.Request.Headers["If-Modified-Since"], out modifiedSinceDate))
				{
					modifiedSinceDate = modifiedSinceDate.ToUniversalTime();

					DateTime lastWriteTime = new DateTime(fileDate.Year, fileDate.Month, fileDate.Day, fileDate.Hour, fileDate.Minute, fileDate.Second, 0, DateTimeKind.Utc);
					if (lastWriteTime != modifiedSinceDate)
						status = 200;
				}
			}

			return status;
		}
	}
}
