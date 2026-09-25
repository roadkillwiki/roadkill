using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.WebApi
{
	/// <summary>
	/// REST api for users: GET api/user, GET api/user/{id}
	/// </summary>
	[ApiController]
	[Route("api/user")]
	[ApiKeyAuthorize]
	public class UserController : Microsoft.AspNetCore.Mvc.ControllerBase
	{
		private readonly UserServiceBase _userService;

		public UserController(UserServiceBase userService)
		{
			_userService = userService;
		}

		[HttpGet("{id:guid}")]
		public UserViewModel Get(Guid id)
		{
			User user = _userService.GetUserById(id);
			if (user == null)
				return null;

			return new UserViewModel(user);
		}

		[HttpGet]
		public IEnumerable<UserViewModel> Get()
		{
			return _userService.ListAdmins().Union(_userService.ListEditors());
		}

		public class UserInfo
		{
			public string Email { get; set; }
			public string Password { get; set; }
		}
	}
}
