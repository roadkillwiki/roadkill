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
using ControllerBase = Roadkill.Core.Mvc.Controllers.ControllerBase;
using Roadkill.Core.Attachments;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core
{
	/// <summary>
	/// Roadkill specific extensions methods for the <see cref="HtmlHelper"/> class.
	/// </summary>
	public static class HtmlExtensions
	{
		/// <summary>
		/// Turns a tag string into a HTML tag cloud, which produces different spans based on the
		/// number of pages with each tag in the system.
		/// </summary>
		/// <param name="content">The tags in ";" delimited format.</param>
		/// <returns>A HTML tag cloud.</returns>
		public static MvcHtmlString TagBlocks(this HtmlHelper helper, IEnumerable<string> tags)
		{
			string result = "";

			StringBuilder builder = new StringBuilder();
			foreach (string item in tags)
			{
				if (!string.IsNullOrWhiteSpace(item))
				{
					string url = helper.ActionLink(item, "Tag", "Pages", new { id = item },null).ToString();
					builder.AppendFormat("<span class=\"tagblock\">{0}</span>", url);
				}
			}

			result = builder.ToString();

			return MvcHtmlString.Create(result);
		}

		/// <summary>
		/// Removes all bad characters (ones which cannot be used in a URL for a page) from a page title.
		/// </summary>
		/// <param name="title">The page title.</param>
		/// <returns>The cleaned title</returns>
		public static string EncodeTitle(this string title)
		{
			return EncodeTitleInternal(title);
		}

		/// <summary>
		/// Removes all bad characters (one which cannot be used in a URL for a page) from a page title.
		/// </summary>
		/// <param name="title">The page title.</param>
		/// <returns>The cleaned title</returns>
		public static MvcHtmlString EncodeTitle(this UrlHelper helper, string title)
		{
			title = EncodeTitleInternal(title);
			return MvcHtmlString.Create(title);
		}

		private static string EncodeTitleInternal(string title)
		{
			if (string.IsNullOrEmpty(title))
				return title;

			// Search engine friendly slug routine with help from http://www.intrepidstudios.com/blog/2009/2/10/function-to-generate-a-url-friendly-string.aspx
			
			// remove invalid characters
			title = Regex.Replace(title, @"[^\w\d\s-]", "");  // this is unicode safe, but may need to revert back to 'a-zA-Z0-9', need to check spec
			
			// convert multiple spaces/hyphens into one space       
			title = Regex.Replace(title, @"[\s-]+", " ").Trim(); 
			
			// If it's over 30 chars, take the first 30.
			title = title.Substring(0, title.Length <= 75 ? title.Length : 75).Trim(); 
			
			// hyphenate spaces
			title = Regex.Replace(title, @"\s", "-");

			return title;
		}

		/// <summary>
		/// Gets a CSS class name for the tag based on the <see cref="TagSummary.Count"/> - the number of
		/// pages with that tag in the system.
		/// </summary>
		/// <param name="helper">The helper.</param>
		/// <param name="tag">A <see cref="TagSummary"/>.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item>1 tag: "tagcloud1"</item>
		/// <item>1-3 tags: "tagcloud2"</item>
		/// <item>3-5 tags: "tagcloud3"</item>
		/// <item>5-10 tags: "tagcloud4"</item>
		/// <item>10+ tags: "tagcloud5"</item>
		/// </list>
		/// </returns>
		public static string ClassNameForTagSummary(this HtmlHelper helper, TagSummary tag)
		{
			string className = "";

			if (tag.Count > 10)
			{
				className = "tagcloud5";
			}
			else if (tag.Count >= 5 && tag.Count < 10)
			{
				className = "tagcloud4";
			}
			else if (tag.Count >= 3 && tag.Count < 5)
			{
				className = "tagcloud3";
			}
			else if (tag.Count > 1 && tag.Count < 3)
			{
				className = "tagcloud2";
			}
			else if (tag.Count == 1)
			{
				className = "tagcloud1";
			}

			return className;
		}

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
		/// Formats a number in bytes using KB or bytes if it is less than 1024 bytes.
		/// </summary>
		/// <param name="helper">The helper.</param>
		/// <param name="size">The size in bytes.</param>
		/// <returns>If the size parameter is 900: 900 bytes. If size is 4000: 4KB.</returns>
		public static string FormatFileSize(this HtmlHelper helper,int size)
		{
			if (size > 1024)
				return size / 1024 + "KB";
			else
				return size + " bytes";
		}

		/// <summary>
		/// Renders the Recaptcha control as HTML, if recaptcha is enabled.
		/// </summary>
		public static MvcHtmlString RenderCaptcha(this HtmlHelper helper, SiteSettings siteSettings)
		{
			ControllerBase controller = helper.ViewContext.Controller as ControllerBase;
			if (controller != null && siteSettings.IsRecaptchaEnabled)
			{
				RecaptchaControl control = new RecaptchaControl();
				control.ID = "recaptcha";
				control.Theme = "clean";
				control.PublicKey = siteSettings.RecaptchaPublicKey;
				control.PrivateKey = siteSettings.RecaptchaPrivateKey;

				using (StringWriter stringWriter = new StringWriter())
				{
					using (HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter))
					{
						stringWriter.WriteLine("<br/><label class=\"userlabel\">As an anti-spam measure, please enter the two words below</label><br style=\"clear:both\"><br/>");
						control.RenderControl(htmlWriter);
						stringWriter.WriteLine("<br/>");
						
						return MvcHtmlString.Create(htmlWriter.InnerWriter.ToString());
					}
				}
			}
			else
			{
				return MvcHtmlString.Empty;
			}
		}

		/// <summary>
		/// Returns the Javascript to dynamically resize images, if the setting is enabled.
		/// </summary>
		public static MvcHtmlString ResizeImagesScript(this HtmlHelper helper)
		{
			ControllerBase controller = helper.ViewContext.Controller as ControllerBase;
			if (controller != null && controller.ApplicationSettings.ResizeImages)
			{
				return MvcHtmlString.Create(@"<script type=""text/javascript"">
			$(document).ready(function ()
			{
				// Resize all images
				$(""#pagecontent img"").each(function()
				{
					resizeImage($(this));
				});
			});
		</script>");
			}
			else
			{
				return MvcHtmlString.Empty;
			}
		}

		/// <summary>
		/// Gets the full path for the attachments folder, including any extra application paths from the url.
		/// </summary>
		public static MvcHtmlString GetAttachmentsPath(this UrlHelper helper, ApplicationSettings settings)
		{
			return MvcHtmlString.Create(settings.AttachmentsUrlPath);
		}

		/// <summary>
		/// Gets a IEnumerable{SelectListItem} from a the SettingsSummary.DatabaseTypesAvailable, as a default
		/// SelectList doesn't add option value attributes.
		/// </summary>
		public static IEnumerable<SelectListItem> DatabaseTypesAvailable(this HtmlHelper helper, SettingsSummary summary)
		{
			List<SelectListItem> items =  new List<SelectListItem>();

			foreach (string name in summary.DatabaseTypesAvailable)
			{
				SelectListItem item =  new SelectListItem();
				item.Text = name;
				item.Value = name;

				if (name == summary.DataStoreTypeName)
					item.Selected = true;

				items.Add(item);
			}

			return items;
		}

		/// <summary>
		/// Creates a drop down list from an <c>IDictionary</c> and selects the item.
		/// </summary>
		public static MvcHtmlString DropDownBox(this HtmlHelper helper, string name, IDictionary<string, string> items, string selectedValue)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (string key in items.Keys)
			{
				SelectListItem selectListItem = new SelectListItem
				{
					Text = items[key],
					Value = key
				};

				if (key == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList);
		}

		/// <summary>
		/// Creates a button for the items per page in the logviewer.
		/// </summary>
		public static MvcHtmlString ItemsPerPageButtonForLog(this HtmlHelper helper, int value)
		{
			int maxItems = Convert.ToInt32(helper.ViewData["maxItems"]);
			string extraCss = (maxItems == value) ? " btn-primary" : "";

			if (value > 0)
				return MvcHtmlString.Create("<input type=\"submit\"class=\"btn-mini" +extraCss+ "\" name=\"maxItems\" value=\"" +value+ "\" />");
			else
				return MvcHtmlString.Create("<input type=\"submit\"class=\"btn-mini" + extraCss + "\" name=\"maxItems\" value=\"All\" />");
		}

		/// <summary>
		/// An alias for Partial() to indicate a dialog's HTML is being rendered.
		/// </summary>
		public static MvcHtmlString DialogPartial(this HtmlHelper helper, string viewName)
		{
			return helper.Partial(viewName);
		}

		/// <summary>
		/// An alias for Partial() to indicate a dialog's HTML is being rendered.
		/// </summary>
		public static MvcHtmlString DialogPartial(this HtmlHelper helper, string viewName, object model)
		{
			return helper.Partial(viewName, model);
		}
	}
}
