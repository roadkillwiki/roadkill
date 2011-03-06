using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using System.Web;

namespace Roadkill.Core.Controllers
{
	public class ControllerBase : Controller
	{
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			RoadkillContext.Current.CurrentUser = GetCurrentUser();
		}

		protected string GetCurrentUser()
		{
			if (FormsAuthentication.IsEnabled)
			{
				if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
				{
					string cookie = Request.Cookies[FormsAuthentication.FormsCookieName].Value;
					if (!string.IsNullOrEmpty(cookie))
					{
						FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie);
						return ticket.Name;
					}
				}

				return "";
			}
			else
			{
				return Request.LogonUserIdentity.Name;
			}
		}
	}
}
