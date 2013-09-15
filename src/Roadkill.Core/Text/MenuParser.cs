using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Localization.Resx;

namespace Roadkill.Core.Text
{
	internal class MenuParser
	{
		private static readonly string CATEGORIES_TOKEN = "%categories%";
		private static readonly string ALLPAGES_TOKEN = "%allpages%";
		private static readonly string MAINPAGE_TOKEN = "%mainpage%";
		private static readonly string NEWPAGE_TOKEN = "%newpage%";
		private static readonly string MANAGEFILES_TOKEN = "%managefiles%";
		private static readonly string SITESETTINGS_TOKEN = "%sitesettings%";

		private IRepository _repository;
		private ListCache _listCache;
		private MarkupConverter _markupConverter;
		private IUserContext _userContext;

		public MenuParser(MarkupConverter markupConverter, IRepository repository, ListCache listCache, IUserContext userContext)
		{
			_markupConverter = markupConverter;
			_repository = repository;
			_listCache = listCache;
			_userContext = userContext;
		}

		public string GetMenu()
		{
			string html = "";

			if (_userContext.IsLoggedIn)
			{
				if (_userContext.IsAdmin)
				{
					html = _listCache.GetAdminMenu();
				}
				else
				{
					html = _listCache.GetLoggedInMenu();
				}
			}
			else
			{
				html = _listCache.GetMenu();
			}

			// If the cache is empty, populate the right menu option
			if (string.IsNullOrEmpty(html))
			{
				SiteSettings siteSettings = _repository.GetSiteSettings();
				html = siteSettings.MenuMarkup;

				html = _markupConverter.ParseMenuHtml(html);
				html = ReplaceKnownTokens(html);

				if (_userContext.IsLoggedIn)
				{
					if (_userContext.IsAdmin)
					{
						_listCache.AddAdminMenu(html);
					}
					else
					{
						_listCache.AddLoggedInMenu(html);
					}
				}
				else
				{
					_listCache.AddMenu(html);
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
		private string ReplaceKnownTokens(string markup)
		{
			string html = markup;

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

			// Very basic empty tag cleanup:
			// - (markdown)
			html = html.Replace("<li></li>\n", "");
			html = html.Replace("<ul>\n</ul>\n", "");	

			// - (creole)
			html = html.Replace("<li> </li>\n", "");
			html = html.Replace("<ul>\n</ul>\n", "");
			html = html.Replace("<p></p>\n", "");
			html = html.Replace("<p></p>", "");

			return html;
		}

		private string CreateAnchorTag(string link, string text)
		{
			return string.Format("<a href=\"{0}\">{1}</a>", link, text);
		}
	}
}
