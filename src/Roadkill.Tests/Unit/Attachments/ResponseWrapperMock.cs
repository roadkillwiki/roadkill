using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Attachments;

namespace Roadkill.Tests.Unit
{
	public class ResponseWrapperMock : IResponseWrapper
	{
		public int StatusCode { get; set; }
		public string ContentType { get; set; }

		public byte[] Buffer { get; set; }
		public string Text { get; set; }

		public void AddStatusCodeForCache(string fullPath, string modifiedSinceHeader)
		{
			StatusCode = 200;
		}

		public void BinaryWrite(byte[] buffer)
		{
			Buffer = buffer;
		}

		public void End()
		{

		}

		public void Write(string text)
		{
			Text = text;
		}
	}
}
