using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Roadkill.Core.Mvc.WebApi
{
	public class ApiKeyAuthorizeAttribute : AuthorizeAttribute
	{
		public static readonly string APIKEY_HEADER_KEY = "Authorization";

		public override void OnAuthorization(HttpActionContext actionContext)
		{
			if (!actionContext.Request.Headers.Contains(APIKEY_HEADER_KEY))
				actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);

			if (actionContext.Request.Headers.GetValues(APIKEY_HEADER_KEY).First() != "1234")
				actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
		}
	}
}