using System;

namespace Roadkill.Core.Exceptions
{
	/// <summary>
	/// An exception that should be translated into a HTTP status code (replaces System.Web.HttpException).
	/// </summary>
	public class HttpStatusException : Exception
	{
		public int StatusCode { get; private set; }

		public HttpStatusException(int statusCode, string message) : base(message)
		{
			StatusCode = statusCode;
		}
	}
}
