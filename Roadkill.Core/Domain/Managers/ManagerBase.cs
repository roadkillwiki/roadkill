using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides all inheriting classes with queryable objects for the system pages and text content.
	/// </summary>
	public class ManagerBase
	{
		/// <summary>
		/// Gets a LINQ-to-NHibernate <see cref="Queryable`Page`"/> object to perform queries with.
		/// </summary>
		protected IQueryable<Page> Pages
		{
			get
			{
				return NHibernateRepository.Current.Queryable<Page>();
			}
		}

		/// <summary>
		/// Gets a LINQ-to-NHibernate <see cref="Queryable`PageContent`"/> object to perform queries with.
		/// </summary>
		protected IQueryable<PageContent> PageContents
		{
			get
			{
				return NHibernateRepository.Current.Queryable<PageContent>();
			}
		}

		/// <summary>
		/// Gets a LINQ-to-NHibernate <see cref="Queryable`User`"/> object to perform queries with.
		/// </summary>
		protected IQueryable<User> Users
		{
			get
			{
				return NHibernateRepository.Current.Queryable<User>();
			}
		}
	}
}
