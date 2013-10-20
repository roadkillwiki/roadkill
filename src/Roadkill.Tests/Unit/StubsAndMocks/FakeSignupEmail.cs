using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	public class FakeSignupEmail : SignupEmail
	{
		public bool IsSent { get; set; }
		public UserViewModel Summary { get; set; }

		public FakeSignupEmail(ApplicationSettings applicationSettings, SiteSettings siteSettings)
			: base(applicationSettings, siteSettings)
		{
		}
		
		public override void Send(UserViewModel summary)
		{
			ReplaceTokens(summary, "{EMAIL}");
			IsSent = true;
			Summary = summary;
		}
	}

	public class FakeResetPasswordEmail : ResetPasswordEmail
	{
		public bool IsSent { get; set; }
		public UserViewModel Summary { get; set; }

		public FakeResetPasswordEmail(ApplicationSettings applicationSettings, SiteSettings siteSettings)
			: base(applicationSettings, siteSettings)
		{
		}

		public override void Send(UserViewModel summary)
		{
			ReplaceTokens(summary, "{EMAIL}");
			IsSent = true;
			Summary = summary;
		}
	}
}
