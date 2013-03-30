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
	public abstract class RoadkillViewPage<T> : WebViewPage<T>
	{
		[SetterProperty]
		public IConfigurationContainer Configuration { get; set; }
		
		[SetterProperty]
		public IRoadkillContext RoadkillContext { get; set; }
		
		[SetterProperty]
		public MarkupConverter MarkupConverter { get; set; }
	}

	// Layout pages aren't created using IDependencyResolver (as they're out of MVC).
	// So use bastard injection for them.
	public abstract class RoadkillLayoutPage<T> : RoadkillViewPage<T>
	{
		public RoadkillLayoutPage()
		{
			Configuration = ObjectFactory.GetInstance<IConfigurationContainer>();
			RoadkillContext = ObjectFactory.GetInstance<IRoadkillContext>();
			MarkupConverter = ObjectFactory.GetInstance<MarkupConverter>();
		}
	}
}
