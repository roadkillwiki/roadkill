using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Email;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	public class EmailTemplateStub : EmailTemplate
	{
		public EmailTemplateStub(ApplicationSettings applicationSettings, IRepository repository, IEmailClient emailClient)
			: base(applicationSettings, repository, emailClient)
		{
			base.PlainTextView = "plaintextview";
			base.HtmlView = "htmlview";
		}
		
		public override void Send(UserViewModel model)
		{
			base.Send(model);
		}

		public IEmailClient GetEmailClient()
		{
			return base.EmailClient;
		}

		public SiteSettings GetSiteSettings()
		{
			return SiteSettings;
		}
	}
}
