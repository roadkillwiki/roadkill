using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Optimization;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Mvc
{
	public class Bundles
	{
		public static string CssFilename { get; private set; }
		public static string JsFilename { get; private set; }

		public static void Register()
		{
			CssFilename = string.Format("roadkill{0}.css", ApplicationSettings.ProductVersion);
			JsFilename = string.Format("roadkill{0}.js", ApplicationSettings.ProductVersion);

			// Don't bundle the installer JS and CSS files, they referenced independently.

			// Bundle all CSS files into a single file		
			StyleBundle cssBundle = new StyleBundle("~/Assets/CSS/" + CssFilename);
            cssBundle.Transforms.Add(new QueryParamBundleTransform());
			IncludeCssFiles(cssBundle);

			// Bundle all JS files into a single file	
			ScriptBundle defaultJsBundle = new ScriptBundle("~/Assets/Scripts/" + JsFilename);
            defaultJsBundle.Transforms.Add(new QueryParamBundleTransform());
			IncludeJQueryScripts(defaultJsBundle);
			IncludeRoadkillScripts(defaultJsBundle);
			IncludeSharedScripts(defaultJsBundle);

			BundleTable.Bundles.Add(cssBundle);
			BundleTable.Bundles.Add(defaultJsBundle);
		}

		private static void IncludeCssFiles(StyleBundle cssBundle)
		{
			cssBundle.Include("~/Assets/CSS/apihelp.css");
			cssBundle.Include("~/Assets/CSS/htmldiff.css");
			cssBundle.Include("~/Assets/CSS/jquery-ui-bootstrap.css");
			cssBundle.Include("~/Assets/CSS/jquery.fileupload.css");
			cssBundle.Include("~/Assets/CSS/roadkill.css");
			cssBundle.Include("~/Assets/CSS/tagmanager.css");
			cssBundle.Include("~/Assets/CSS/toastr.css");
		}

		private static void IncludeJQueryScripts(ScriptBundle jsBundle)
		{
			jsBundle.Include("~/Assets/Scripts/jquery/additional-methods.js");
			jsBundle.Include("~/node_modules/jquery/dist/jquery.js");
			jsBundle.Include("~/node_modules/jquery-ui-dist/jquery-ui.min.js");
			jsBundle.Include("~/Assets/Scripts/jquery/jquery.fieldSelection.js");
			jsBundle.Include("~/Assets/Scripts/jquery/jquery.fileupload.js");
			jsBundle.Include("~/Assets/Scripts/jquery/jquery.form-extensions.js");
			jsBundle.Include("~/Assets/Scripts/jquery/jquery.iframe-transport.js");
			jsBundle.Include("~/Assets/Scripts/jquery/jquery.timeago.js");
			jsBundle.Include("~/Assets/Scripts/jquery/jquery.validate.js");
		}

		private static void IncludeRoadkillScripts(ScriptBundle jsBundle)
		{
			jsBundle.Include("~/Assets/Scripts/roadkill/dialogs.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/setup.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/validation.js");

			jsBundle.Include("~/Assets/Scripts/roadkill/editpage/editpage.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/editpage/wysiwygeditor.js");

			jsBundle.Include("~/Assets/Scripts/roadkill/filemanager/ajaxrequest.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/filemanager/breadcrumbtrail.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/filemanager/buttonevents.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/filemanager/htmlbuilder.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/filemanager/setup.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/filemanager/tableevents.js");
			jsBundle.Include("~/Assets/Scripts/roadkill/filemanager/util.js");

			jsBundle.Include("~/Assets/Scripts/roadkill/sitesettings/settings.js");
		}

		private static void IncludeSharedScripts(ScriptBundle jsBundle)
		{
			jsBundle.Include("~/Assets/Scripts/shared/bootbox.js");
			jsBundle.Include("~/Assets/Scripts/shared/head.js");
			jsBundle.Include("~/Assets/Scripts/shared/tagmanager.js");
			jsBundle.Include("~/Assets/Scripts/shared/toastr.js");
		}
	}

    /// <summary>
    /// For adding cache-busting query param to bundled js files
    /// </summary>
    public class QueryParamBundleTransform: IBundleTransform
    {
        /// <summary>
        /// Transforms script bundle, adding a cache-busting query param
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        public void Process(BundleContext context, BundleResponse response)
        {
            string queryGuid = Guid.NewGuid().ToString();

            foreach(var file in response.Files)
            {
                file.IncludedVirtualPath = string.Concat(file.IncludedVirtualPath, "?v=", queryGuid);
            }
        }
    }
}
