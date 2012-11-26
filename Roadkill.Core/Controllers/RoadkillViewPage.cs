using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using StructureMap;

namespace Roadkill.Core
{
	public abstract class RoadkillViewPage<T> : WebViewPage<T>, IInjectionLaunderer
	{
		public IConfigurationContainer Configuration { get; private set; }
		public IRoadkillContext RoadkillContext { get; private set; }
		public MarkupConverter MarkupConverter { get; private set; }

		public RoadkillViewPage()
		{
			Configuration = ObjectFactory.GetInstance<IConfigurationContainer>();
			RoadkillContext = ObjectFactory.GetInstance<IRoadkillContext>();
			MarkupConverter = ObjectFactory.GetInstance<MarkupConverter>();
		}
	}
}
