using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Roadkill.Core.Extensions
{
	/// <summary>
	/// Extension methods that spit out Bootstrap class="" into the elements.
	/// </summary>
	public static class BootstrapHtmlExtensions
	{
		public static MvcHtmlString BootstrapTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.TextBoxFor(expression, GetHtmlAttributes(help, autoCompleteOff, tabIndex));
		}

		public static MvcHtmlString BootstrapPasswordFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.PasswordFor(expression, GetHtmlAttributes(help, autoCompleteOff, tabIndex));
		}

		public static MvcHtmlString BootstrapPassword(this HtmlHelper htmlHelper, string name, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.Password(name, null, GetHtmlAttributes(help, autoCompleteOff, tabIndex));
		}

		public static MvcHtmlString BootstrapLongTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.TextBoxFor(expression, GetHtmlAttributes(help, autoCompleteOff, tabIndex));
		}

		public static MvcHtmlString BootstrapLongTextBox(this HtmlHelper htmlHelper, string name, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.TextBox(name, null, GetHtmlAttributes(help, autoCompleteOff, tabIndex, " longtextbox"));
		}

		public static MvcHtmlString BootstrapDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string help, int tabIndex = 0)
		{
			return htmlHelper.DropDownListFor(expression, selectList, new { @class = "form-control", rel = "popover", data_content = help, tabindex = tabIndex });
		}

		public static MvcHtmlString BootstrapCheckBoxFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, string help, int tabIndex = 0)
		{
			return htmlHelper.CheckBoxFor(expression, new { rel = "popover", data_content = help, tabindex = tabIndex });
		}

		public static MvcHtmlString BootstrapTextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, int tabIndex = 0)
		{
			return htmlHelper.TextAreaFor(expression, new { @class = "form-control", rel = "popover", data_content = help, tabindex = tabIndex });
		}

		public static MvcHtmlString BootstrapValidationSummary(this HtmlHelper htmlHelper, string message)
		{
			return htmlHelper.ValidationSummary(message, new { @class = "alert alert-block alert-danger fade in", data_dismiss = "alert" });
		}

		private static object GetHtmlAttributes(string help, bool autoCompleteOff, int tabIndex, string additionalCssClass = "")
		{
			if (autoCompleteOff)
				return new { @class = "form-control" + additionalCssClass, rel = "popover", data_content = help, tabIndex = tabIndex, autocomplete = "off" };
			else
				return new { @class = "form-control" + additionalCssClass, rel = "popover", data_content = help, tabIndex = tabIndex };
		}
	}
}
