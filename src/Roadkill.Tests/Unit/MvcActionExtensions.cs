using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using NUnit.Framework;

namespace Roadkill.Tests.Unit
{
	public static class MvcActionExtensions
	{
		public static T ModelFromActionResult<T>(this ActionResult actionResult)
		{
			// Taken from Stackoverflow
			object model;
			if (actionResult.GetType() == typeof(ViewResult))
			{
				ViewResult viewResult = (ViewResult)actionResult;
				model = viewResult.Model;
			}
			else if (actionResult.GetType() == typeof(PartialViewResult))
			{
				PartialViewResult partialViewResult = (PartialViewResult)actionResult;
				model = partialViewResult.Model;
			}
			else
			{
				throw new InvalidOperationException(string.Format("Actionresult of type {0} is not supported by ModelFromResult extractor.", actionResult.GetType()));
			}

			T typedModel = (T)model;
			return typedModel;
		}

		public static void AssertHttpPostOnly<T>(this T controller, Expression<Action<T>> action) where T : Controller
		{
			Type type = controller.GetType();
			MethodCallExpression body = action.Body as MethodCallExpression;
			MethodInfo actionMethod = body.Method;

			HttpPostAttribute postAttribute = actionMethod.GetCustomAttributes(typeof(HttpPostAttribute), false)
							 .Cast<HttpPostAttribute>()
							 .SingleOrDefault();

			HttpGetAttribute getAttribute = actionMethod.GetCustomAttributes(typeof(HttpGetAttribute), false)
							 .Cast<HttpGetAttribute>()
							 .SingleOrDefault();

			Assert.That(postAttribute != null && getAttribute == null,
				"{0}.{1} does not have [HttpPost] attribute", controller.GetType().Name, actionMethod.Name);
		}

		public static T AssertResultIs<T>(this ActionResult result) where T : ActionResult
		{
			T newResult = result as T;
			if (newResult == null)
				Assert.Fail("Expected '{0}' but was '{1}'", typeof(T).Name, result.GetType().Name);

			return newResult;
		}

		public static void AssertActionRouteIs(this RedirectToRouteResult result, string value)
		{
			Assert.That(result.RouteValues["action"], Is.EqualTo(value), "'{0}' for action route value failed:", value);
		}

		public static void AssertControllerRouteIs(this RedirectToRouteResult result, string value)
		{
			Assert.That(result.RouteValues["controller"], Is.EqualTo(value), "'{0}' for controller route value failed:", value);
		}
	}
}
