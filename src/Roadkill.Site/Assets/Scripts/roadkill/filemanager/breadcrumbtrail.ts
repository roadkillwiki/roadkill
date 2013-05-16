/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Site.FileManager
{
	export class BreadCrumbTrail
	{
		public static removeLastItem()
		{
			var item = $("ul.navigator li:last-child");
			var level: number = item.attr("data-level");

			if (level == 0)
				$("ul.navigator li").remove();
			else
				$("ul.navigator li:gt(" + (level - 1) + ")").remove();
		}

		public static removePriorBreadcrumb()
		{
			var count: number = $("ul.navigator li").length;
			if (count == 1) // cannot delete base attachments directory
				return;

			var li = $("ul.navigator li:last-child").prev("li");
			var level: number = li.attr("data-level");

			this.removeLastItem(level);
		}

		public static addNewItem(data : DirectorySummary)
		{
			var htmlBuilder = new HtmlBuilder();
			var count: number = $("ul.navigator li").length;
			var breadCrumbHtml = htmlBuilder.getBreadCrumb(data, count);

			$("ul.navigator").append(breadCrumbHtml);
		}
	}
}