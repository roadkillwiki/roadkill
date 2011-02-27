using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Spruce.Core;

namespace Spruce.Site
{
	public partial class SiteMaster : ViewMasterPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ViewData["CurrentUser"] = SpruceContext.Current.CurrentUser;
			ViewData["CurrentProjectName"] = SpruceContext.Current.CurrentProject.Name;

			ViewData["CurrentIterationName"] = SpruceContext.Current.FilterSettings.IterationName;
			ViewData["CurrentIterationPath"] = SpruceContext.Current.FilterSettings.IterationPath;
			ViewData["CurrentAreaName"] = SpruceContext.Current.FilterSettings.AreaName;
			ViewData["CurrentAreaPath"] = SpruceContext.Current.FilterSettings.AreaPath;

			ViewData["Projects"] = SpruceContext.Current.ProjectNames;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
		}
	}
}