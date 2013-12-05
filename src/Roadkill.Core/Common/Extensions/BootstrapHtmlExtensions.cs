using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Roadkill.Core
{
	/// <summary>
	/// Extension methods that spit out Bootstrap class="" into the elements.
	/// </summary>
	public static class BootstrapHtmlExtensions
	{
		public static MvcHtmlString BootstrapTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false)
		{
			return htmlHelper.TextBoxFor(expression, GetHtmlAttributes(help, autoCompleteOff));
		}

		public static MvcHtmlString BootstrapPasswordFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false)
		{
			return htmlHelper.PasswordFor(expression, GetHtmlAttributes(help, autoCompleteOff));
		}

		public static MvcHtmlString BootstrapPassword(this HtmlHelper htmlHelper, string name, string help, bool autoCompleteOff = false)
		{
			return htmlHelper.Password(name, null, GetHtmlAttributes(help, autoCompleteOff));
		}

		public static MvcHtmlString BootstrapLongTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false)
		{
			return htmlHelper.TextBoxFor(expression, GetHtmlAttributes(help, autoCompleteOff));
		}

		public static MvcHtmlString BootstrapLongTextBox(this HtmlHelper htmlHelper, string name, string help, bool autoCompleteOff = false)
		{
			return htmlHelper.TextBox(name, null, GetHtmlAttributes(help, autoCompleteOff, " longtextbox"));
		}

		public static MvcHtmlString BootstrapDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string help)
		{
			return htmlHelper.DropDownListFor(expression, selectList, new { @class = "form-control", rel = "popover", data_content = help });
		}

		public static MvcHtmlString BootstrapCheckBoxFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, string help)
		{
			return htmlHelper.CheckBoxFor(expression, new { rel = "popover", data_content = help });
		}

		public static MvcHtmlString BootstrapTextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help)
		{
			return htmlHelper.TextAreaFor(expression, new { @class = "form-control", rel = "popover", data_content = help });
		}

		public static MvcHtmlString BootstrapValidationSummary(this HtmlHelper htmlHelper, string message)
		{
			return htmlHelper.ValidationSummary(message, new { @class = "alert alert-block alert-danger fade in", data_dismiss = "alert" });
		}

		private static object GetHtmlAttributes(string help, bool autoCompleteOff, string additionalCssClass = "")
		{
			if (autoCompleteOff)
				return new { @class = "form-control" + additionalCssClass, rel = "popover", data_content = help, autocomplete = "off" };
			else
				return new { @class = "form-control" + additionalCssClass, rel = "popover", data_content = help };
		}
	}
}
