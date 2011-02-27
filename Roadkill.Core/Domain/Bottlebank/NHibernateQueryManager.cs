using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using System.Reflection;
using NHibernate.Criterion;

namespace BottleBank
{
	/// <summary>
	/// A class that provides helpers methods for simple NHibernate actions. This class isn't intended
	/// as a replacement for the Criteria/HQL api, but for common tasks.
	/// </summary>
	/// <remarks>The class allows you to pass a SessionFactory to it, or properties to build one with.
	/// All queries are performed using the Unit of Work approach to ISessions. Once the query is
	/// executed, the session is closed and disposed.</remarks>
	/// <typeparam name="T">An object that has been mapped using a Nhibernate mapping file.</typeparam>
	public class NHibernateQueryManager
	{
		#region Fields
		private string _idName;
		private ISessionFactory _sessionFactory;
		private bool _disposeSessions;
		#endregion

		#region Properties
		/// <summary>
		/// Gets/sets the name of the ID column used for queries. This is 'Id' by default.
		/// </summary>
		public string IdName
		{
			get { return _idName; }
			set { _idName = value; }
		}

		/// <summary>
		/// The NHibernate <see cref="ISessionFactory"></see> used for operations.
		/// </summary>
		public ISessionFactory SessionFactory
		{
			get { return _sessionFactory; }
			private set { _sessionFactory = value; }
		}

		/// <summary>
		/// Whether sessions are disposed of before each method returns. This is true by default.
		/// </summary>
		public bool DisposeSessions
		{
			get { return _disposeSessions; }
			set { _disposeSessions = value; }
		}
		#endregion

