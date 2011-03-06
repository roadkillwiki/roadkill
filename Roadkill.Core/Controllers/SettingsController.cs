using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.IO;
using System.Web.Configuration;
using System.Configuration;

namespace Roadkill.Core.Controllers
{
	public class SettingsController : ControllerBase
    {
		/// <summary>
		/// TODO
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult CreateSchema()
		{
			Page.Configure(RoadkillSettings.ConnectionString, true);
			return RedirectToAction("Index", "Home");
		}

		public ActionResult GlobalJsVars()
		{
			UrlHelper helper = new UrlHelper(HttpContext.Request.RequestContext);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Format("var ROADKILL_CORESCRIPTPATH = '{0}';", helper.Content("~/Assets/Scripts/")));

			if (RoadkillContext.Current.IsLoggedIn)
			{
				builder.AppendLine(string.Format("var ROADKILL_FILEMANAGERURL = '{0}';", helper.Content("~/Page/AllFiles/")));
				builder.AppendLine(string.Format("var ROADKILL_TAGAJAXURL = '{0}';", helper.Content("~/Page/AllTags/")));
				builder.AppendLine(string.Format("var ROADKILL_PREVIEWURL = '{0}';", helper.Action("GetPreview", "Page")));
				builder.AppendLine(string.Format("var ROADKILL_MARKUPTYPE = '{0}';", RoadkillSettings.MarkupType));
				builder.AppendLine(string.Format("var ROADKILL_THEMEPATH =  '{0}';", Url.Content(RoadkillSettings.ThemePath)));
				builder.AppendLine(string.Format("var ROADKILL_ATTACHMENTSPATH = '{0}';", Url.Content(RoadkillSettings.AttachmentsFolder)));
			}

			return Content(builder.ToString(), "text/javascript");
		}

		[RoadkillAuthorize(Roles = "Admins")]
		public ActionResult Export()
		{
			PageManager manager = new PageManager();
			string xml = manager.ExportToXml();

			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(xml);
			writer.Flush();
			stream.Position = 0;

			FileStreamResult result = new FileStreamResult(stream, "text/xml");
			result.FileDownloadName = "roadkill-export.xml";

			return result;
		}

		[RoadkillAuthorize(Roles = "Admins")]
		public ActionResult ExportContent()
		{
			PageManager manager = new PageManager();
			IEnumerable<PageSummary> pages = manager.AllPages();

			StringBuilder builder = new StringBuilder();
			foreach (PageSummary summary in pages)
			{
				string filePath = AppDomain.CurrentDomain.BaseDirectory + summary.Title.AsValidPath() + ".wiki";
				string content = "Tags:" + summary.Tags.SpaceDelimitTags() +"\r\n"+summary.Content;

				System.IO.File.WriteAllText(filePath,content);
				builder.AppendFormat("Written {0}<br/>",filePath);
			}

			return Content(builder.ToString());
		}
    }
}
