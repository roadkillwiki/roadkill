using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Web.Mvc;
using NHibernate;
using System.Web.Security;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// Encapsulates all Roadkill-specific information about the current user and page.
	/// </summary>
	public class RoadkillContext
	{
		private static readonly string CONTEXT_KEY = "ROADKILL_CONTEXT";
		private static RoadkillContext _contextForNoneWeb;

		/// <summary>
		/// The current logged in user name (including domain suffix for Windows authentication).
		/// This is set once a controller action has finished execution.
		/// </summary>
		public string CurrentUser { get; set; }

		/// <summary>
		/// Gets the username of the current user. This differs from <see cref="CurrenUser"/> which retrieves the email,
		/// unless using windows auth where both fields are the same.
		/// This property is derived from the current UserManager's GetUser() method, if the CurrentUser property is not empty.
		/// </summary>
		public string CurrentUsername
		{
			get
			{
				if (IsLoggedIn)
					return UserManager.Current.GetUser(CurrentUser).Username;
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
					return UserManager.Current.IsAdmin(CurrentUser);
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
					return UserManager.Current.IsEditor(CurrentUser);
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
		/// Whether the <see cref="RoadkillContext"/> is running inside a web environment. This
		/// setting is for unit testing, to ensure the <see cref="Current"/> property does not
		/// use the HttpContext.Current.Items as a store.
		/// </summary>
		public static bool IsWeb { get; set; }	

		/// <summary>
		/// The underlying <see cref="PageSummary"/> object for the current page.
		/// </summary>
		public PageSummary Page { get; set; }	

		/// <summary>
		/// The current <see cref="RoadkillContext"/>. One context exists per request and is stored in the HttpContext.Current.Items.
		/// </summary>
		public static RoadkillContext Current
		{
			get
			{
				if (IsWeb)
				{
					RoadkillContext context = HttpContext.Current.Items[CONTEXT_KEY] as RoadkillContext;
					if (context == null)
					{
						context = new RoadkillContext();
						HttpContext.Current.Items[CONTEXT_KEY] = context;
					}

					return context;
				}
				else
				{
					if (_contextForNoneWeb == null)
						_contextForNoneWeb = new RoadkillContext();

					return _contextForNoneWeb;
				}
			}
		}

		/// <summary>
		/// Static constructor for the <see cref="RoadkillContext"/> class.
		/// </summary>
		static RoadkillContext()
		{
			IsWeb = true;
		}

		/// <summary>
		/// Clears the request context item that stores the current logged in username.
		/// </summary>
		public static void Clear()
		{
			if (IsWeb)
			{
				if (HttpContext.Current.Items[CONTEXT_KEY] != null)
				{
					HttpContext.Current.Items.Remove(CONTEXT_KEY);
				}

				string user = Current.CurrentUser;
			}
		}
	}
}