		#region Ctors
		/// <summary>
		/// Initializes a new instance of the <see cref="NHibernateManager">NHibernateManager</see> class.
		/// </summary>
		/// <param name="type">Used to load Configuration settings from the type's assembly.</param>
		/// <remarks>
		/// The NHibernate Session factory is automatically configured in this constructor, using application settings.
		/// Your configuration settings will be discovered by NHibernate from a "hibernate.cfg.xml" file or your app/web.config file.
		/// </remarks>
		public NHibernateQueryManager(Type type)
		{
			Configuration configuration = new Configuration();
			configuration.Configure();
			configuration.AddAssembly(type.Assembly);
			SessionFactory = configuration.BuildSessionFactory();

			_disposeSessions = true;
			_idName = "Id";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NHibernateManager">NHibernateManager</see> class.
		/// </summary>
		/// <param name="disposeSessions">Whether sessions are disposed of before each method returns.</param>
		/// <remarks>
		/// The NHibernate Session factory is automatically configured in this constructor, using application settings.
		/// Your configuration settings will be discovered by NHibernate from a "hibernate.cfg.xml" file or your app/web.config file.
		/// </remarks>
		public NHibernateQueryManager(bool disposeSessions)
		{
			_disposeSessions = disposeSessions;
			_idName = "Id";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NHibernateManager">NHibernateManager</see> class,
		/// setting the name of the ID column for the object.
		/// </summary>
		/// <param name="idName">The name of the Id column for the object.</param>
		public NHibernateQueryManager(string idName)
		{
			_disposeSessions = true;
			_idName = idName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NHibernateManager">NHibernateManager</see> class
		/// with the provided NHibernate Session factory.
		/// </summary>
		/// <param name="factory">The NHibernate ISessionFactory which has previously been parsed and built.</param>
		public NHibernateQueryManager(ISessionFactory factory)
		{
			SessionFactory = factory;

			_disposeSessions = true;
			_idName = "Id";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NHibernateManager">NHibernateManager</see> class, configuration
		/// a ISessionFactory with the provided properties.
		/// </summary>
		/// <param name="properties">A Dictionary with the properties to pass when building the ISessionFactory.</param>
		/// <remarks>
		/// This overload is useful if you are using a class library that does not contain the configuration settings.
		/// </remarks>
		public NHibernateQueryManager(Dictionary<string, string> properties)
		{
			Configuration configuration = new Configuration();
			configuration.SetProperties(properties);
			SessionFactory = configuration.BuildSessionFactory();

			_disposeSessions = true;
			_idName = "Id";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NHibernateManager">NHibernateManager</see> class, using the provided
		/// NHibernate Session factory and name for the Id column of the object.
		/// </summary>
		/// <param name="factory">The NHibernate ISessionFactory which has previously been parsed and built.</param>
		/// <param name="idName">The name of the ID column for the object.</param>
		public NHibernateQueryManager(ISessionFactory factory, string idName)
			: this(idName)
		{
			SessionFactory = factory;
		}
		#endregion

		#region Query methods
		/// <summary>
		/// Retrieves the object from database using the provided Guid as the lookup.
		/// </summary>
		/// <param name="id">The id to search with (the primary key/id column)</param>
		/// <returns>The object (if found) otherwise null.</returns>
		public T Read<T>(Guid id) where T : class
		{
			ISession session = SessionFactory.OpenSession();

			ICriteria criteria = session.CreateCriteria(typeof(T));
			criteria.Add(Expression.Eq(IdName, id));

			if (DisposeSessions)
			{
				var result = criteria.UniqueResult<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.UniqueResult<T>();
			}
		}

		/// <summary>
		/// Retrieves the object from database using the provided integer id as the lookup.
		/// </summary>
		/// <param name="id">The id to search with (the primary key/id column)</param>
		/// <returns>The object (if found) otherwise null.</returns>
		public T Read<T>(int id) where T : class
		{
			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));
			criteria.Add(Expression.Eq(IdName, id));

			if (DisposeSessions)
			{
				var result = criteria.UniqueResult<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.UniqueResult<T>();
			}
		}

		/// <summary>
		/// Retrieves the object from database using the provided id as the lookup.
		/// </summary>
		/// <param name="id">The id to search with (the primary key/id column)</param>
		/// <typeparam name="TKey">The type of the Id column</typeparam>
		/// <returns>The object (if found) otherwise null.</returns>
		public T Read<T, TKey>(TKey id) where T : class
		{
			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));
			criteria.Add(Expression.Eq(IdName, id));

			if (DisposeSessions)
			{
				var result = criteria.UniqueResult<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.UniqueResult<T>();
			}
		}

		/// <summary>
		/// Retrieves the first item from the database.
		/// </summary>
		/// <returns>The object specified by T.</returns>
		public T First<T>() where T : class
		{
			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));
			criteria.SetMaxResults(1);

			if (DisposeSessions)
			{
				var result = criteria.UniqueResult<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.UniqueResult<T>();
			}
		}

