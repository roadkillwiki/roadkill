using System;
namespace Roadkill.Core.Attachments
{
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
