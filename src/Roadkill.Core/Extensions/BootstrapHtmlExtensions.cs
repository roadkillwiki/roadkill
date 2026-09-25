using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Roadkill.Core.Extensions
{
	/// <summary>
	/// Extension methods that spit out Bootstrap class="" into the elements.
	/// </summary>
	public static class BootstrapHtmlExtensions
	{
		public static IHtmlContent BootstrapTextBoxFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.TextBoxFor(expression, GetHtmlAttributes(help, autoCompleteOff, tabIndex));
		}

		public static IHtmlContent BootstrapPasswordFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.PasswordFor(expression, GetHtmlAttributes(help, autoCompleteOff, tabIndex));
		}

		public static IHtmlContent BootstrapPassword(this IHtmlHelper htmlHelper, string name, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.Password(name, null, GetHtmlAttributes(help, autoCompleteOff, tabIndex));
		}

		public static IHtmlContent BootstrapLongTextBoxFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.TextBoxFor(expression, GetHtmlAttributes(help, autoCompleteOff, tabIndex));
		}

		public static IHtmlContent BootstrapLongTextBox(this IHtmlHelper htmlHelper, string name, string help, bool autoCompleteOff = false, int tabIndex = 0)
		{
			return htmlHelper.TextBox(name, null, GetHtmlAttributes(help, autoCompleteOff, tabIndex, " longtextbox"));
		}

		public static IHtmlContent BootstrapDropDownListFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string help, int tabIndex = 0)
		{
			return htmlHelper.DropDownListFor(expression, selectList, new { @class = "form-control", rel = "popover", data_content = help, tabindex = tabIndex });
		}

		public static IHtmlContent BootstrapCheckBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, string help, int tabIndex = 0)
		{
			return htmlHelper.CheckBoxFor(expression, new { rel = "popover", data_content = help, tabindex = tabIndex });
		}

		public static IHtmlContent BootstrapTextAreaFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, int tabIndex = 0)
		{
			return htmlHelper.TextAreaFor(expression, new { @class = "form-control", rel = "popover", data_content = help, tabindex = tabIndex });
		}

		public static IHtmlContent BootstrapValidationSummary(this IHtmlHelper htmlHelper, string message)
		{
			// ASP.NET Core always renders the summary (hidden by a "validation-summary-valid" class), which the Bootstrap alert
			// classes make visible: like MVC 5, render nothing when there are no errors.
			if (htmlHelper.ViewData.ModelState.IsValid)
				return HtmlString.Empty;

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
