using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Roadkill.Core.Plugins.SpecialPages;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class SpecialPageMock : SpecialPage
	{
		public override string Name
		{
			get { return "kay"; }
		}

		public override ActionResult GetResult()
		{
			return new ContentResult() { Content = "Some content" };
		}
	}
}
