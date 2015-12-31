using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.WebApi
{
	[RoutePrefix("api/user")]
	[ApiKeyAuthorize]
	public class UserController : ApiController
	{
		private readonly UserServiceBase _userService;

		public UserController(UserServiceBase userService)
		{
			_userService = userService;
		}

		/// <summary>
		/// Retrieves a user by their id [admin access is required to make this call].
		/// </summary>
		/// <param name="id">The id of the user</param>
		/// <returns>The user's details.</returns>
		[HttpGet]
		public UserViewModel Get(Guid id)
		{
			User user = _userService.GetUserById(id);
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
