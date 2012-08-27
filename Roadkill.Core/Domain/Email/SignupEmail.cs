using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Globalization;

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
			string templatePath = HttpContext.Current.Server.MapPath("~/App_Data/EmailTemplates/");
			string culturePath = Path.Combine(templatePath, CultureInfo.CurrentUICulture.Name);

			string htmlFile = Path.Combine(templatePath, "Signup.html");
			string plainTextFile = Path.Combine(templatePath, "Signup.txt");

			// If there's templates for the current culture, then use those instead
			if (Directory.Exists(culturePath))
			{
				string cultureHtmlFile = Path.Combine(culturePath, "Signup.html");
				if (File.Exists(cultureHtmlFile))
					htmlFile = cultureHtmlFile;

				string culturePlainTextFile = Path.Combine(culturePath, "Signup.txt");
				if (File.Exists(culturePlainTextFile))
					plainTextFile = culturePlainTextFile;
			}

			_htmlContent = File.ReadAllText(htmlFile);
			_plainTextContent = File.ReadAllText(plainTextFile);
		}

		public SignupEmail(UserSummary summary)
			: base(summary, _plainTextContent, _htmlContent)
		{
		}
	}
}
