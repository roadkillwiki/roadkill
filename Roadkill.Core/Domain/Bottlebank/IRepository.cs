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
	public interface IRepository<T, TRepository> 
		where T : NHibernateObject<T, TRepository>
		where TRepository : class, IRepository<T, TRepository>,new()
	{
		/// <summary>
		/// Saves the or update.
		/// </summary>
		/// <param name="t">The t.</param>
		void SaveOrUpdate(T t);

		/// <summary>
		/// Deletes the specified t.
		/// </summary>
		/// <param name="t">The t.</param>
		void Delete(T t);

		/// <summary>
		/// Lists this instance.
		/// </summary>
		/// <returns></returns>
		IList<T> List();

		/// <summary>
		/// An ordered the list of the items.
		/// </summary>
		/// <param name="ascending">if set to <c>true</c> [ascending].</param>
		/// <param name="orderBy">The order by.</param>
		/// <returns></returns>
		IList<T> OrderedList(bool ascending, params string[] orderBy);

		/// <summary>
		/// Counts this instance.
		/// </summary>
		/// <returns></returns>
		long Count();

		/// <summary>
		/// Queries the specified HQL.
		/// </summary>
		/// <param name="hql">The HQL.</param>
		/// <returns></returns>
		IList<T> Query(string hql);

		/// <summary>
		/// Pages the specified page.
		/// </summary>
		/// <param name="page">The page.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="orderBy">The order by.</param>
		/// <returns></returns>
		IList<T> Page(int page, int pageSize, string orderBy);
	}
}
