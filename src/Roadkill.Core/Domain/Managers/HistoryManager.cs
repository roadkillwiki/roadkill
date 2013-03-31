using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides a way of viewing, and comparing the version history of page content, and reverting to previous versions.
	/// </summary>
	public class HistoryManager : ServiceBase
	{
		private MarkupConverter _markupConverter;
		private IRoadkillContext _context;
		private PageSummaryCache _pageSummaryCache;

		public HistoryManager(ApplicationSettings settings, IRepository repository, IRoadkillContext context, PageSummaryCache pageSummaryCache)
			: base(settings, repository)
		{
			_markupConverter = new MarkupConverter(settings, repository);
			_context = context;
			_pageSummaryCache = pageSummaryCache;
		}

		/// <summary>
		/// Retrieves all history for a page.
		/// </summary>
		/// <param name="pageId">The id of the page to get the history for.</param>
		/// <returns>An <see cref="IEnumerable{HistorySummary}"/> ordered by the most recent version number.</returns>
		/// <exception cref="HistoryException">An NHibernate (database) error occurred while retrieving the list.</exception>
		public IEnumerable<HistorySummary> GetHistory(int pageId)
		{
			try
			{
				IEnumerable<PageContent> contentList = Repository.FindPageContentsByPageId(pageId);
				IEnumerable<HistorySummary> historyList = from p in contentList
														  select
															  new HistorySummary()
															  {
																  Id = p.Id,
																  PageId = pageId,
																  EditedBy = p.EditedBy,
																  EditedOn = p.EditedOn,
																  VersionNumber = p.VersionNumber,
																  IsPageAdminOnly = p.Page.IsLocked
															  };

				return historyList.OrderByDescending(h => h.VersionNumber);
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred getting the history for page id {0}", pageId);
			}
			catch (DatabaseException ex)
			{
				throw new HistoryException(ex, "A DatabaseException occurred getting the history for page id {0}", pageId);
			}
		}

		/// <summary>
		/// Compares a page version to the previous version.
		/// </summary>
		/// <param name="mainVersionId">The id of the version to compare</param>
		/// <returns>Returns a IEnumerable of two versions, where the 2nd item is the previous version.
		/// If the current version is 1, or a previous version cannot be found, then the 2nd item will be null.</returns>
		/// <exception cref="HistoryException">An NHibernate (database) error occurred while comparing the two versions.</exception>
		public IEnumerable<PageSummary> CompareVersions(Guid mainVersionId)
		{
			try
			{
				List<PageSummary> versions = new List<PageSummary>();

				PageContent mainContent = Repository.GetPageContentByVersionId(mainVersionId);
				versions.Add(mainContent.ToSummary(_markupConverter));

				if (mainContent.VersionNumber == 1)
				{
					versions.Add(null);
				}
				else
				{
					PageSummary summary = _pageSummaryCache.Get(mainContent.Page.Id, mainContent.VersionNumber - 1);

					if (summary == null)
					{
						PageContent previousContent = Repository.GetPageContentByPageIdAndVersionNumber(mainContent.Page.Id, mainContent.VersionNumber - 1);
						if (previousContent == null)
						{
							summary = null;
						}
						else
						{
							summary = previousContent.ToSummary(_markupConverter);
							_pageSummaryCache.Add(mainContent.Page.Id, mainContent.VersionNumber - 1, summary);
						}
					}

					versions.Add(summary);
				}

				return versions;
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred comparing the version history for version id {0}", mainVersionId);
			}
			catch (DatabaseException ex)
			{
				throw new HistoryException(ex, "A HibernateException occurred comparing the version history for version id {0}", mainVersionId);
			}
		}

		/// <summary>
		/// Reverts a page to a particular version, creating a new version in the process.
		/// </summary>
		/// <param name="pageId">The id of the page</param>
		/// <param name="versionNumber">The version number to revert to.</param>
		/// <exception cref="HistoryException">An NHibernate (database) error occurred while reverting to the version.</exception>
		public void RevertTo(int pageId, int versionNumber)
		{
			try
			{
				PageContent pageContent = Repository.GetPageContentByPageIdAndVersionNumber(pageId, versionNumber);

				if (pageContent != null)
				{
					RevertTo(pageContent.Id, _context);
				}
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred when reverting to version number {0} for page id {1}", versionNumber, pageId);
			}
			catch (DatabaseException ex)
			{
				throw new HistoryException(ex, "A DatabaseException occurred when reverting to version number {0} for page id {1}", versionNumber, pageId);
			}
		}

		/// <summary>
		/// Reverts to a particular version, creating a new version in the process.
		/// </summary>
		/// <param name="versionId">The version ID to revert to.</param>
		/// <param name="context">The current logged in user's context.</param>
		/// <exception cref="HistoryException">An NHibernate (database) error occurred while reverting to the version.</exception>
		public void RevertTo(Guid versionId, IRoadkillContext context)
		{
			try
			{
				string currentUser = context.CurrentUsername;

				PageContent versionContent = Repository.GetPageContentByVersionId(versionId);
				Page page = Repository.GetPageById(versionContent.Page.Id);

				int versionNumber = MaxVersion(page.Id) + 1;
				string text = versionContent.Text;
				string editedBy = currentUser;
				DateTime editedOn = DateTime.Now;
				Repository.AddNewPageContentVersion(page, text, editedBy, editedOn, versionNumber);
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred when reverting to version ID {0}", versionId);
			}
			catch (DatabaseException ex)
			{
				throw new HistoryException(ex, "A DatabaseException occurred when reverting to version ID {0}", versionId);
			}
		}

		/// <summary>
		/// Retrieves the latest version number for a page.
		/// </summary>
		/// <param name="pageId">The id of the page to get the version number for.</param>
		/// <returns>The latest version number.</returns>
		public int MaxVersion(int pageId)
		{
			return Repository.GetLatestPageContent(pageId).VersionNumber;
		}
	}
}
