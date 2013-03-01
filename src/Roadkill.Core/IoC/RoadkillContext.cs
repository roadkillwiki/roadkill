
using System;
namespace Roadkill.Core
{
	/// <summary>
	/// Encapsulates all Roadkill-specific information about the current user and page.
	/// </summary>
	public class RoadkillContext : IRoadkillContext
	{
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
					Guid userId;
					if (Guid.TryParse(CurrentUser, out userId) && userId != Guid.Empty)
					{
						// Guids are used for cookie auth
						return _userManager.GetUserById(userId).Username;
					}
					else
					{
						return CurrentUser;
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
					return _userManager.IsAdmin(CurrentUser);
				else
					return false;
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
					return _userManager.IsEditor(CurrentUser);
				else
					return false;
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