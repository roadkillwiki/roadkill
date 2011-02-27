using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	public static class Extensions
	{
		public static string CleanTags(this string tags)
		{
			if (!string.IsNullOrWhiteSpace(tags))
			{
				return tags.Replace(" ", ";") + ";";
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
