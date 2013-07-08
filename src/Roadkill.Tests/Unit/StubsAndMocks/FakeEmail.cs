using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	public class FakeEmail : Email
	{
		public bool IsSent { get; set; }
		public UserSummary Summary { get; set; }

		public FakeEmail(ApplicationSettings applicationSettings, SiteSettings siteSettings)
			: base(applicationSettings, siteSettings)
		{
		}

		public override void Send(UserSummary summary)
		{
			ReplaceTokens(summary, "{EMAIL}");
			IsSent = true;
			Summary = summary;
		}
	}
}
