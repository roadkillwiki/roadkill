using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Web.Mvc;
using NHibernate;

namespace Roadkill.Core
{
	public class RoadkillContext
	{
		private static readonly string CONTEXT_KEY = "ROADKILL_CONTEXT";
		private static RoadkillContext _contextForNoneWeb;

		public static bool IsWeb { get; set; }
		public string CurrentUser { get; set; }

		public static RoadkillContext Current
		{
			get
			{
				if (IsWeb)
				{
					// Use a session instead of HttpContext.Items as Items doesn't survive redirects
					RoadkillContext context = HttpContext.Current.Session[CONTEXT_KEY] as RoadkillContext;
					if (context == null)
					{
						context = new RoadkillContext();
						HttpContext.Current.Session[CONTEXT_KEY] = context;
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