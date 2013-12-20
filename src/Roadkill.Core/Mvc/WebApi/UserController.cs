using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Controllers.Api
{
	public class UserController : ApiControllerBase
	{
		private UserServiceBase _userService;

		public UserController(UserServiceBase userService)
		{
			_userService = userService;
		}

		/// <summary>
		/// Logs a user, assigning a cookie in the resulting headers which can then be 
		/// used for further API requests.
		/// </summary>
		/// <param name="user">The user's email and password</param>
		/// <returns>True if the authentication request was sucessful, false otherwise.</returns>
		[HttpPost]
		[Route("Authenticate")]
		public bool Authenticate(UserInfo user)
		{
			return _userService.Authenticate(user.Email, user.Password);
		}

		/// <summary>
		/// Logouts out the current user session, clearing the login cookie.
		/// </summary>
		/// <returns>"OK" if the logout call was successful.</returns>
		[HttpGet]
		[Route("Logout")]
		public string Logout()
		{
			_userService.Logout();
			return "OK";
		}

		/// <summary>
		/// Retrieves a user by their id [admin access is required to make this call].
		/// </summary>
		/// <param name="id">The id of the user</param>
		/// <returns>The user's details.</returns>
		[WebApiAdminRequired]
		[HttpGet]
		public UserViewModel Get(Guid id)
		{
			User user = _userService.GetUserById(id, false);
			if (user == null)
				return null;

			return new UserViewModel(user);
		}

		/// <summary>
		/// Lists all users in the system [admin access is required to make this call].
		/// </summary>
		/// <returns>An array of user objects.</returns>
		[WebApiAdminRequired]
		[HttpGet]
		public IEnumerable<UserViewModel> Get()
		{
			return _userService.ListAdmins().Union(_userService.ListEditors());
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