		/// <summary>
		/// Retrieves the first item from the database.
		/// </summary>
		/// <returns>The object specified by T.</returns>
		public T First<T>(params object[] filters) where T : class
		{
			if (filters == null || filters.Length == 0)
				throw new ArgumentException("The filters parameter is null or zero length for List()");
			if (filters.Length % 2 != 0)
				throw new ArgumentException("Mismatch on the number of filters for List() - use Name/Value.");

			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));

			for (int i = 0; i < filters.Length; i += 2)
			{
				string name = filters[i].ToString();
				object value = filters[i + 1];

				// Support @Property syntax though it's not really needed,
				// it makes the filter easier to read.
				if (name.StartsWith("@"))
					name = name.Remove(0, 1);

				criteria.Add(Expression.Eq(name, value));
			}

			criteria.SetMaxResults(1);

			if (DisposeSessions)
			{
				var result = criteria.UniqueResult<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.UniqueResult<T>();
			}
		}

		/// <summary>
		/// Retrieves a paged list of objects.
		/// </summary>
		/// <param name="page">The page to start from - this should be zero based.</param>
		/// <param name="pageSize">The number of results per page.</param>
		/// <returns>An <see cref="IList">IList</see> of results containing the pageSize amount of items.</returns>
		public IList<T> Page<T>(int page, int pageSize) where T : class
		{
			ISession session = SessionFactory.OpenSession();

			// NB this does nothing in SQL2000Dialect, so use SQL2005Dialect in the config
			ICriteria criteria = session.CreateCriteria(typeof(T));
			criteria.SetFirstResult(page * pageSize);
			criteria.SetMaxResults(pageSize + 1);

			if (DisposeSessions)
			{
				var result = criteria.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.List<T>();
			}
		}

		/// <summary>
		/// Retrieves an ordered, paged list of objects.
		/// </summary>
		/// <param name="page">The page to start from - this should be zero based.</param>
		/// <param name="pageSize">The number of results per page.</param>
		/// <param name="orderBy">The column to order the results with (in ascending order)</param>
		/// <returns>An <see cref="IList">IList</see> of results containing the pageSize amount of items.</returns>
		public IList<T> Page<T>(int page, int pageSize, string orderBy)
		{
			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));
			criteria.SetFirstResult(page * pageSize);
			criteria.SetMaxResults(pageSize + 1);
			criteria.AddOrder(Order.Asc(orderBy));

			if (DisposeSessions)
			{
				var result = criteria.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.List<T>();
			}
		}

		/// <summary>
		/// Executes the provided HQL query.
		/// </summary>
		/// <param name="hql">The HQL string</param>
		/// <returns>An <see cref="IList">IList</see> of objects.</returns>
		public IList<T> Query<T>(string hql) where T : class
		{
			ISession session = SessionFactory.OpenSession();
			IQuery query = session.CreateQuery(hql);

			if (DisposeSessions)
			{
				var result = query.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return query.List<T>();
			}
		}

		/// <summary>
		/// Executes the provided HQL query with named parameters.
		/// </summary>
		/// <param name="hql">The HQL string</param>
		/// <param name="parameters">A list of parameters and the values. These should appear in the HQL as ":paramname",
		/// but the parameter name should not include the ":".</param>
		/// <returns>An <see cref="IList">IList</see> of objects.</returns>
		public IList<T> Query<T>(string hql, params object[] parameters) where T : class
		{
			if (parameters.Length % 2 != 0)
				throw new ArgumentException("Mismatch on the number of parameters for Query() - use Name,Value.");

			ISession session = SessionFactory.OpenSession();
			IQuery query = session.CreateQuery(hql);
			for (int i = 0; i < parameters.Length; i += 2)
			{
				string name = parameters[i].ToString();
				object value = parameters[i + 1];

				query.SetParameter(name, value);
			}

			if (DisposeSessions)
			{
				var result = query.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return query.List<T>();
			}
		}

		/// <summary>
		/// Executes an <see cref="ICriteria">ICriteria</see> query using the provided criteria.
		/// </summary>
		/// <param name="criteria">An array of <see cref="ICriterion">ICriterion</see> to filter with.</param>
		/// <returns>An <see cref="IList">IList</see> of objects.</returns>
		/// <example>
		/// Below is example using a dummy object called User, with a Name and Website property. By default
		/// the expressions will be put together using an AND.
		/// <code>
		/// <![CDATA[
		/// using NHibernate.Criterion;
		/// ...
		/// NHibernateManager<User> manager = new NHibernateManager<User>();
		/// IList<Name> list = manager.List(Expression.Eq("Name", "bob"), Expression.Like("Website", "google.com"));
		/// ]]>
		/// </code>
		/// </example>
		public IList<T> List<T>(params ICriterion[] criteria) where T : class
		{
			ISession session = SessionFactory.OpenSession();
			ICriteria sessionCriteria = session.CreateCriteria(typeof(T));

			for (int i = 0; i < criteria.Length; i++)
			{
				sessionCriteria.Add(criteria[i]);
			}

			if (DisposeSessions)
			{
				var result = sessionCriteria.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return sessionCriteria.List<T>();
			}
		}

		/// <summary>
		/// Return a list of all objects.
		/// </summary>
		/// <returns>An <see cref="IList">IList</see> of objects.</returns>
		public IList<T> List<T>() where T : class
		{
			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));

			if (DisposeSessions)
			{
				var result = criteria.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.List<T>();
			}
		}

		/// <summary>
		/// Return an ordered list of all objects, ordered using the provided property.
		/// </summary>
		/// <param name="orderBy">The property name to order by (this is case sensitive)</param>
		/// <returns>An ordered <see cref="IList">IList</see> of objects.</returns>
		/// <remarks>This returns the list using ascending order.</remarks>
		public IList<T> OrderedList<T>(string orderBy) where T : class
		{
			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));
			criteria.AddOrder(Order.Asc(orderBy));

			if (DisposeSessions)
			{
				var result = criteria.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.List<T>();
			}
		}

		/// <summary>
		/// Return a descending ordered list of all objects, ordered using the provided property.
		/// </summary>
		/// <param name="orderBy">The property name to order by (this is case sensitive)</param>
		/// <returns>An ordered <see cref="IList">IList</see> of objects.</returns>
		public IList<T> DescendingList<T>(string orderBy) where T : class
		{
			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));
			criteria.AddOrder(Order.Desc(orderBy));

			if (DisposeSessions)
			{
				var result = criteria.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.List<T>();
			}
		}

		/// <summary>
		/// Return an ordered list of all objects, using the provided property names.
		/// </summary>
		/// <param name="orderBy">The property names to order by (these are case sensitive)</param>
		/// <returns>An <see cref="IList">IList</see> of objects.</returns>
		/// <remarks>This returns the list using ascending order.</remarks>
		public IList<T> OrderedList<T>(params string[] orderBy) where T : class
		{
			return OrderedList<T>(true, orderBy);
		}

		/// <summary>
		/// Return an ordered list of all objects, using the provided property names and whether
		/// to use ascending or descending order.
		/// </summary>
		/// <param name="orderBy">The property names to order by (these are case sensitive)</param>
		/// <param name="ascending">True for ascending order, false otherwise.</param>
		/// <returns>An <see cref="IList">IList</see> of objects.</returns>
		public IList<T> OrderedList<T>(bool ascending, params string[] orderBy) where T : class
		{
			if (orderBy == null || orderBy.Length == 0)
				throw new ArgumentException("The orderBy parameter is null or zero length for OrderedList()");

			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));

			for (int i = 0; i < orderBy.Length; i++)
			{
				if (ascending)
					criteria.AddOrder(Order.Asc(orderBy[i]));
				else
					criteria.AddOrder(Order.Desc(orderBy[i]));
			}

			if (DisposeSessions)
			{
				var result = criteria.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.List<T>();
			}
		}

		/// <summary>
		/// Returns a list using the provided filter.
		/// </summary>
		/// <param name="orderBy">The property names and values to filter with</param>
		/// <returns>An <see cref="IList">IList</see> of objects.</returns>
		/// <example>
		/// Below is example using a dummy object called User, with a Name and Website property. The
		/// query is done by using an AND with the property names. The @ syntax is optional for the property names
		/// but clarifies which are the names and which are the values in the list.
		/// <code>
		/// <![CDATA[
		/// using NHibernate.Criterion;
		/// ...
		/// NHibernateManager<User> manager = new NHibernateManager<User>();
		/// IList<Name> list = manager.List("@Name", "bob","@Website", "google.com");
		/// ]]>
		/// </code>
		/// </example>
		public IList<T> List<T>(params object[] filters) where T : class
		{
			if (filters == null || filters.Length == 0)
				throw new ArgumentException("The filters parameter is null or zero length for List()");
			if (filters.Length % 2 != 0)
				throw new ArgumentException("Mismatch on the number of filters for List() - use Name/Value.");

			ISession session = SessionFactory.OpenSession();
			ICriteria criteria = session.CreateCriteria(typeof(T));

			for (int i = 0; i < filters.Length; i += 2)
			{
				string name = filters[i].ToString();
				object value = filters[i + 1];

				// Support @Property syntax though it's not really needed,
				// it makes the filter easier to read.
				if (name.StartsWith("@"))
					name = name.Remove(0, 1);

				criteria.Add(Expression.Eq(name, value));
			}

			if (DisposeSessions)
			{
				var result = criteria.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.List<T>();
			}
		}

		/// <summary>
		/// Returns a list using the provided filters, "OR"'d together.
		/// </summary>
		/// <param name="orderBy">The property names and values to filter with</param>
		/// <returns>An <see cref="IList">IList</see> of objects.</returns>
		/// <example>
		/// Below is example using a dummy object called User, finding all Users with the name bob or brian. The  @ syntax 
		/// is optional for the property names but clarifies which are the names and which are the values in the list.
		/// <code>
		/// <![CDATA[
		/// using NHibernate.Criterion;
		/// ...
		/// NHibernateManager<User> manager = new NHibernateManager<User>();
		/// IList<Name> list = manager.OrList("@Name", "bob","@Name", "brian");
		/// ]]>
		/// </code>
		/// </example>
		public IList<T> OrList<T>(params object[] filters) where T : class
		{
			if (filters == null || filters.Length == 0)
				throw new ArgumentException("The filters parameter is null or zero length for List()");

			ISession session = SessionFactory.OpenSession();
			List<SimpleExpression> expressionList = new List<SimpleExpression>();
			ICriteria criteria = session.CreateCriteria(typeof(T));

			for (int i = 0; i < filters.Length; i += 2)
			{
				string name = filters[i].ToString();
				object value = filters[i + 1];

				// Same applies as List
				if (name.StartsWith("@"))
					name = name.Remove(0, 1);

				expressionList.Add(Expression.Eq(name, value));
			}

			// Use a disjunction to join with an OR
			Disjunction disjunction = new Disjunction();
			foreach (SimpleExpression expression in expressionList)
			{
				disjunction.Add(expression);
			}

			// Add it + exec
			criteria.Add(disjunction);

			if (DisposeSessions)
			{
				var result = criteria.List<T>();
				session.Dispose();
				return result;
			}
			else
			{
				return criteria.List<T>();
			}
		}

		/// <summary>
		/// Returns the number of objects in the database.
		/// </summary>
		/// <returns>The number of objects.</returns>
		/// <remarks>NHibernate returns this is a bigint or long (64 bit) integer.</remarks>
		public long Count<T>() where T : class
		{
			string className = typeof(T).FullName;
			ISession session = SessionFactory.OpenSession();
			string hql = string.Format("SELECT Count(*) FROM {0}", className);
			IQuery query = session.CreateQuery(hql);

			// Count in HQL returns a long.

			if (DisposeSessions)
			{
				var result = query.UniqueResult<long>();
				session.Dispose();
				return result;
			}
			else
			{
				return query.UniqueResult<long>();
			}
		}
		#endregion

		#region Delete/save
		/// <summary>
		/// Deletes alls objects from the database.
		/// </summary>
		public void DeleteAll<T>() where T : class
		{
			string className = typeof(T).FullName;
			ISession session = SessionFactory.OpenSession();
			using (session.BeginTransaction()) // 2.1 uses transactions by default
			{
				// TODO: use ClassExtractor for a more intelligent way
				session.CreateQuery(string.Format("DELETE {0} o", className)).ExecuteUpdate();
				session.Transaction.Commit();
			}

			if (DisposeSessions)
				session.Dispose();
		}

		/// <summary>
		/// Deletes the object from the database.
		/// </summary>
		/// <param name="obj">The object to delete.</param>
		public void Delete<T>(T obj) where T : class
		{
			ISession session = SessionFactory.OpenSession();
			using (session.BeginTransaction())
			{
				session.Delete(obj);
				session.Transaction.Commit();
			}

			if (DisposeSessions)
				session.Dispose();
		}

		/// <summary>
		/// Inserts or updates the object depending on whether it exists in the database.
		/// </summary>
		/// <param name="obj">The object to insert/update.</param>
		public void SaveOrUpdate<T>(T obj) where T : class
		{
			ISession session = SessionFactory.OpenSession();
			using (session.BeginTransaction())
			{
				session.SaveOrUpdate(obj);
				session.Transaction.Commit();
			}

			if (DisposeSessions)
				session.Dispose();
		}
		#endregion
	}
}