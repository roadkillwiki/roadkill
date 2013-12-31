using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
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

		private IRepository _repository;
		private SiteCache _siteCache;
		private MarkupConverter _markupConverter;
		private IUserContext _userContext;

		public MenuParser(MarkupConverter markupConverter, IRepository repository, SiteCache siteCache, IUserContext userContext)
		{
			_markupConverter = markupConverter;
			_repository = repository;
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
				SiteSettings siteSettings = _repository.GetSiteSettings();
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

			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(html);

			//
			// Remove P tags, and empty ul and li tags
			//
			HtmlNodeCollection paragraphNodes = document.DocumentNode.SelectNodes("//p");
			if (paragraphNodes != null)
			{
				foreach (HtmlNode node in paragraphNodes)
				{
					var parentNode = node.ParentNode;
					var childNodes = node.ChildNodes;

					node.Remove();
					parentNode.AppendChildren(childNodes);
				}
			}

			HtmlNodeCollection liNodes = document.DocumentNode.SelectNodes("//li");
			if (liNodes != null)
			{
				foreach (HtmlNode node in liNodes)
				{
					if (string.IsNullOrEmpty(node.InnerText) || string.IsNullOrEmpty(node.InnerText.Trim()) ||
						string.IsNullOrEmpty(node.InnerHtml) || string.IsNullOrEmpty(node.InnerHtml.Trim()))
					{
						node.Remove();
					}
				}
			}

			HtmlNodeCollection ulNodes = document.DocumentNode.SelectNodes("//ul");
			if (ulNodes != null)
			{
				foreach (HtmlNode node in ulNodes)
				{
					if (string.IsNullOrEmpty(node.InnerText) || string.IsNullOrEmpty(node.InnerText.Trim()) ||
						string.IsNullOrEmpty(node.InnerHtml) || string.IsNullOrEmpty(node.InnerHtml.Trim()))
					{
						node.Remove();
					}
				}
			}

			// Clean up newlines
			html = document.DocumentNode.InnerHtml;
			html = html.Trim();
			html = html.Replace("\n", "");
			html = html.Replace("\r", "");

			return html;
		}

		private string CreateAnchorTag(string link, string text)
		{
			return string.Format("<a href=\"{0}\">{1}</a>", link, text);
		}
	}
}
