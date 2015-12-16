using System.Web;
using Moq;

namespace Roadkill.Tests.Unit.StubsAndMocks.Mvc
{
	/// <summary>
	/// Provides access to the Mock objects for the MVC mock helper extention methods.
	/// </summary>
	public class MvcMockContainer
	{
		public Mock<HttpContextBase> Context { get; set; }
		public Mock<HttpRequestBase> Request { get; set; }
		public Mock<HttpResponseBase> Response { get; set; }
		public Mock<HttpSessionStateBase> SessionState { get; set; }
		public Mock<HttpServerUtilityBase> ServerUtility { get; set; }
	}
}
