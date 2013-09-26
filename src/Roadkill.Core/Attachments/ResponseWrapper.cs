using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// A wrapper around HttpResponse...just to help tests.
	/// </summary>
	public class ResponseWrapper : IResponseWrapper
	{
		private NameValueCollection _headers;
		private HttpResponse _context;

		public int StatusCode { get; set; }
		public string ContentType { get; set; }

		public ResponseWrapper()
		{
		}

		public ResponseWrapper(HttpResponse context)
		{
			_context = context;
		}

		public void Write(string text)
		{
			if (_context != null)
				_context.Write(text);
		}

		public void BinaryWrite(byte[] buffer)
		{
			if (_context != null)
				_context.BinaryWrite(buffer);
		}

		public void End()
		{
			if (_context != null)
				_context.End();
		}

		/// <summary>
		/// Adds the HTTP headers for cache expiry, and status code.
		/// </summary>
		/// <param name="fullPath"></param>
		/// <param name="modifiedSinceHeader"></param>
		public void AddStatusCodeForCache(string fullPath, string modifiedSinceHeader)
		{
			if (_context != null)
			{
				// https://developers.google.com/speed/docs/best-practices/caching
				_context.AddFileDependency(fullPath);

				FileInfo info = new FileInfo(fullPath);
				_context.Cache.SetCacheability(HttpCacheability.Public);
				_context.Headers.Add("Expires", "-1"); // always followed by the browser
				_context.Cache.SetLastModifiedFromFileDependencies(); // sometimes followed by the browser
				 int statusCode = GetStatusCodeForCache(info.LastWriteTimeUtc, modifiedSinceHeader);

				_context.StatusCode = statusCode;
				StatusCode = statusCode;
			}
		}

		/// <summary>
		/// Gets a 304 HTTP response if there is a "If-Modified-Since" header and it matches 
		/// the fileDate. Otherwise a 200 OK is given.
		/// </summary>
		/// <param name="fileDate">The date the item was last modified.</param>
		/// <param name="modifiedSinceHeader">The modified since header (an ISO date). If this doesn't 
		/// exist then 200 is returned.</param>
		/// <returns>The status code for the cache - 200 or 304.</returns>
		public static int GetStatusCodeForCache(DateTime fileDate, string modifiedSinceHeader)
		{
			int status = 200;
			if (!string.IsNullOrEmpty(modifiedSinceHeader))
			{
				// When If-modified is sent (never when it's incognito mode), it matches the 
				// the write time you send back for the file. So 1st Jan 2001, it will send back
				// 1st Jan 2001 for If-Modified.
				status = 304;
				DateTime modifiedSinceDate = DateTime.UtcNow;
				if (DateTime.TryParse(modifiedSinceHeader, out modifiedSinceDate))
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
