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
	public class RoadkillContext
	{
		private static readonly string CONTEXT_KEY = "ROADKILL_CONTEXT";
		private static RoadkillContext _contextForNoneWeb;

		public static bool IsWeb { get; set; }
		public string CurrentUser { get; set; }

		public bool IsLoggedIn
		{
			get
			{
				return !string.IsNullOrWhiteSpace(CurrentUser);
			}
		}

		/// <summary>
		/// Indicates whether the current user is a member of the admin role.
		/// </summary>
		public bool IsAdmin
		{
			get
			{
				return Roles.IsUserInRole(RoadkillSettings.AdminRoleName);
			}
		}

		public PageSummary Page { get; set; }

		public bool IsContentPage
		{
			get
			{
				return Page != null;
			}
		}

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

		static RoadkillContext()
		{
			IsWeb = true;
		}

		public RoadkillContext()
		{		
		}
	}
}