using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructureMap;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides all inheriting classes with queryable objects for the system pages and text content.
	/// </summary>
	public class ManagerBase
	{
		protected IRepository Repository;

		public ManagerBase()
		{
			Repository = ObjectFactory.GetInstance<IRepository>();
		}

		/// <summary>
		/// Gets a LINQ-to-NHibernate <see cref="Queryable`Page`"/> object to perform queries with.
		/// </summary>
		internal IQueryable<Page> Pages
		{
			get
			{
				return Repository.Queryable<Page>();
			}
		}

		/// <summary>
		/// Gets a LINQ-to-NHibernate <see cref="Queryable`PageContent`"/> object to perform queries with.
		/// </summary>
		internal IQueryable<PageContent> PageContents
		{
			get
			{
				return Repository.Queryable<PageContent>();
			}
		}

		/// <summary>
		/// Gets a LINQ-to-NHibernate <see cref="Queryable`User`"/> object to perform queries with.
		/// </summary>
		internal IQueryable<User> Users
		{
			get
			{
				return Repository.Queryable<User>();
			}
		}
	}
}
