using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using StructureMap;
using StructureMap.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Attachments;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Over-rides the OutputCache so it doesn't force text/html
	/// </summary>
	public class CacheContentTypeAttribute : OutputCacheAttribute
	{
		public string ContentType { get; set; }

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			base.OnResultExecuted(filterContext);

			ContentType = ContentType ?? "text/html";
			filterContext.HttpContext.Response.ContentType = ContentType;
		}
	}
}
