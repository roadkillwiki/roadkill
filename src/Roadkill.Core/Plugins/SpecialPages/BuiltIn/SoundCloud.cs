using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using StructureMap.Attributes;

namespace Roadkill.Core.Plugins.SpecialPage.BuiltIn
{
	public class SoundCloud : SpecialPagePlugin
	{
		public override string Name
		{
			get
			{
				return "SoundCloud";
			}
		}

		public override ActionResult GetResult(SpecialPagesController controller)
		{
			// View example, soon to be removed
			return new ViewResult() { ViewName = "SoundCloud/SoundCloud", };
		}
	}
}
