using Roadkill.Core.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// A wrapper around HttpResponse, including caching capabilities.
	/// </summary>
	/// <summary>
	/// Wraps the ASP.NET Core <see cref="HttpResponse"/>. The body is buffered until <see cref="FlushAsync"/> is called,
	/// so the file service can stay synchronous and testable.
	/// </summary>
	public class ResponseWrapper : IResponseWrapper
	{
		private readonly HttpResponse _response;
		private byte[] _buffer;
		private string _text;

		public int StatusCode { get; set; }
		public string ContentType { get; set; }

		/// <summary>
		/// Whether the response is for a private site: it's then only cached by the browser, not by shared caches (proxies).
		/// </summary>
		public bool IsPrivate { get; set; }

		public ResponseWrapper()
		{
			StatusCode = 200;
		}

		public ResponseWrapper(HttpResponse response) : this()
		{
			_response = response;
		}

		public void Write(string text)
		{
			_text = (_text ?? "") + text;
		}

		public void BinaryWrite(byte[] buffer)
		{
			_buffer = buffer;
		}

		public void End()
		{
		}

		public void AddStatusCodeForCache(string fullPath, string modifiedSinceHeader)
		{
			FileInfo info = new FileInfo(fullPath);
			AddStatusCodeForCache(fullPath, modifiedSinceHeader, info.LastWriteTimeUtc);
		}

		public void AddStatusCodeForCache(string fileName, string modifiedSinceHeader, DateTime lastWriteTimeUtc)
		{
			// https://developers.google.com/speed/docs/best-practices/caching
			int statusCode = GetStatusCodeForCache(lastWriteTimeUtc, modifiedSinceHeader);
			StatusCode = statusCode;

			if (_response != null)
			{
				_response.Headers[HeaderNames.CacheControl] = IsPrivate ? "private" : "public";
				_response.Headers[HeaderNames.Expires] = "-1"; // always followed by the browser
				_response.Headers[HeaderNames.LastModified] = lastWriteTimeUtc.ClearMilliseconds().ToString("R"); // sometimes followed by the browser
			}
		}

		/// <summary>
		/// Writes the status code, content type and buffered body to the underlying response.
		/// </summary>
		public async Task FlushAsync()
		{
			if (_response == null)
				return;

			_response.StatusCode = StatusCode;

			if (StatusCode == 304)
				return;

			if (!string.IsNullOrEmpty(ContentType))
				_response.ContentType = ContentType;

			if (_buffer != null)
			{
				_response.ContentLength = _buffer.Length;
				await _response.Body.WriteAsync(_buffer, 0, _buffer.Length);
			}
			else if (_text != null)
			{
				await _response.WriteAsync(_text);
			}
		}

		public static int GetStatusCodeForCache(DateTime fileDate, string modifiedSinceHeader)
		{
			int status = 200;

			// When If-modified is sent (never when it's incognito mode), it matches the 
			// the write time you send back for the file. So 1st Jan 2001, it will send back
			// 1st Jan 2001 for If-Modified.	
			DateTime modifiedSinceDate = GetLastModifiedDate(modifiedSinceHeader);
			if (modifiedSinceDate != DateTime.MinValue)
			{
				status = 304;
				DateTime lastWriteTime = new DateTime(fileDate.Year, fileDate.Month, fileDate.Day, fileDate.Hour, fileDate.Minute, fileDate.Second, 0, DateTimeKind.Utc);
				if (lastWriteTime != modifiedSinceDate)
					status = 200;
			}

			return status;
		}

		/// <summary>
		/// Parses the modified string given, turning the date into a UTC date and removing any milliseconds
		/// from the DateTime returned. If the string isn't a valid date, DateTime.Min is returned.
		/// </summary>
		public static DateTime GetLastModifiedDate(string modifiedSince)
		{
			DateTime modifiedSinceDate = DateTime.MinValue;

			if (!string.IsNullOrWhiteSpace(modifiedSince))
			{
				if (DateTime.TryParse(modifiedSince, out modifiedSinceDate))
				{
					modifiedSinceDate = modifiedSinceDate.ToUniversalTime();
					modifiedSinceDate = modifiedSinceDate.ClearMilliseconds();
				}
			}

			return modifiedSinceDate;
		}
	}
}
