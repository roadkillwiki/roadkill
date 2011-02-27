using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace Roadkill.Core
{
	public class HistoryManager
	{
		public IEnumerable<HistorySummary> GetHistory(Guid pageId)
		{
			List<HistorySummary> historyList = new List<HistorySummary>();

			IList<PageContent> contentList = PageContent.Repository.List("Page.Id", pageId);
			foreach (PageContent item in contentList)
			{
				historyList.Add(new HistorySummary()
				{
					Id = item.Id,
					PageId = pageId,
					EditedBy = item.EditedBy,
					EditedOn = item.EditedOn,
					VersionNumber = item.VersionNumber
				});
			}

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
				PageContent previousContent = PageContent.Repository.List("Page.Id", mainContent.Page.Id, "VersionNumber", mainContent.VersionNumber - 1).FirstOrDefault();

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

		public void RevertTo(Guid pageId, int versionNumber)
		{
			PageContent pageContent = PageContent.Repository.List("Page.Id",pageId,"VersionNumber",versionNumber).FirstOrDefault();
			if (pageContent != null)
			{
				RevertTo(pageContent.Id);
			}
		}

		public void RevertTo(Guid versionId)
		{
			string currentUser = RoadkillContext.Current.CurrentUser;

			PageContent versionContent = PageContent.Repository.Read(versionId);
			Page page = Page.Repository.Read(versionContent.Page.Id);

			PageContent pageContent = new PageContent();
			pageContent.VersionNumber = MaxVersion(page.Id) + 1;
			pageContent.Text = versionContent.Text;
			pageContent.EditedBy = currentUser;
			pageContent.EditedOn = DateTime.Now;
			pageContent.Page = page;
			PageContent.Repository.SaveOrUpdate(pageContent);
		}

		public int MaxVersion(Guid pageId)
		{
			IQuery query = PageContent.Repository.Manager().SessionFactory.OpenSession()
				.CreateQuery("SELECT max(VersionNumber) FROM PageContent WHERE Page.Id=:Id");

			query.SetGuid("Id", pageId);
			query.SetMaxResults(1);

			return query.UniqueResult<int>();
		}
	}
}
