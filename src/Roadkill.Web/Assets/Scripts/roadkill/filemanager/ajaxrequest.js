var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/filemanager.references.ts" />
        (function (FileManager) {
            var AjaxRequest = (function () {
                function AjaxRequest() {
                }
                AjaxRequest.prototype.getFolderInfo = function (path, successFunction) {
                    var url = ROADKILL_FILEMANAGERURL + "/folderinfo";
                    var data = { dir: path };
                    var errorMessage = ROADKILL_FILEMANAGER_ERROR_DIRECTORYLISTING + " <br/>";

                    this.makeAjaxRequest(url, data, errorMessage, successFunction);
                };

                AjaxRequest.prototype.deleteFolder = function (folder, successFunction) {
                    var url = ROADKILL_FILEMANAGERURL + "/deletefolder";
                    var data = { folder: folder };
                    var errorMessage = ROADKILL_FILEMANAGER_ERROR_DELETEFOLDER + " <br/>";

                    this.makeAjaxRequest(url, data, errorMessage, successFunction);
                };

                AjaxRequest.prototype.deleteFile = function (fileName, filePath, successFunction) {
                    var url = ROADKILL_FILEMANAGERURL + "/deletefile";
                    var data = { filename: fileName, filepath: filePath };
                    var errorMessage = ROADKILL_FILEMANAGER_ERROR_DELETEFILE + " <br/>";

                    this.makeAjaxRequest(url, data, errorMessage, successFunction);
                };

                AjaxRequest.prototype.newFolder = function (currentPath, newFolder, successFunction) {
                    var url = ROADKILL_FILEMANAGERURL + "/newFolder";
                    var data = { currentFolderPath: currentPath, newFolderName: newFolder };
                    var errorMessage = ROADKILL_FILEMANAGER_ERROR_CREATEFOLDER + " <br/>";

                    this.makeAjaxRequest(url, data, errorMessage, successFunction);
                };

                AjaxRequest.prototype.makeAjaxRequest = function (url, data, errorMessage, successFunction) {
                    var request = $.ajax({
                        type: "POST",
                        url: url,
                        data: data,
                        dataType: "json"
                    });

                    request.done(successFunction);

                    request.fail(function (jqXHR, textStatus, errorThrown) {
                        if (errorThrown.message.indexOf("unexpected character") !== -1) {
                            window.location = window.location;
                        } else {
                            toastr.error(errorMessage + errorThrown);
                        }
                    });
                };
                return AjaxRequest;
            })();
            FileManager.AjaxRequest = AjaxRequest;
        })(Web.FileManager || (Web.FileManager = {}));
        var FileManager = Web.FileManager;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
