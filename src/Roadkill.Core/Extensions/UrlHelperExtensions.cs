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
			if (!relativePath.StartsWith("~"))
				relativePath = "~/Assets/CSS/" + relativePath;

			return MvcHtmlString.Create("<link href=\"" + helper.Content(relativePath) + "\" rel=\"stylesheet\" type=\"text/css\" />");
		}

		/// <summary>
		/// Provides a Javascript script tag for the Javascript file provided. If the relative path does not begin with ~ then
		/// the Assets/Scripts folder is assumed.
		/// </summary>
		public static MvcHtmlString ScriptLink(this UrlHelper helper, string relativePath)
		{
			if (!relativePath.StartsWith("~"))
				relativePath = "~/Assets/Scripts/" + relativePath;

			return MvcHtmlString.Create("<script type=\"text/javascript\" language=\"javascript\" src=\"" + helper.Content(relativePath) + "\"></script>");
		}

		/// <summary>
		/// Provides a Javascript script tag for the installer Javascript file provided, using ~/Assets/Scripts/roadkill/installer as the base path.
		/// </summary>
		public static MvcHtmlString InstallerScriptLink(this UrlHelper helper, string filename)
		{
			string relativePath = "~/Assets/Scripts/roadkill/installer/" + filename;
			return MvcHtmlString.Create("<script type=\"text/javascript\" language=\"javascript\" src=\"" + helper.Content(relativePath) + "\"></script>");
		}

		/// <summary>
		/// Provides a CSS tag for the Bootstrap framework.
		/// </summary>
		public static MvcHtmlString BootstrapCSS(this UrlHelper helper)
		{
			return MvcHtmlString.Create("<link href=\"" + helper.Content("~/Assets/bootstrap/css/bootstrap.min.css") + "\" rel=\"stylesheet\" type=\"text/css\" />");
		}

		/// <summary>
		/// Provides a Javascript script tag for the Bootstrap framework.
		/// </summary>
		public static MvcHtmlString BootstrapJS(this UrlHelper helper)
		{
			return MvcHtmlString.Create("<script type=\"text/javascript\" language=\"javascript\" src=\"" + helper.Content("~/Assets/bootstrap/js/bootstrap.min.js") + "\"></script>");
		}

		/// <summary>
		/// Returns the script link for the JS bundle
		/// </summary>
		public static MvcHtmlString JsBundle(this UrlHelper helper)
		{
			StringBuilder builder = new StringBuilder();
			string mainJs = Scripts.Render("~/Assets/Scripts/" + Bundles.JsFilename).ToHtmlString();
			mainJs = mainJs.Replace("\r\n", ""); // remove them, the lines are done in the view
			builder.AppendLine(mainJs);

			string jsVars = "";
			jsVars = ScriptLink(helper, "~/home/globaljsvars?version=" + ApplicationSettings.ProductVersion).ToHtmlString();
			jsVars = jsVars.Replace("\r\n", "");
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
