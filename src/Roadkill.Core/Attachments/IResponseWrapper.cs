using System;
namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// Defines a class that wraps the HttpResponse object.
	/// </summary>
	public interface IResponseWrapper
	{
		void AddStatusCodeForCache(string fullPath, string modifiedSinceHeader);
		void BinaryWrite(byte[] buffer);
		string ContentType { get; set; }
		void End();
		int StatusCode { get; set; }
		void Write(string text);
	}
}
