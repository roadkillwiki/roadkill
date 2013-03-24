using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using Roadkill.Core.Configuration;
using StructureMap;

namespace Roadkill.Core
{
	public class CacheAttribute : OutputCacheAttribute
	{
		// TODO: needs an attribute factory first

		private IConfigurationContainer _config;
		public CacheAttribute()
		{
			Location = OutputCacheLocation.ServerAndClient;
			_config = ObjectFactory.GetInstance<IConfigurationContainer>();
			if (!_config.ApplicationSettings.UseObjectCache)
			{
				Duration = 0;
			}
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			base.OnActionExecuted(filterContext);
		}
	}
}
