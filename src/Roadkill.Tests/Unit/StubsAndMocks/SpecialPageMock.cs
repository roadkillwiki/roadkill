using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Plugins;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class SpecialPageMock : SpecialPagePlugin
	{
		public override string Name
		{
			get { return "kay"; }
		}

		public override IActionResult GetResult(SpecialPagesController controller)
		{
			return new ContentResult() { Content = "Some content" };
		}
	}
}
