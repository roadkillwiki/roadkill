using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.Web.Management;
using System.Data.SqlClient;
using Roadkill.Core.Converters;
using Roadkill.Core.Search;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides functionality that is common through the site.
	/// </summary>
	public class HomeController : ControllerBase
    {
		/// <summary>
		/// Display the homepage/mainpage. If no page has been tagged with the 'homepage' tag,
		/// then a dummy PageSummary is put in its place.
		/// </summary>
		public ActionResult Index()
		{
			PageManager manager = new PageManager();
			PageSummary summary = manager.FindByTag("homepage").FirstOrDefault();
			if (summary == null)
			{
				summary = new PageSummary();
				summary.Title = "You have no mainpage set";
				summary.Content = "To set a main page, create a page and assign the tag 'homepage' to it.";
				summary.CreatedBy = "";
				summary.CreatedOn = DateTime.Now;
				summary.Tags = "homepage";
				summary.ModifiedOn = DateTime.Now;
				summary.ModifiedBy = "";
			}

			RoadkillContext.Current.Page = summary;
			return View(summary);
		}

		/// <summary>
		/// Returns a string containing Javascript 'constants' for the site. If the user is logged in, 
		/// additional variables are returned that are used by the edit page.
		public ActionResult GlobalJsVars()
		{
			UrlHelper helper = new UrlHelper(HttpContext.Request.RequestContext);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Format("var ROADKILL_CORESCRIPTPATH = '{0}';", helper.Content("~/Assets/Scripts/")));
			builder.AppendLine(string.Format("var ROADKILL_THEMEPATH =  '{0}/';", Url.Content(RoadkillSettings.ThemePath)));

			if (RoadkillContext.Current.IsLoggedIn)
			{
				// Edit page constants
				builder.AppendLine(string.Format("var ROADKILL_TAGAJAXURL = '{0}/';", helper.Action("AllTagsAsJson", "Pages")));
				builder.AppendLine(string.Format("var ROADKILL_PREVIEWURL = '{0}/';", helper.Action("GetPreview", "Pages")));
				builder.AppendLine(string.Format("var ROADKILL_MARKUPTYPE = '{0}';", RoadkillSettings.MarkupType));
				builder.AppendLine(string.Format("var ROADKILL_WIKIMARKUPHELP = '{0}';", helper.Action(RoadkillSettings.MarkupType + "Reference", "Help")));

				// File manager constants
				builder.AppendLine(string.Format("var ROADKILL_FILEMANAGERURL = '{0}';", helper.Action("Index", "Files")));
				builder.AppendLine(string.Format("var ROADKILL_FILETREE_URL =  '{0}/';", Url.Action("Folder","Files")));
				builder.AppendLine(string.Format("var ROADKILL_FILETREE_PATHNAME_URL =  '{0}/';", Url.Action("GetPath", "Files")));
				builder.AppendLine(string.Format("var ROADKILL_ATTACHMENTSPATH = '{0}';", Url.Content(RoadkillSettings.AttachmentsFolder)));

				// Tokens for the edit toolbar
				MarkupConverter converter = new MarkupConverter();
				IParser parser = converter.Parser;
				builder.AppendLine(string.Format("var ROADKILL_EDIT_BOLD_TOKEN = \"{0}\";", parser.BoldToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_ITALIC_TOKEN = \"{0}\";", parser.ItalicToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_UNDERLINE_TOKEN = \"{0}\";", parser.UnderlineToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_LINK_STARTTOKEN = \"{0}\";", parser.LinkStartToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_LINK_ENDTOKEN = \"{0}\";", parser.LinkEndToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_IMAGE_STARTTOKEN = \"{0}\";", parser.ImageStartToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_IMAGE_ENDTOKEN = \"{0}\";", parser.ImageEndToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_NUMBERLIST_TOKEN = \"{0}\";", parser.NumberedListToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_BULLETLIST_TOKEN = \"{0}\";", parser.BulletListToken));
				builder.AppendLine(string.Format("var ROADKILL_EDIT_HEADING_TOKEN = \"{0}\";", parser.HeadingToken));

				if (RoadkillContext.Current.IsAdmin)
				{
					// User management constants for the settings->user page
					builder.AppendLine(string.Format("var ROADKILL_ADDADMIN_FORMACTION = \"{0}\";", helper.Action("AddAdmin","Settings")));
					builder.AppendLine(string.Format("var ROADKILL_ADDEDITOR_FORMACTION = \"{0}\";", helper.Action("AddEditor","Settings")));
					builder.AppendLine(string.Format("var ROADKILL_EDITUSER_FORMACTION = \"{0}\";", helper.Action("EditUser","Settings")));
				}
			}

			return Content(builder.ToString(), "text/javascript");
		}
    }
}
