using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace Roadkill.Core
{
	public class HistoryManager
	{
		private IQueryable<Page> Pages
		{
			get
			{
				return Page.Repository.Manager().Queryable<Page>();
			}
		}

		private IQueryable<PageContent> PageContents
		{
			get
			{
				return Page.Repository.Manager().Queryable<PageContent>();
			}
		}

		public IEnumerable<HistorySummary> GetHistory(int pageId)
		{
			IEnumerable<PageContent> contentList = PageContents.Where(p => p.Page.Id == pageId);
			IEnumerable<HistorySummary> historyList = from p in contentList select
													  new HistorySummary()
													  {
															Id = p.Id,
															PageId = pageId,
															EditedBy = p.EditedBy,
															EditedOn = p.EditedOn,
															VersionNumber = p.VersionNumber
														};

			return historyList.OrderByDescending(h => h.VersionNumber);
		}

		/// <summary>
		/// Returns a IEnumerable of two version, where the 2nd index is the previous version.
		/// If the current version is 1, or a previous version cannot be found, then the 2nd
		/// index will be null.
		/// </summary>
		/// <param name="mainVersionId"></param>
		/// <returns></returns>
		public IEnumerable<PageSummary> CompareVersions(Guid mainVersionId)
		{
			List<PageSummary> versions = new List<PageSummary>();

			PageContent mainContent = PageContent.Repository.Read(mainVersionId);
			versions.Add(mainContent.Page.ToSummary(mainContent));

			if (mainContent.VersionNumber == 1)
			{
				versions.Add(null);
			}
			else
			{
				PageContent previousContent = PageContents.FirstOrDefault(p => p.Page.Id == mainContent.Page.Id && p.VersionNumber == mainContent.VersionNumber - 1);

				if (previousContent == null)
				{
					versions.Add(null);
				}
				else
				{
					versions.Add(previousContent.Page.ToSummary(previousContent));
				}
			}

			return versions;
		}

		public void RevertTo(int pageId, int versionNumber)
		{
			PageContent pageContent = PageContents.FirstOrDefault(p => p.Page.Id == pageId && p.VersionNumber == versionNumber);
			if (pageContent != null)
			{
				RevertTo(pageContent.Id);
			}
		}

		public void RevertTo(Guid versionId)
		{
			string currentUser = RoadkillContext.Current.CurrentUser;

			PageContent versionContent = PageContents.FirstOrDefault(p => p.Id == versionId);
			Page page = Pages.FirstOrDefault(p => p.Id == versionContent.Page.Id);

			PageContent pageContent = new PageContent();
			pageContent.VersionNumber = MaxVersion(page.Id) + 1;
			pageContent.Text = versionContent.Text;
			pageContent.EditedBy = currentUser;
			pageContent.EditedOn = DateTime.Now;
			pageContent.Page = page;
			PageContent.Repository.SaveOrUpdate(pageContent);
		}

		public int MaxVersion(int pageId)
		{
			return PageContents.Where(p => p.Page.Id == pageId).Max(p => p.VersionNumber);
		}
	}
}
