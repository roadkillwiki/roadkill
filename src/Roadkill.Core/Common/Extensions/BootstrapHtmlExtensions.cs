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
		public static MvcHtmlString BootstrapTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help)
		{
			return htmlHelper.TextBoxFor(expression, new { @class = "form-control", rel = "popover", data_content = help, autocomplete = "off" });
		}


		public static MvcHtmlString BootstrapPasswordFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help)
		{
			return htmlHelper.PasswordFor(expression, new { @class = "form-control", rel = "popover", data_content = help, autocomplete = "off" });
		}

		public static MvcHtmlString BootstrapLongTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help)
		{
			return htmlHelper.TextBoxFor(expression, new { @class = "form-control longtextbox", rel = "popover", data_content = help, autocomplete = "off" });
		}

		public static MvcHtmlString BootstrapLongTextBox(this HtmlHelper htmlHelper, string name, string help)
		{
			return htmlHelper.TextBox(name, null, new { @class = "form-control longtextbox", rel = "popover", data_content = help, autocomplete = "off" });
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
	}
}
