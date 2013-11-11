using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Roadkill.Core.Configuration;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using StructureMap.Attributes;

namespace Roadkill.Core.Mvc.Attributes
{
	public class WebApiEditorRequired : System.Web.Http.AuthorizeAttribute, ISetterInjected
	{
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		[SetterProperty]
		public IUserContext Context { get; set; }

		[SetterProperty]
		public UserServiceBase UserService { get; set; }

		[SetterProperty]
		public PageService PageService { get; set; }

		[SetterProperty]
		public SettingsService SettingsService { get; set; }

		protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			IPrincipal user = Thread.CurrentPrincipal;
			IIdentity identity = Thread.CurrentPrincipal.Identity;

			if (!identity.IsAuthenticated)
			{
				return false;
			}

			// An empty editor role name implies everyone is an editor - there's no page security.
			if (string.IsNullOrEmpty(ApplicationSettings.EditorRoleName))
				return true;

			if (UserService.IsAdmin(identity.Name) || UserService.IsEditor(identity.Name))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
