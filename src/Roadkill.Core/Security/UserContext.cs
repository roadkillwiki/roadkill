using System;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Core.Security
{
	/// <summary>
	/// Encapsulates all Roadkill-specific information about the current user on the site.
	/// </summary>
	public class UserContext : IUserContext
	{
		private bool? _isAdmin;
		private bool? _isEditor;
		private User _user;
		private UserServiceBase _userService;

		/// <summary>
		/// Creates a new instance of a <see cref="UserContext"/>.
		/// </summary>
		/// <param name="userService">A <see cref="UserServiceBase"/> which the RoadkillContext uses for user lookups.</param>
		public UserContext(UserServiceBase userService)
		{
			_userService = userService;
		}

		/// <summary>
		/// The current logged in user id (a guid), or a username including domain suffix for Windows authentication.
		/// This is set once a controller action has finished execution.
		/// </summary>
		public string CurrentUser { get; set; }

		/// <summary>
		/// Gets the username of the current user. This differs from <see cref="CurrentUser"/> which retrieves the email,
		/// unless using windows auth where both fields are the same.
		/// This property is derived from the current UserService's GetLoggedInUser() method, if the CurrentUser property is not empty.
		/// </summary>
		public string CurrentUsername
		{
			get
			{
				if (IsLoggedIn)
				{
					if (_user == null)
					{
						_user = _userService.GetLoggedInUser(CurrentUser);

						if (_user == null)
						{
							// Assume the cookie/request identity value is bad, and logout.
							_userService.Logout();
							return "";
						}
					}

					return _user.Username;
				}
				else
				{
					return "";
				}
			}
		}

		/// <summary>
		/// Indicates whether the current user is a member of the admin role.
		/// </summary>
		public bool IsAdmin
		{
			get
			{
				if (IsLoggedIn)
				{
					if (_isAdmin == null)
					{
						_isAdmin = _userService.IsAdmin(CurrentUser);
					}

					return _isAdmin.Value;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Indicates whether the current user is a member of the editor role.
		/// </summary>
		public bool IsEditor
		{
			get
			{
				if (IsLoggedIn)
				{
					if (_isEditor == null)
					{
						_isEditor = _userService.IsEditor(CurrentUser);
					}

					return _isEditor.Value;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Whether the user is currently logged in or not. If the <see cref="CurrentUser"/> 
		/// property is populated, this is assumed to be true.
		/// </summary>
		public bool IsLoggedIn
		{
			get
			{
				return !string.IsNullOrWhiteSpace(CurrentUser);
			}
		}
	}
}