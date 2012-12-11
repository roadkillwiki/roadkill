using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Globalization;
using Roadkill.Core.Configuration;

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
			string templatePath = HttpContext.Current.Server.MapPath("~/App_Data/EmailTemplates/");
			string culturePath = Path.Combine(templatePath, CultureInfo.CurrentUICulture.Name);

			string htmlFile = Path.Combine(templatePath, "ResetPassword.html");
			string plainTextFile = Path.Combine(templatePath, "ResetPassword.txt");

			// If there's templates for the current culture, then use those instead
			if (Directory.Exists(culturePath))
			{
				string cultureHtmlFile = Path.Combine(culturePath, "ResetPassword.html");
				if (File.Exists(cultureHtmlFile))
					htmlFile = cultureHtmlFile;

				string culturePlainTextFile = Path.Combine(culturePath, "ResetPassword.txt");
				if (File.Exists(culturePlainTextFile))
					plainTextFile = culturePlainTextFile;
			}

			_htmlContent = File.ReadAllText(htmlFile);
			_plainTextContent = File.ReadAllText(plainTextFile);
		}

		public ResetPasswordEmail(UserSummary summary, IConfigurationContainer config)
			: base(summary, _plainTextContent, _htmlContent, config)
		{
		}
	}
}
