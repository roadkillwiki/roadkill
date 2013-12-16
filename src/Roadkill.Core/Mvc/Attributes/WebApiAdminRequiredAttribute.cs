using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Roadkill.Core.Configuration;
using Roadkill.Core.DI;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using StructureMap.Attributes;

namespace Roadkill.Core.Mvc.Attributes
{
	public class WebApiAdminRequiredAttribute : System.Web.Http.AuthorizeAttribute, ISetterInjected, IAuthorizationAttribute
	{
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		[SetterProperty]
		public IUserContext Context { get; set; }

		[SetterProperty]
		public UserServiceBase UserService { get; set; }

		[SetterProperty]
		public IPageService PageService { get; set; }

		[SetterProperty]
		public SettingsService SettingsService { get; set; }

		[SetterProperty]
		public IAuthorizationProvider AuthorizationProvider { get; set; }

		protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			if (AuthorizationProvider == null)
				throw new SecurityException("The AuthorizationProvider property has not been set for WebApiAdminRequiredAttribute.", null);

			IPrincipal principal = Thread.CurrentPrincipal;
			return AuthorizationProvider.IsAdmin(principal);
		}
	}
}
