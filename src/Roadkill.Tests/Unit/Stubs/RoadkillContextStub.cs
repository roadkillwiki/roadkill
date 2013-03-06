using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;

namespace Roadkill.Tests.Unit
{
	internal class RoadkillContextStub : IRoadkillContext
	{
		public string CurrentUser { get; set; }
		public string CurrentUsername { get; set; }
		public bool IsAdmin { get; set; }
		public bool IsContentPage { get; set; }
		public bool IsEditor { get; set; }
		public bool IsLoggedIn { get; set; }
		public PageSummary Page { get; set; }
	}
}
