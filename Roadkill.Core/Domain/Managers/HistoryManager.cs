using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides a way of viewing, and comparing the version history of page content, and reverting to previous versions.
	/// </summary>
	public class HistoryManager : ServiceBase
	{
		private MarkupConverter _markupConverter;

		public HistoryManager(IConfigurationContainer configuration, IRepository repository, MarkupConverter markupConverter)
			: base(configuration, repository)
		{
			_markupConverter = markupConverter;
		}

		/// <summary>
		/// Retrieves all history for a page.
		/// </summary>
		/// <param name="pageId">The id of the page to get the history for.</param>
		/// <returns>An <see cref="IEnumerable`HistorySummary"/> ordered by the most recent version number.</returns>
		/// <exception cref="HistoryException">An NHibernate (database) error occurred while retrieving the list.</exception>
		public IEnumerable<HistorySummary> GetHistory(int pageId)
		{
			try
			{
				IEnumerable<PageContent> contentList = Repository.PageContents.Where(p => p.Page.Id == pageId);
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
			catch (HibernateException ex)
			{
				throw new HistoryException(ex, "A HibernateException occurred getting the history for page id {0}", pageId);
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

				PageContent mainContent = Repository.PageContents.FirstOrDefault(p => p.Id == mainVersionId);
				versions.Add(mainContent.ToSummary(_markupConverter));

				if (mainContent.VersionNumber == 1)
				{
					versions.Add(null);
				}
				else
				{
					PageContent previousContent = Repository.PageContents.FirstOrDefault(p => p.Page.Id == mainContent.Page.Id && p.VersionNumber == mainContent.VersionNumber - 1);

					if (previousContent == null)
					{
						versions.Add(null);
					}
					else
					{
						versions.Add(previousContent.ToSummary(_markupConverter));
					}
				}

				return versions;
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred comparing the version history for version id {0}", mainVersionId);
			}
			catch (HibernateException ex)
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
				PageContent pageContent = Repository.PageContents.FirstOrDefault(p => p.Page.Id == pageId && p.VersionNumber == versionNumber);

				if (pageContent != null)
				{
					RevertTo(pageContent.Id);
				}
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred when reverting to version number {0} for page id {1}", versionNumber, pageId);
			}
			catch (HibernateException ex)
			{
				throw new HistoryException(ex, "A HibernateException occurred when reverting to version number {0} for page id {1}", versionNumber, pageId);
			}
		}

		/// <summary>
		/// Reverts to a particular version, creating a new version in the process.
		/// </summary>
		/// <param name="versionNumber">The version ID to revert to.</param>
		/// <exception cref="HistoryException">An NHibernate (database) error occurred while reverting to the version.</exception>
		public void RevertTo(Guid versionId)
		{
			try
			{
				string currentUser = RoadkillContext.Current.CurrentUsername;

				PageContent versionContent = Repository.PageContents.FirstOrDefault(p => p.Id == versionId);
				Page page = Repository.Pages.FirstOrDefault(p => p.Id == versionContent.Page.Id);

				PageContent pageContent = new PageContent();
				pageContent.VersionNumber = MaxVersion(page.Id) + 1;
				pageContent.Text = versionContent.Text;
				pageContent.EditedBy = currentUser;
				pageContent.EditedOn = DateTime.Now;
				pageContent.Page = page;
				Repository.SaveOrUpdate<PageContent>(pageContent);
			}
			catch (ArgumentNullException ex)
			{
				throw new HistoryException(ex, "An ArgumentNullException occurred when reverting to version ID {0}", versionId);
			}
			catch (HibernateException ex)
			{
				throw new HistoryException(ex, "A HibernateException occurred when reverting to version ID {0}", versionId);
			}
		}

		/// <summary>
		/// Retrieves the latest version number for a page.
		/// </summary>
		/// <param name="pageId">The id of the page to get the version number for.</param>
		/// <returns>The latest version number.</returns>
		public int MaxVersion(int pageId)
		{
			return Repository.PageContents.Where(p => p.Page.Id == pageId).Max(p => p.VersionNumber);
		}
	}
}
