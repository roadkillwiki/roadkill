/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Site.FileManager
{
	export class AjaxRequest 
	{
		public getFolderInfo(path: string, successFunction : (data: DirectorySummary) => void)
		{
			var url = ROADKILL_FILEMANAGERURL + "/folderinfo";
			var data = { dir: path };
			var errorMessage = "folderInfo failed";

			this.makeAjaxRequest(url, data, errorMessage, successFunction);
		}

		public deleteFolder(folder: string, successFunction: (data: any) => void)
		{
			var url = ROADKILL_FILEMANAGERURL + "/deletefolder";
			var data = { folder: folder };
			var errorMessage = "deleteFolder failed";

			this.makeAjaxRequest(url, data, errorMessage, successFunction);
		}

		public deleteFile(fileName: string, filePath: string, successFunction: (data: any) => void)
		{
			var url = ROADKILL_FILEMANAGERURL + "/deletefile";
			var data = { filename: fileName, filepath: filePath };
			var errorMessage = "deleteFile failed";

			this.makeAjaxRequest(url, data, errorMessage, successFunction);
		}

		public newFolder(currentPath: string, newFolder: string, successFunction: (data: any) => void )
		{
			var url = ROADKILL_FILEMANAGERURL + "/newFolder";
			var data = { currentFolderPath: currentPath, newFolderName: newFolder };
			var errorMessage = "newFolder failed";

			this.makeAjaxRequest(url, data, errorMessage, successFunction);
		}

		private makeAjaxRequest(url: string, data: any, errorMessage: string, successFunction: (data: any) => void)
		{
			var request = $.ajax({
				type: "POST",
				url: url,
				data: data,
				dataType: "json"
			});

			request.done(successFunction);

			request.fail(function (jqXHR, textStatus, errorThrown)
			{
				alert(errorMessage)
			});

			request.always(function ()
			{
				//...
			});
		}
	}
}