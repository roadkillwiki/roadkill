using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Domain;
using StructureMap;
using Roadkill.Core.Controllers;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// Represents an attribute that is used to restrict access by callers to users that are in Admin role group.
	/// </summary>
	public class AdminRequiredAttribute : AuthorizeAttribute
	{
		private UserManager _userManager;

		public AdminRequiredAttribute()
		{
			// "Bastard injection" for now
			_userManager = ObjectFactory.GetInstance<UserManager>();
		}

		/// <summary>
		/// Provides an entry point for custom authorization checks.
		/// </summary>
		/// <param name="httpContext">The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP request.</param>
		/// <returns>
		/// true if the user is in the role name specified by the roadkill web.config adminRoleName setting or if this is blank; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="httpContext"/> parameter is null.</exception>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			IPrincipal user = httpContext.User;
			IIdentity identity = user.Identity;

			if (!identity.IsAuthenticated)
			{
				return false;
			}

			if (string.IsNullOrEmpty(RoadkillSettings.Current.ApplicationSettings.AdminRoleName))
				return true;

			if (_userManager.IsAdmin(identity.Name))
				return true;
			else
				return false;
		}
	}

	public class AdminRequiredFilterProvider : IFilterProvider
	{

		public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
		{
			if (controllerContext.Controller is SettingsController)
			{
				if (actionDescriptor.ActionName == "Index")
					return null;
			}

			return null;
		}
	}

	public class ConditionalFilterProvider : IFilterProvider
	{
		private readonly
		  IEnumerable<Func<ControllerContext, ActionDescriptor, object>> _conditions;

		public ConditionalFilterProvider(
		  IEnumerable<Func<ControllerContext, ActionDescriptor, object>> conditions)
		{

			_conditions = conditions;
		}

		public IEnumerable<Filter> GetFilters(
			ControllerContext controllerContext,
			ActionDescriptor actionDescriptor)
		{
			return from condition in _conditions
				   select condition(controllerContext, actionDescriptor) into filter
				   where filter != null
				   select new Filter(filter, FilterScope.Global, null);
		}
	}
}
