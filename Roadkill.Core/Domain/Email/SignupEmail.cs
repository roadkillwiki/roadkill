using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace Roadkill.Core
{
	/// <summary>
	/// The template for signup emails.
	/// </summary>
	public class SignupEmail : Email
	{
		private static string _htmlContent;
		private static string _plainTextContent;

		static SignupEmail()
		{
			_htmlContent = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/EmailTemplates/Signup.html"));
			_plainTextContent = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/EmailTemplates/Signup.txt"));
		}

		public SignupEmail(UserSummary summary)
			: base(summary, _plainTextContent, _htmlContent)
		{
		}
	}
}
