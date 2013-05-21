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
			var that = this;

			$(document).on("click", "#addfolderbtn",    { instance: that }, this.addFolderInput);
			$(document).on("click", "#deletefolderbtn", { instance: that }, this.deleteFolder);
			$(document).on("click", "#deletefilebtn",   { instance: that }, this.deleteFile);
			$(document).on("keyup", "#newfolderinput",  { instance: that }, this.addNewFolder);
			$(document).on("click", "#newfoldercancel", { instance: that }, this.cancelNewFolder);
		}

		deleteFolder(event)
		{
			var that = event.data.instance;  // this instance of the ButtonEvents class
			var tr = $("tr.select");
			var folder: string = tr.attr("data-urlpath");

			if (Util.IsStringNullOrEmpty(folder))
			{
				return;
			}

			var message: string = Util.FormatString(ROADKILL_FILEMANAGER_DELETE_CONFIRM, folder);
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

					toastr.info("ROADKILL_FILEMANAGER_FOLDER_DELETED_SUCCESS");
				}
				else
				{
					toastr.error(ROADKILL_FILEMANAGER_DELETE_ERROR + " :<br/>" + data.message);
				}
			};

			that._ajaxRequest.deleteFolder(folder, success);
		}

		deleteFile(event)
		{
			var that = event.data.instance;
			var tr = $("tr.select");

			if (tr.length > 0 && tr.attr("data-itemtype") == "file")
			{
				var currentPath: string = TableEvents.getCurrentPath();
				var filename: string = $("td.file", tr).text();

				var message = Util.FormatString(ROADKILL_FILEMANAGER_DELETE_CONFIRM, filename);
				if (!confirm(message))
					return;

				// Ajax request
				var success = function (data)
				{
					if (data.status == "ok")
					{
						$(tr).remove();
						toastr.info("ROADKILL_FILEMANAGER_FILE_DELETED_SUCCESS");
					}
					else
					{
						toastr.error(ROADKILL_FILEMANAGER_DELETE_ERROR + " :<br/>" + data.message);
					}
				};

				that._ajaxRequest.deleteFile(filename, currentPath, success);
			}
		}

		addFolderInput(event)
		{
			var that = event.data.instance;

			if ($("tr#newfolderrow").length > 0)
			{
				$("#newfolderinput").focus();
				return;
			}

			var tr = $("table#files tr[data-itemtype=folder]");
			var newfolderHtml = that._htmlBuilder.getNewFolder();

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
			var that = event.data.instance;

			if (event.which == 0 || event.which == 27)
			{
				that.cancelNewFolder();
			}
			else if (event.which == 13)
			{
				var newFolder: string = $("#newfolderinput").val();

				if (newFolder.replace(/\s/g, "").length == 0)
				{
					that.cancelNewFolder();
					return;
				}

				var success = function (data)
				{
					if (data.status == "error")
					{
						toastr.error(ROADKILL_FILEMANAGER_ERROR_CREATEFOLDER + ":<br/>" +data.message);
						return;
					}

					var item = $("ul.navigator li:last-child");
					TableEvents.update(item.attr("data-urlpath"));

					BreadCrumbTrail.removeLastItem();

					$("tr#newfolderrow").remove();
				};

				that._ajaxRequest.newFolder(TableEvents.getCurrentPath(), newFolder, success);
			}
		}

		cancelNewFolder()
		{
			$("tr#newfolderrow").remove();
		}
	}
}