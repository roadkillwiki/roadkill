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
		public static MvcHtmlString SettingsTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help, bool required=false)
		{
			string cssClass = "form-control";
			if (required)
				cssClass += " validate[required]";

			return htmlHelper.TextBoxFor(expression, new { @class = cssClass, rel = "popover", data_content = help });
		}

		public static MvcHtmlString SettingsLongTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help)
		{
			return htmlHelper.TextBoxFor(expression, new { @class = "form-control longtextbox validate[required]", rel = "popover", data_content = help });
		}

		public static MvcHtmlString SettingsDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string help)
		{
			return htmlHelper.DropDownListFor(expression, selectList, new { @class = "form-control", rel = "popover", data_content = help });
		}

		public static MvcHtmlString SettingsCheckBoxFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, string help)
		{
			return htmlHelper.CheckBoxFor(expression, new { rel = "popover", data_content = help });
		}

		public static MvcHtmlString SettingsTextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string help)
		{
			return htmlHelper.TextAreaFor(expression, new { @class = "form-control", rel = "popover", data_content = help });
		}
	}
}
