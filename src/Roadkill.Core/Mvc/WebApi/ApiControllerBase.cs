using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Controllers.Api
{
	public class ApiControllerBase : ApiController
	{
		protected ApplicationSettings ApplicationSettings;
		protected UserServiceBase UserService;
		protected IUserContext UserContext;

		public ApiControllerBase(ApplicationSettings appSettings, UserServiceBase userService, IUserContext userContext)
		{
			ApplicationSettings = appSettings;
			UserService = userService;
			UserContext = userContext;
		}

		public override System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> ExecuteAsync(System.Web.Http.Controllers.HttpControllerContext controllerContext, System.Threading.CancellationToken cancellationToken)
		{
			return base.ExecuteAsync(controllerContext, cancellationToken);
		}

		protected override void Initialize(System.Web.Http.Controllers.HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);

			// Redirect if Roadkill isn't installed or an upgrade is needed.
			if (!ApplicationSettings.Installed)
			{
				return;
			}
			else if (ApplicationSettings.UpgradeRequired)
			{
				return;
			}

			UserContext.CurrentUser = UserService.GetLoggedInUserName(new HttpContextWrapper(HttpContext.Current));
		}
	}
}
