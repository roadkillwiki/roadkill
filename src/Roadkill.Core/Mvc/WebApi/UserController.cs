using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Controllers.Api
{
	[WebApiAdminRequired]
	[RoutePrefix("api/user")]
	public class UserController : ApiControllerBase
	{
		public UserController(ApplicationSettings appSettings, UserServiceBase userService, IUserContext userContext)
			: base(appSettings, userService, userContext)
		{
		}

		/// <summary>
		/// Logs a user, assigning a cookie in the resulting headers which can then be 
		/// used for further API requests.
		/// </summary>
		/// <param name="user">The user's email and password</param>
		/// <returns>True if the authentication request was successful, false otherwise.</returns>
		[HttpPost]
		[Route("authenticate")]
		[Route("~/api/Authenticate")]
		[AllowAnonymous]
		public bool Authenticate(UserInfo user)
		{
			return UserService.Authenticate(user.Email, user.Password);
		}

		/// <summary>
		/// Logouts out the current user session, clearing the login cookie.
		/// </summary>
		/// <returns>"OK" if the logout call was successful.</returns>
		[HttpGet]
		[Route("Logout")]
		[Route("~/api/Logout")]
		[AllowAnonymous]
		public string Logout()
		{
			UserService.Logout();
			return "OK";
		}

		/// <summary>
		/// Retrieves a user by their id [admin access is required to make this call].
		/// </summary>
		/// <param name="id">The id of the user</param>
		/// <returns>The user's details.</returns>
		[HttpGet]
		public UserViewModel Get(Guid id)
		{
			User user = UserService.GetUserById(id);
			if (user == null)
				return null;

			return new UserViewModel(user);
		}

		/// <summary>
		/// Lists all users in the system [admin access is required to make this call].
		/// </summary>
		/// <returns>An array of user objects.</returns>
		[HttpGet]
		public IEnumerable<UserViewModel> Get()
		{
			return UserService.ListAdmins().Union(UserService.ListEditors());
		}

		/// <summary>
		/// Json details sent in from a request
		/// </summary>
		public class UserInfo
		{
			public string Email { get; set; }
			public string Password { get; set; }
		}
	}
}
