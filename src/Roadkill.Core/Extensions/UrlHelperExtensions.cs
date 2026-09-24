using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Core.Configuration;

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
		public static string ThemeContent(this IUrlHelper helper, string relativePath, SiteSettings settings)
		{
			return helper.Content(settings.ThemePath + "/" + relativePath);
		}

		/// <summary>
		/// Provides a CSS link tag for the CSS file provided. If the relative path does not begin with ~ then
		/// the Assets/Css folder is assumed.
		/// </summary>
		public static IHtmlContent CssLink(this IUrlHelper helper, string relativePath)
		{
			string path = relativePath;

			if (!path.StartsWith("~"))
				path = "~/Assets/CSS/" + relativePath;

			path = helper.Content(path);
			string html = $"<link href=\"{path}?version={ApplicationSettings.ProductVersion}\" rel=\"stylesheet\" type=\"text/css\" />";

			return new HtmlString(html);
		}

		/// <summary>
		/// Provides a Javascript script tag for the Javascript file provided. If the relative path does not begin with ~ then
		/// the Assets/Scripts folder is assumed.
		/// </summary>
		public static IHtmlContent ScriptLink(this IUrlHelper helper, string relativePath)
		{
			string path = relativePath;

			if (!path.StartsWith("~"))
				path = "~/Assets/Scripts/" + relativePath;

			path = helper.Content(path);
			string html = $"<script type=\"text/javascript\" language=\"javascript\" src=\"{path}?version={ApplicationSettings.ProductVersion}\"></script>";

			return new HtmlString(html);
		}

		/// <summary>
		/// Provides a Javascript script tag for the installer Javascript file provided, using ~/Assets/Scripts/roadkill/installer as the base path.
		/// </summary>
		public static IHtmlContent InstallerScriptLink(this IUrlHelper helper, string filename)
		{
			return ScriptLink(helper, "~/Assets/Scripts/roadkill/installer/" + filename);
		}

		/// <summary>
		/// Provides a CSS tag for the Bootstrap framework.
		/// </summary>
		public static IHtmlContent BootstrapCSS(this IUrlHelper helper)
		{
			return CssLink(helper, "~/Assets/bootstrap/css/bootstrap.min.css");
		}

		/// <summary>
		/// Provides a Javascript script tag for the Bootstrap framework.
		/// </summary>
		public static IHtmlContent BootstrapJS(this IUrlHelper helper)
		{
			string html = ScriptLink(helper, "~/Assets/bootstrap/js/bootstrap.min.js").ToString();
			html += "\n";

			html += ScriptLink(helper, "~/Assets/bootstrap/js/respond.min.js").ToString();

			return new HtmlString(html);
		}

		/// <summary>
		/// Returns the script link for the JS bundle
		/// </summary>
		public static IHtmlContent JsBundle(this IUrlHelper helper)
		{
			StringBuilder builder = new StringBuilder();

			builder.AppendLine(ScriptLink(helper, "~/Assets/Scripts/roadkill.min.js").ToString());
			builder.AppendLine(ScriptLink(helper, "~/home/globaljsvars").ToString());

			return new HtmlString(builder.ToString());
		}

		/// <summary>
		/// Returns the script link for the CSS bundle.
		/// </summary>
		public static IHtmlContent CssBundle(this IUrlHelper helper)
		{
			string html = CssLink(helper, "~/Assets/CSS/roadkill.css").ToString();
			return new HtmlString(html);
		}
	}
}
