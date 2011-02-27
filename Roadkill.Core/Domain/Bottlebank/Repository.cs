using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace BottleBank
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Repository<T, TRepo> : IRepository<T, TRepo> 
		where T : NHibernateObject<T, TRepo>
		where TRepo : class,IRepository<T, TRepo>,new()
	{
		/// <summary>
		/// Saves the or update.
		/// </summary>
		/// <param name="entity">The entity.</param>
		public virtual void SaveOrUpdate(T entity)
		{
			Manager().SaveOrUpdate<T>(entity);
		}

		/// <summary>
		/// Deletes the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		public virtual void Delete(T entity)
		{
			Manager().Delete<T>(entity);
		}

		/// <summary>
		/// Lists this instance.
		/// </summary>
		/// <returns></returns>
		public virtual IList<T> List()
		{
			return Manager().List<T>();
		}

		/// <summary>
		/// Lists this instance.
		/// </summary>
		/// <param name="filters"></param>
		/// <returns></returns>
		public virtual IList<T> List(params object[] filters)
		{
			return Manager().List<T>(filters);
		}

		/// <summary>
		/// Lists this instance.
		/// </summary>
		/// <returns></returns>
		public virtual T Read(Guid id)
		{
			return Manager().Read<T>(id);
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual T First()
		{
			return Manager().First<T>();
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual T First(params string[] filters)
		{
			return Manager().First<T>(filters);
		}

		/// <summary>
		/// Ordereds the list.
		/// </summary>
		/// <param name="ascending">if set to <c>true</c> [ascending].</param>
		/// <param name="orderBy">The order by.</param>
		/// <returns></returns>
		public virtual IList<T> OrderedList(bool ascending, params string[] orderBy)
		{
			return Manager().OrderedList<T>(ascending, orderBy);
		}

		/// <summary>
		/// Counts this instance.
		/// </summary>
		/// <returns></returns>
		public virtual long Count()
		{
			return Manager().Count<T>();
		}

		/// <summary>
		/// Queries the specified HQL.
		/// </summary>
		/// <param name="hql">The HQL.</param>
		/// <returns></returns>
		public virtual IList<T> Query(string hql)
		{
			return Manager().Query<T>(hql);
		}

		/// <summary>
		/// Queries the specified HQL.
		/// </summary>
		/// <param name="hql">The HQL.</param>
		/// <returns></returns>
		public virtual IList<T> Query(string hql, params object[] parameters)
		{
			return Manager().Query<T>(hql, parameters);
		}

		/// <summary>
		/// Pages the specified page.
		/// </summary>
		/// <param name="page">The page.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="orderBy">The order by.</param>
		/// <returns></returns>
		public virtual IList<T> Page(int page, int pageSize, string orderBy)
		{
			return Manager().Page<T>(page, pageSize, orderBy);
		}

		/// <summary>
		/// Managers this instance.
		/// </summary>
		/// <returns></returns>
		public NHibernateQueryManager Manager()
		{
			NHibernateQueryManager manager = new NHibernateQueryManager(NHibernateManager.Current.SessionFactory);
			manager.DisposeSessions = false;
			return manager;
		}
	}
}
