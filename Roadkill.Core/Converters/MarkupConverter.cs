using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Converters
{
	public abstract class MarkupConverter
	{
		/// <summary>
		/// Turns the provided markup format into HTML.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public abstract string ToHtml(string text);
	}
}
