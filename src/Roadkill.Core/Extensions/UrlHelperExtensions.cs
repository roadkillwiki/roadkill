using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Roadkill.Core.Converters;
using System.Web;
using System.Text.RegularExpressions;
using Recaptcha;
using System.Web.UI;
using System.IO;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using StructureMap;
using ControllerBase = Roadkill.Core.Mvc.Controllers.ControllerBase;
using Roadkill.Core.Attachments;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Localization;
using Roadkill.Core.DI;
using System.Linq.Expressions;
using Roadkill.Core.Mvc.Controllers;
using System.Web.Optimization;
using Roadkill.Core.Mvc;

namespace Roadkill.Core.Extensions
{
	/// <summary>
	/// Roadkill specific extensions methods for the <see cref="UrlHelper"/> class.
	/// </summary>
	public static class UrlHelperExtensions
	{
		/// <summary>
		/// Gets a complete URL path to an item in the current theme directory.
		/// </summary>
		/// <param name="helper">The helper.</param>
		/// <param name="relativePath">The filename or path inside the current theme directory.</param>
		/// <returns>A url path to the item, e.g. '/MySite/Themes/Mediawiki/logo.png'</returns>
		public static string ThemeContent(this UrlHelper helper, string relativePath, SiteSettings settings)
		{
			return helper.Content(settings.ThemePath + "/" + relativePath);
		}

		/// <summary>
		/// Provides a CSS link tag for the CSS file provided. If the relative path does not begin with ~ then
		/// the Assets/Css folder is assumed.
		/// </summary>
		public static MvcHtmlString CssLink(this UrlHelper helper, string relativePath)
		{
			string path = relativePath;

			if (!path.StartsWith("~"))
				path = "~/Assets/CSS/" + relativePath;

			path = helper.Content(path);
			string html = string.Format("<link href=\"{0}?version={1}\" rel=\"stylesheet\" type=\"text/css\" />", path, ApplicationSettings.ProductVersion);

			return MvcHtmlString.Create(html);
		}

		/// <summary>
		/// Provides a Javascript script tag for the Javascript file provided. If the relative path does not begin with ~ then
		/// the Assets/Scripts folder is assumed.
		/// </summary>
		public static MvcHtmlString ScriptLink(this UrlHelper helper, string relativePath)
		{
			string path = relativePath;

			if (!path.StartsWith("~"))
				path = "~/Assets/Scripts/" + relativePath;

			path = helper.Content(path);
			string html = string.Format("<script type=\"text/javascript\" language=\"javascript\" src=\"{0}?version={1}\"></script>", path, ApplicationSettings.ProductVersion);

			return MvcHtmlString.Create(html);
		}

		/// <summary>
		/// Provides a Javascript script tag for the installer Javascript file provided, using ~/Assets/Scripts/roadkill/installer as the base path.
		/// </summary>
		public static MvcHtmlString InstallerScriptLink(this UrlHelper helper, string filename)
		{
			string path = helper.Content("~/Assets/Scripts/roadkill/installer/" + filename);
			string html = string.Format("<script type=\"text/javascript\" language=\"javascript\" src=\"{0}?version={1}\"></script>", path, ApplicationSettings.ProductVersion);

			return MvcHtmlString.Create(html);
		}

		/// <summary>
		/// Provides a CSS tag for the Bootstrap framework.
		/// </summary>
		public static MvcHtmlString BootstrapCSS(this UrlHelper helper)
		{
			string path = helper.Content("~/Assets/bootstrap/css/bootstrap.min.css");
			string html = string.Format("<link href=\"{0}?version={1}\" rel=\"stylesheet\" type=\"text/css\" />", path, ApplicationSettings.ProductVersion);
			
			return MvcHtmlString.Create(html);
		}

		/// <summary>
		/// Provides a Javascript script tag for the Bootstrap framework.
		/// </summary>
		public static MvcHtmlString BootstrapJS(this UrlHelper helper)
		{
			string path = helper.Content("~/Assets/bootstrap/js/bootstrap.min.js");
			string html = string.Format("<script type=\"text/javascript\" language=\"javascript\" src=\"{0}?version={1}\"></script>", path, ApplicationSettings.ProductVersion);
			
			return MvcHtmlString.Create(html);
		}

		/// <summary>
		/// Returns the script link for the JS bundle
		/// </summary>
		public static MvcHtmlString JsBundle(this UrlHelper helper)
		{
			StringBuilder builder = new StringBuilder();
			string mainJs = Scripts.Render("~/Assets/Scripts/" + Bundles.JsFilename).ToHtmlString();
			mainJs = mainJs.Replace("\r\n", ""); // remove these newlines, the newlines are done in the view
			builder.AppendLine(mainJs);

			string jsVars = "";
			jsVars = ScriptLink(helper, "~/home/globaljsvars").ToHtmlString();
			jsVars = jsVars.Replace("\r\n", ""); // more cleanup to tidy the HTML up
			builder.Append(jsVars, 2);

			return MvcHtmlString.Create(builder.ToString());
		}

		/// <summary>
		/// Returns the script link for the CSS bundle.
		/// </summary>
		public static MvcHtmlString CssBundle(this UrlHelper helper)
		{
			string html = Styles.Render("~/Assets/CSS/" + Bundles.CssFilename).ToHtmlString();
			html = html.Replace("\r\n", ""); // done in the view
			return MvcHtmlString.Create(html);
		}
	}
}
