using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace Roadkill.Core
{
	/// <summary>
	/// The template for password reset emails.
	/// </summary>
	public class ResetPasswordEmail : Email
	{
		private static string _htmlContent;
		private static string _plainTextContent;

		static ResetPasswordEmail()
		{
			_htmlContent = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/EmailTemplates/ResetPassword.html"));
			_plainTextContent = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/EmailTemplates/ResetPassword.txt"));
		}

		public ResetPasswordEmail(UserSummary summary)
			: base(summary, _plainTextContent, _htmlContent)
		{
		}
	}
}
