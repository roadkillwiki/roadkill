using System.Collections.Generic;
using System.IO;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Roadkill.Tests.Unit.StubsAndMocks.Owin
{
	public class OwinContextStub : IOwinContext
	{
		private OwinRequestStub _request;

		IOwinRequest IOwinContext.Request
		{
			get { return _request; }
		}

		public OwinRequestStub Request
		{
			get { return _request; }
		}

		public IOwinResponse Response { get; set; }
		public IAuthenticationManager Authentication { get; set; }
		public IDictionary<string, object> Environment { get; set; }
		public TextWriter TraceOutput { get; set; }

		public OwinContextStub()
		{
			_request = new OwinRequestStub();
			Response = new OwinResponse();
		}

		public T Get<T>(string key)
		{
			return default(T);
		}

		public IOwinContext Set<T>(string key, T value)
		{
			return null;
		}
	}
}