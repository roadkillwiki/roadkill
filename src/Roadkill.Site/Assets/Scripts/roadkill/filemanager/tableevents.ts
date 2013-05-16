/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Site.FileManager
{
	export class TableEvents
	{
		bind()
		{
			$("tr.listrow")
						.live("mouseenter", function () { $(this).addClass("focus"); })
						.live("mouseleave", function () { $(this).removeClass("focus"); })
						.live("click", function () { this.handleRowSelection(this); });
		}

		handleRowSelection(tr)
		{
			if ($(tr).attr("data-itemtype") == "folder")
			{
				TableEvents.update($(tr).attr("data-itemid"));
			}
			else
			{
				$("table#files tr.select").removeClass("select");
				$(tr).addClass("select");

				$("table#files").trigger("fileselected", {
					file: TableEvents.getCurrentPath() + "/" + $("td.file", tr).text()
				});
			}
		}

		public static getCurrentPath(): string
		{
			return $("ul.navigator li:last").attr("data-urlpath");
		}

		public static update(path: string)
		{
			var that = this;
			var success = function (data: DirectorySummary)
			{
				BreadCrumbTrail.addNewItem(data);
				var htmlBuilder = new HtmlBuilder();

				var tableHtml: string[] = htmlBuilder.getFolderTable(data);
				$("#folder-container").empty().append(tableHtml.join(""));

				var currentPath = this.getCurrentPath();
				$("#destination_folder").val(currentPath);
			};

			var ajaxRequest = new AjaxRequest();
			ajaxRequest.getFolderInfo(path, success)
		}
	}
}