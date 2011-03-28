using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	public class ManagerBase
	{
		protected IQueryable<Page> Pages
		{
			get
			{
				return NHibernateRepository.Current.Queryable<Page>();
			}
		}

		protected IQueryable<PageContent> PageContents
		{
			get
			{
				return NHibernateRepository.Current.Queryable<PageContent>();
			}
		}
	}
}
