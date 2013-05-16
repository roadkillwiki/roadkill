/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Site.FileManager
{
	export class ButtonEvents
	{
		private _htmlBuilder: HtmlBuilder;
		private _ajaxRequest: AjaxRequest;

		constructor()
		{
			this._htmlBuilder = new HtmlBuilder();
			this._ajaxRequest = new AjaxRequest();
		}

		bind()
		{
			$("#addfolderbtn").on("click", this.addFolderInput);
			$("#deletefolderbtn").bind("click", this.deleteFolder);
			$("#deletefilebtn").bind("click", this.deleteFile);
			$("#newfolderinput").live("keyup", this.addNewFolder);
			$("#newfoldercancel").live("click", this.cancelNewFolder);
		}

		deleteFolder()
		{
			var folder: string = TableEvents.getCurrentPath();

			if (folder == "")
			{
				alert(ROADKILL_DELETE_BASEFOLDER_ERROR);
				return;
			}

			var message: string = Util.FormatString(ROADKILL_DELETE_CONFIRM, folder);
			if (!confirm(message))
				return;

			// Ajax request
			var success = function (data)
			{
				if (data.status == "ok")
				{
					BreadCrumbTrail.removePriorBreadcrumb();

					var li = $("ul.navigator li:last-child").prev("li");
					var folder: string = li.attr("data-urlpath");
					TableEvents.update(folder);
				}
				else
				{
					alert(data.message);
				}
			};
			this._ajaxRequest.deleteFolder(folder, success);
		}

		deleteFile()
		{
			var tr = $("tr.select");

			if (tr.length > 0 && tr.attr("data-itemtype") == "file")
			{
				var currentPath: string = TableEvents.getCurrentPath();
				var filename: string = $("td.file", tr).text();

				var message = Util.FormatString(ROADKILL_DELETE_CONFIRM,
												currentPath + "/" + filename);
				if (!confirm(message))
					return;

				// Ajax request
				var success = function (data)
				{
					if (data.status == "ok")
						$(tr).remove();
					else
						alert(data.message);
				};
				this._ajaxRequest.deleteFile(filename, currentPath, success);
			}
		}

		addFolderInput()
		{
			if ($("tr#newfolderrow").length > 0)
			{
				$("#newfolderinput").focus();
				return;
			}

			var tr = $("table#files tr[data-itemtype=folder]");
			var newfolderHtml = this._htmlBuilder.getNewFolder();

			if (tr.length > 0)
			{
				tr = tr.last();
				$(newfolderHtml).insertAfter(tr);
			}
			else
			{
				$("table#files").append(newfolderHtml);
			}
			$("#newfolderinput").focus();
		}

		addNewFolder(event)
		{
			if (event.which == 0 || event.which == 27)
			{
				this.cancelNewFolder();
			}
			else if (event.which == 13)
				{
				var newFolder: string = $("#newfolderinput").val();

				if (newFolder.replace(/\s/g, "").length == 0)
				{
					this.cancelNewFolder();
					return;
				}

				var success = function (data)
				{
					if (data.status == "error")
					{
						alert(data.message);
						return;
					}

					var item = $("ul.navigator li:last-child");
					this.getFolderInfo(item.attr("data-urlpath"));

					BreadCrumbTrail.removeLastItem();

					$("tr#newfolderrow").remove();
				};
				this._ajaxRequest.newFolder(TableEvents.getCurrentPath(), newFolder, success);
			}
		}

		cancelNewFolder()
		{
			$("tr#newfolderrow").remove();
		}
	}
}