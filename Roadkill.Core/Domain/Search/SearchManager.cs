using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace Roadkill.Core.Domain.Search
{
	public class SearchManager
	{
		public IEnumerable<PageSummary> Search(string text)
		{
			IQuery query = PageContent.Repository.Manager().SessionFactory.OpenSession()
				.CreateQuery("FROM Page WHERE Title LIKE :search");

			query.SetString("search", "%" + text + ";%");
			IList<Page> pages = query.List<Page>();
			List<PageSummary> list = new List<PageSummary>();

			foreach (Page page in pages)
			{
				list.Add(page.ToSummary());
			}

			return list;
		}
	}
}
