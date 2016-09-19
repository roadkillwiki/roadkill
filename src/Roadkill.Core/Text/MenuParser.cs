using System.Web;
using System.Web.Mvc;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Localization;

namespace Roadkill.Core.Text
{
	public class MenuParser
	{
		private static readonly string CATEGORIES_TOKEN = "%categories%";
		private static readonly string ALLPAGES_TOKEN = "%allpages%";
		private static readonly string MAINPAGE_TOKEN = "%mainpage%";
		private static readonly string NEWPAGE_TOKEN = "%newpage%";
		private static readonly string MANAGEFILES_TOKEN = "%managefiles%";
		private static readonly string SITESETTINGS_TOKEN = "%sitesettings%";

		private readonly ISettingsRepository _settingsRepository;
		private readonly SiteCache _siteCache;
		private readonly MarkupConverter _markupConverter;
		private readonly IUserContext _userContext;

		public MenuParser(MarkupConverter markupConverter, ISettingsRepository settingsRepository, SiteCache siteCache, IUserContext userContext)
		{
			_markupConverter = markupConverter;
			_settingsRepository = settingsRepository;
			_siteCache = siteCache;
			_userContext = userContext;
		}

		public string GetMenu()
		{
			string html = "";

			if (_userContext.IsLoggedIn)
			{
				if (_userContext.IsAdmin)
				{
					html = _siteCache.GetAdminMenu();
				}
				else
				{
					html = _siteCache.GetLoggedInMenu();
				}
			}
			else
			{
				html = _siteCache.GetMenu();
			}

			// If the cache is empty, populate the right menu option
			if (string.IsNullOrEmpty(html))
			{
				SiteSettings siteSettings = _settingsRepository.GetSiteSettings();
				html = siteSettings.MenuMarkup;

				html = _markupConverter.ParseMenuMarkup(html);
				html = ReplaceKnownTokens(html);

				if (_userContext.IsLoggedIn)
				{
					if (_userContext.IsAdmin)
					{
						_siteCache.AddAdminMenu(html);
					}
					else
					{
						_siteCache.AddLoggedInMenu(html);
					}
				}
				else
				{
					_siteCache.AddMenu(html);
				}
			}

			return html;
		}

		/// <summary>
		/// Support for the following tokens:
		/// 
		/// [Categories]
		/// [AllPages]
		/// [MainPage]
		/// [NewPage]
		/// [ManageFiles]
		/// [SiteSettings]
		/// </summary>
		private string ReplaceKnownTokens(string html)
		{
			if (string.IsNullOrEmpty(html))
				return "";

			string categories = CreateAnchorTag("/pages/alltags", SiteStrings.Navigation_Categories);
			string allPages = CreateAnchorTag("/pages/allpages", SiteStrings.Navigation_AllPages);
			string mainPage = CreateAnchorTag("/", SiteStrings.Navigation_MainPage);
			string newpage = CreateAnchorTag("/pages/new", SiteStrings.Navigation_NewPage);
			string manageFiles = CreateAnchorTag("/filemanager", SiteStrings.FileManager_Title);
			string siteSettings = CreateAnchorTag("/settings", SiteStrings.Navigation_SiteSettings);

			if (HttpContext.Current != null)
			{
				UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

				categories = CreateAnchorTag(urlHelper.Action("AllTags", "Pages"), SiteStrings.Navigation_Categories);
				allPages = CreateAnchorTag(urlHelper.Action("AllPages", "Pages"), SiteStrings.Navigation_AllPages);
				mainPage = CreateAnchorTag(urlHelper.Action("Index", "Home"), SiteStrings.Navigation_MainPage);
				newpage = CreateAnchorTag(urlHelper.Action("New", "Pages"), SiteStrings.Navigation_NewPage);
				manageFiles = CreateAnchorTag(urlHelper.Action("Index", "FileManager"), SiteStrings.FileManager_Title);
				siteSettings = CreateAnchorTag(urlHelper.Action( "Index", "Settings"), SiteStrings.Navigation_SiteSettings);
			}

			if (!_userContext.IsLoggedIn)
			{
				newpage = "";
				manageFiles = "";
			}

			if (!_userContext.IsAdmin)
			{
				siteSettings = "";
			}

			html = html.Replace(CATEGORIES_TOKEN, categories);
			html = html.Replace(ALLPAGES_TOKEN, allPages);
			html = html.Replace(MAINPAGE_TOKEN, mainPage);
			html = html.Replace(NEWPAGE_TOKEN, newpage);
			html = html.Replace(MANAGEFILES_TOKEN, manageFiles);
			html = html.Replace(SITESETTINGS_TOKEN, siteSettings);

			var parser = new HtmlParser();
			IHtmlDocument document = parser.Parse(html);

			RemoveParagraphTags(document);
			RemoveEmptyLiTags(document);
			RemoveEmptyUlTags(document);

			// Clean up newlines
			html = document.QuerySelector("body").InnerHtml;
			html = html.Trim();
			html = html.Replace("\n", "");
			html = html.Replace("\r", "");

			return html;
		}

		private static void RemoveEmptyUlTags(IHtmlDocument document)
		{
			IHtmlCollection<IElement> ulNodes = document.QuerySelectorAll("ul");
			if (ulNodes != null)
			{
				foreach (IElement element in ulNodes)
				{
					if (string.IsNullOrEmpty(element.TextContent) || string.IsNullOrEmpty(element.TextContent.Trim()) ||
					    string.IsNullOrEmpty(element.InnerHtml) || string.IsNullOrEmpty(element.InnerHtml.Trim()))
					{
						element.Remove();
					}
				}
			}
		}

		private static void RemoveEmptyLiTags(IHtmlDocument document)
		{
			IHtmlCollection<IElement> liNodes = document.QuerySelectorAll("li");
			if (liNodes != null)
			{
				foreach (IElement element in liNodes)
				{
					if (string.IsNullOrEmpty(element.TextContent) || string.IsNullOrEmpty(element.TextContent.Trim()) ||
					    string.IsNullOrEmpty(element.InnerHtml) || string.IsNullOrEmpty(element.InnerHtml.Trim()))
					{
						element.Remove();
					}
				}
			}
		}

		// Remove all paragraph tags and put their child nodes into the P tag's parent.
		private static void RemoveParagraphTags(IHtmlDocument document)
		{
			IHtmlCollection<IElement> paragraphNodes = document.QuerySelectorAll("p");
			if (paragraphNodes != null)
			{
				foreach (IElement element in paragraphNodes)
				{
					IElement parentNode = element.ParentElement;
					INodeList childNodes = element.ChildNodes;
					element.Remove();

					foreach (INode node in childNodes)
					{
						IElement childElement = node as IElement;
						if (childElement != null)
							parentNode.InnerHtml += childElement.OuterHtml;
						else
							parentNode.InnerHtml += node.TextContent;
					}
				}
			}
		}

		private string CreateAnchorTag(string link, string text)
		{
			return $"<a href=\"{link}\">{text}</a>";
		}
	}
}
