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
		public ApplicationSettings ApplicationSettings { get; set; }
		
		[SetterProperty]
		public IRoadkillContext RoadkillContext { get; set; }
		
		[SetterProperty]
		public MarkupConverter MarkupConverter { get; set; }
	}
}
