using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using StructureMap;
using StructureMap.Attributes;

namespace Roadkill.Core
{
	// Layout pages aren't created using IDependencyResolver (as they're outside of MVC). So use bastard injection for them.
	public abstract class RoadkillLayoutPage : WebViewPage<object>
	{
		public ApplicationSettings ApplicationSettings { get; set; }
		public IRoadkillContext RoadkillContext { get; set; }
		public MarkupConverter MarkupConverter { get; set; }

		public RoadkillLayoutPage()
		{
			ApplicationSettings = ObjectFactory.GetInstance<ApplicationSettings>();
			RoadkillContext = ObjectFactory.GetInstance<IRoadkillContext>();
			MarkupConverter = ObjectFactory.GetInstance<MarkupConverter>();
		}
	}
}
