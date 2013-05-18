var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (FileManager) {
            var AjaxRequest = (function () {
                function AjaxRequest() { }
                AjaxRequest.prototype.getFolderInfo = function (path, successFunction) {
                    var url = ROADKILL_FILEMANAGERURL + "/folderinfo";
                    var data = {
                        dir: path
                    };
                    var errorMessage = "An expected error occurred getting the directory listing: <br/>";
                    this.makeAjaxRequest(url, data, errorMessage, successFunction);
                };
                AjaxRequest.prototype.deleteFolder = function (folder, successFunction) {
                    var url = ROADKILL_FILEMANAGERURL + "/deletefolder";
                    var data = {
                        folder: folder
                    };
                    var errorMessage = "An expected error occurred deleting the folder: <br/>";
                    this.makeAjaxRequest(url, data, errorMessage, successFunction);
                };
                AjaxRequest.prototype.deleteFile = function (fileName, filePath, successFunction) {
                    var url = ROADKILL_FILEMANAGERURL + "/deletefile";
                    var data = {
                        filename: fileName,
                        filepath: filePath
                    };
                    var errorMessage = "An expected error occurred deleting the file: <br/>";
                    this.makeAjaxRequest(url, data, errorMessage, successFunction);
                };
                AjaxRequest.prototype.newFolder = function (currentPath, newFolder, successFunction) {
                    var url = ROADKILL_FILEMANAGERURL + "/newFolder";
                    var data = {
                        currentFolderPath: currentPath,
                        newFolderName: newFolder
                    };
                    var errorMessage = "An expected error occurred creating the folder: <br/>";
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
                        toastr.error(errorMessage + errorThrown);
                    });
                    request.always(function () {
                    });
                };
                return AjaxRequest;
            })();
            FileManager.AjaxRequest = AjaxRequest;            
        })(Site.FileManager || (Site.FileManager = {}));
        var FileManager = Site.FileManager;

    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;

})(Roadkill || (Roadkill = {}));

