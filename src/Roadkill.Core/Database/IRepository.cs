﻿using System;
using System.Linq;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using StructureMap.Attributes;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// Defines a repository for storing and retrieving Roadkill domain objects in a data store.
	/// </summary>
	public interface IRepository : IPageRepository, IUserRepository, IDisposable
	{
		void DeletePage(Page page);
		void DeletePageContent(PageContent pageContent);
		void DeleteUser(User user);
		void DeleteAllPages();
		void DeleteAllPageContent();
		void DeleteAllUsers();

		void SaveOrUpdatePage(Page page);
		PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn);
		PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version);
		void UpdatePageContent(PageContent content); // no new version
		void SaveOrUpdateUser(User user);
		void SaveSitePreferences(SitePreferences preferences);
		SitePreferences GetSitePreferences();

		void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache);
		void Install(DataStoreType dataStoreType, string connectionString, bool enableCache);
		void Test(DataStoreType dataStoreType, string connectionString);
	}
}
