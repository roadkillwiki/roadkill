using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Globalization;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Database;

namespace Roadkill.Core.Email
{
	/// <summary>
	/// The template for signup emails.
	/// </summary>
	public class SignupEmail : EmailTemplate
	{
		private static string _htmlContent;
		private static string _plainTextContent;

		public SignupEmail(ApplicationSettings applicationSettings, IRepository repository, IEmailClient emailClient)
			: base(applicationSettings, repository, emailClient)
		{
		}

		public override void Send(UserViewModel model)
		{
			// Thread safety should not be an issue here
			if (string.IsNullOrEmpty(_plainTextContent))
				_plainTextContent = ReadTemplateFile("Signup.txt");

			if (string.IsNullOrEmpty(_htmlContent))
				_htmlContent = ReadTemplateFile("Signup.html");

			PlainTextView = _plainTextContent;
			HtmlView = _htmlContent;

			base.Send(model);
		}
	}
}
