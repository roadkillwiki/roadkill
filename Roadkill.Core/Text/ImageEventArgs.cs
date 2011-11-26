using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// Holds information when an image is processed, giving the caller the ability to translate the outputted HTML.
	/// </summary>
	public class ImageEventArgs : EventArgs
	{
		/// <summary>
		/// The original image source url.
		/// </summary>
		public string OriginalSrc { get; set; }

		/// <summary>
		/// The source url used inside the HTML.
		/// </summary>
		public string Src { get; set; }

		/// <summary>
		/// The alt tag for the image.
		/// </summary>
		public string Alt { get; set; }

		/// <summary>
		/// The title tag for the image.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageEventArgs"/> class.
		/// </summary>
		public ImageEventArgs(string originalSrc, string src, string alt, string title)
		{
			OriginalSrc = originalSrc;
			Src = src;
			Alt = alt;
			Title = title;
		}
	}
}
