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
		/// Displays the login page.
		/// </summary>
		/// <remarks>If the session times out in the file manager, then an alternative
		/// login view with no theme is displayed.</remarks>
		public ActionResult Login()
		{
			if (Request.QueryString["ReturnUrl"] != null && Request.QueryString["ReturnUrl"].Contains("Files"))
				return View("BlankLogin");

			return View();
		}

		/// <summary>
		/// Handles the login page POST, validates the login and if successful redirects to the url provided.
		/// If the login is unsucessful, the default Login view is re-displayed.
		/// </summary>
		[HttpPost]
		public ActionResult Login(string username, string password, string fromUrl)
		{
			if (UserManager.Current.Authenticate(username, password))
			{
				FormsAuthentication.SetAuthCookie(username, true);

				if (!string.IsNullOrWhiteSpace(fromUrl))
					return Redirect(fromUrl);
				else
					return RedirectToAction("Index");
			}
			else
			{
				ModelState.AddModelError("Username/Password", "The username/password are incorrect");
				return View();
			}
		}

		/// <summary>
		/// Logouts the current logged in user, and redirects to the homepage.
		/// </summary>
		public ActionResult Logout()
		{
			UserManager.Current.Logout();
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Provides a page for editing the logged in user's profile details.
		/// </summary>
		public ActionResult Profile()
		{
			if (RoadkillContext.Current.IsLoggedIn)
			{
				UserSummary summary = UserManager.Current.GetUser(RoadkillContext.Current.CurrentUser).ToSummary();
				return View(summary);
			}
			else
			{
				return RedirectToAction("Login");
			}
		}

		/// <summary>
		/// Updates the POST'd user profile details.
		/// </summary>
		[HttpPost]
		public ActionResult Profile(UserSummary summary)
		{
			if (!RoadkillContext.Current.IsLoggedIn)
				return RedirectToAction("Login");

			if (ModelState.IsValid)
			{
				try
				{
					if (!UserManager.Current.UpdateUser(summary))
					{
						ModelState.AddModelError("General", "An error occured updating your profile");

						summary.ExistingEmail = summary.NewEmail;
					}

					if (!string.IsNullOrEmpty(summary.Password))
						UserManager.Current.ChangePassword(summary.ExistingEmail, summary.Password);
				}
				catch (SecurityException e)
				{
					ModelState.AddModelError("General", e.Message);
				}
			}

			return View();
		}

		/// <summary>
		/// Searches the lucene index using the search string provided.
		/// </summary>
		public ActionResult Search(string q)
		{
			ViewData["search"] = q;

			List<SearchResult> results = SearchManager.Current.SearchIndex(q);
			return View(results);
		}

		/// <summary>
		/// Provides a page for creating a new user account. This redirects to the home page if
		/// windows authentication is enabled, or AllowUserSignup is disabled.
		/// </summary>
		public ActionResult Signup()
		{
			if (RoadkillContext.Current.IsLoggedIn || !RoadkillSettings.AllowUserSignup || RoadkillSettings.UseWindowsAuthentication)
			{
				return RedirectToAction("Index");
			}
			else
			{
				return View();
			}
		}

		/// <summary>
		/// Attempts to create the new user, sending a validation key
		/// </summary>
		[HttpPost]
		public ActionResult Signup(UserSummary summary)
		{
			if (RoadkillContext.Current.IsLoggedIn || !RoadkillSettings.AllowUserSignup || RoadkillSettings.UseWindowsAuthentication)
				return RedirectToAction("Index");

			if (ModelState.IsValid)
			{
				try
				{
					string key = UserManager.Current.Signup(summary,null);
					if (string.IsNullOrEmpty(key))
					{
						ModelState.AddModelError("General", "An error occured with the signup.");
					}
					else
					{
						// Send the confirm email
						Email.Send(summary.NewEmail, summary, new SignupEmail(summary));

						return View("SignupComplete", summary);
					}
				}
				catch (SecurityException e)
				{
					ModelState.AddModelError("General", e.Message);
				}
			}

			return View();			
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
