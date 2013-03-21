
using System;
namespace Roadkill.Core
{
	/// <summary>
	/// Encapsulates all Roadkill-specific information about the current user and page.
	/// </summary>
	public class RoadkillContext : IRoadkillContext
	{
		private bool? _isAdmin;
		private bool? _isEditor;
		private User _user;
		private UserManager _userManager;

		/// <summary>
		/// Creates a new instance of a <see cref="RoadkillContext"/>.
		/// </summary>
		/// <param name="userManager">A <see cref="UserManager"/> which the RoadkillContext uses for user lookups.</param>
		public RoadkillContext(UserManager userManager)
		{
			_userManager = userManager;
		}

		/// <summary>
		/// The current logged in user id (a guid), or a username including domain suffix for Windows authentication.
		/// This is set once a controller action has finished execution.
		/// </summary>
		public string CurrentUser { get; set; }

		/// <summary>
		/// Gets the username of the current user. This differs from <see cref="CurrentUser"/> which retrieves the email,
		/// unless using windows auth where both fields are the same.
		/// This property is derived from the current UserManager's GetUser() method, if the CurrentUser property is not empty.
		/// </summary>
		public string CurrentUsername
		{
			get
			{
				if (IsLoggedIn)
				{
					if (_user != null)
					{
						return _user.Username;
					}
					else
					{
						Guid userId;
						if (Guid.TryParse(CurrentUser, out userId) && userId != Guid.Empty)
						{
							// Guids are now used for cookie auth
							_user = _userManager.GetUserById(userId); // handle old logins by ignoring them
							if (_user != null)
							{
								return _user.Username;
							}
							else
							{
								_userManager.Logout();
								return "(User id no longer exists)";
							}
						}
						else
						{
							_userManager.Logout();
							return CurrentUser;
						}
					}
				}
				else
					return "";
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
						_isAdmin = _userManager.IsAdmin(CurrentUser);
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
						_isEditor = _userManager.IsEditor(CurrentUser);
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
		/// Returns true if the current page is not the initial dummy 'mainpage'.
		/// </summary>
		public bool IsContentPage
		{
			get
			{
				return Page != null;
			}
		}

		/// <summary>
		/// Whether the user is currently logged in or not.
		/// </summary>
		public bool IsLoggedIn
		{
			get
			{
				return !string.IsNullOrWhiteSpace(CurrentUser);
			}
		}

		/// <summary>
		/// The underlying <see cref="PageSummary"/> object for the current page.
		/// </summary>
		public PageSummary Page { get; set; }	
	}
}