/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Web.FileManager
{
	export class TableEvents
	{
		bind()
		{
			var that = this;
			$("#folder-container")
				.on("mouseenter", "tr.listrow", function () { $(this).addClass("focus"); })
				.on("mouseleave", "tr.listrow", function () { $(this).removeClass("focus"); })
				.on("click", "tr.listrow", function() { that.handleRowSelection(this); })
				.on("dblclick", "tr.listrow", function () { that.handleDoubleClickForRow(this); });
		}

		handleRowSelection(tr)
		{
			$("table#files tr.select").removeClass("select");
			$(tr).addClass("select");

			if ($(tr).attr("data-itemtype") !== "folder")
			{
				var path = TableEvents.getCurrentPath();
				if (path !== "/")
					path += "/";

				$("table#files").trigger("fileselected", {
					file: path + $("td.file", tr).text()
				});
			}
		}

		handleDoubleClickForRow(tr)
		{
			if ($(tr).attr("data-itemtype") === "folder")
			{
				TableEvents.update($(tr).attr("data-urlpath"));
			}
		}

		public static getCurrentPath(): string
		{
			return $("ul.navigator li:last").attr("data-urlpath");
		}

        public static update(path: string = "", addBreadCrumb: boolean = true)
		{
			if (path === "")
				path = TableEvents.getCurrentPath();

			var that = this;
			var success = function (data: DirectoryViewModel)
			{
				if (data.status === "error")
				{
					toastr.error(data.message);
				}
				else
				{
					if (addBreadCrumb)
						BreadCrumbTrail.addNewItem(data);

					var htmlBuilder = new HtmlBuilder();
					var tableHtml: string[] = htmlBuilder.getFolderTable(data);
					$("#folder-container").html(tableHtml.join(""));

					var currentPath = TableEvents.getCurrentPath();
					$("#destination_folder").val(currentPath);
				}
			};

			var ajaxRequest = new AjaxRequest();
			ajaxRequest.getFolderInfo(path, success)
		}
	}
}