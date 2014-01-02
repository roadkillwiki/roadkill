using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters;
using System.Web.Security;
using System.IO;
using System.Web.Mvc;
using System.Web;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Extensions
{
	/// <summary>
	/// A set of common extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Converts the text provided to Base64 format, for use in views.
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
		/// Appends text with the number of tabs before the text.
		/// </summary>
		public static StringBuilder Append(this StringBuilder builder, string value, int tabCount)
		{
			string tabs = "";
			for (int i = 0; i < tabCount; i++)
			{
				tabs += "\t";
			}

			builder.Append(tabs + value);
			return builder;
		}

		/// <summary>
		/// Appends a line with the number of tabs before the text.
		/// </summary>
		public static StringBuilder AppendLine(this StringBuilder builder, string value, int tabCount)
		{
			string tabs = "";
			for (int i = 0; i < tabCount; i++)
			{
				tabs += "\t";
			}

			builder.AppendLine(tabs + value);
			return builder;
		}

		/// <summary>
		/// Sets the milliseconds part of a datetime to zero.
		/// </summary>
		public static DateTime ClearMilliseconds(this DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, 0);
		}
	}
}
