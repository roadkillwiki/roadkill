/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Web.FileManager
{
	export class AjaxRequest 
	{
		public getFolderInfo(path: string, successFunction : (data: DirectoryViewModel) => void)
		{
			var url = ROADKILL_FILEMANAGERURL + "/folderinfo";
			var data = { dir: path };
			var errorMessage = ROADKILL_FILEMANAGER_ERROR_DIRECTORYLISTING +" <br/>";

			this.makeAjaxRequest(url, data, errorMessage, successFunction);
		}

		public deleteFolder(folder: string, successFunction: (data: any) => void)
		{
			var url = ROADKILL_FILEMANAGERURL + "/deletefolder";
			var data = { folder: folder };
			var errorMessage = ROADKILL_FILEMANAGER_ERROR_DELETEFOLDER +" <br/>";

			this.makeAjaxRequest(url, data, errorMessage, successFunction);
		}

		public deleteFile(fileName: string, filePath: string, successFunction: (data: any) => void)
		{
			var url = ROADKILL_FILEMANAGERURL + "/deletefile";
			var data = { filename: fileName, filepath: filePath };
			var errorMessage = ROADKILL_FILEMANAGER_ERROR_DELETEFILE +" <br/>";

			this.makeAjaxRequest(url, data, errorMessage, successFunction);
		}

		public newFolder(currentPath: string, newFolder: string, successFunction: (data: any) => void )
		{
			var url = ROADKILL_FILEMANAGERURL + "/newFolder";
			var data = { currentFolderPath: currentPath, newFolderName: newFolder };
			var errorMessage = ROADKILL_FILEMANAGER_ERROR_CREATEFOLDER +" <br/>";

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
			
			request.fail(function (jqXHR, textStatus, errorThrown : SyntaxError)
			{
				// Logged out since the call was made
				if (errorThrown.message.indexOf("unexpected character") !== -1)
				{
					window.location.href = window.location.href;
				}
				else
				{
					toastr.error(errorMessage + errorThrown);
				}
			});
		}
	}
}