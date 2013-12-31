using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace Roadkill.Core.Security
{
	/// <summary>
	/// Used to wrap FormsAuthentication methods, where Mono does not implement the methods or 
	/// behaves slightly differently from the Windows implementation.
	/// </summary>
	public class FormsAuthenticationWrapper
	{
		public static bool IsEnabled()
		{
#if MONO
			return true; // Mono doesn't support FormsAuthentication
#else
			return FormsAuthentication.IsEnabled;
#endif
		}

		public static string CookieName()
		{
#if MONO
			if (!string.IsNullOrEmpty(FormsAuthentication.FormsCookieName))
				return FormsAuthentication.FormsCookieName;
			else
				return "";
#else
			return FormsAuthentication.FormsCookieName;
#endif
		}
	}
}
