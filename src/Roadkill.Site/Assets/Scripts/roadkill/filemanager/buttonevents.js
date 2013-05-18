var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (FileManager) {
            var ButtonEvents = (function () {
                function ButtonEvents() {
                    this._htmlBuilder = new FileManager.HtmlBuilder();
                    this._ajaxRequest = new FileManager.AjaxRequest();
                }
                ButtonEvents.prototype.bind = function () {
                    var that = this;
                    $(document).on("click", "#addfolderbtn", {
                        instance: that
                    }, this.addFolderInput);
                    $(document).on("click", "#deletefolderbtn", {
                        instance: that
                    }, this.deleteFolder);
                    $(document).on("click", "#deletefilebtn", {
                        instance: that
                    }, this.deleteFile);
                    $(document).on("keyup", "#newfolderinput", {
                        instance: that
                    }, this.addNewFolder);
                    $(document).on("click", "#newfoldercancel", {
                        instance: that
                    }, this.cancelNewFolder);
                };
                ButtonEvents.prototype.deleteFolder = function (event) {
                    var that = event.data.instance;
                    var tr = $("tr.select");
                    var folder = tr.attr("data-urlpath");
                    if(folder == "") {
                        toastr.error(ROADKILL_DELETE_BASEFOLDER_ERROR);
                        return;
                    }
                    var message = FileManager.Util.FormatString(ROADKILL_DELETE_CONFIRM, folder);
                    if(!confirm(message)) {
                        return;
                    }
                    var success = function (data) {
                        if(data.status == "ok") {
                            FileManager.BreadCrumbTrail.removePriorBreadcrumb();
                            var li = $("ul.navigator li:last-child").prev("li");
                            var folder = li.attr("data-urlpath");
                            FileManager.TableEvents.update(folder);
                        } else {
                            toastr.error("Unable to delete the folder: <br/>" + data.message);
                        }
                    };
                    that._ajaxRequest.deleteFolder(folder, success);
                };
                ButtonEvents.prototype.deleteFile = function (event) {
                    var that = event.data.instance;
                    var tr = $("tr.select");
                    if(tr.length > 0 && tr.attr("data-itemtype") == "file") {
                        var currentPath = FileManager.TableEvents.getCurrentPath();
                        var filename = $("td.file", tr).text();
                        var message = FileManager.Util.FormatString(ROADKILL_DELETE_CONFIRM, currentPath + "/" + filename);
                        if(!confirm(message)) {
                            return;
                        }
                        var success = function (data) {
                            if(data.status == "ok") {
                                $(tr).remove();
                            } else {
                                toastr.error("Unable to delete the file: <br/>" + data.message);
                            }
                        };
                        that._ajaxRequest.deleteFile(filename, currentPath, success);
                    }
                };
                ButtonEvents.prototype.addFolderInput = function (event) {
                    var that = event.data.instance;
                    if($("tr#newfolderrow").length > 0) {
                        $("#newfolderinput").focus();
                        return;
                    }
                    var tr = $("table#files tr[data-itemtype=folder]");
                    var newfolderHtml = that._htmlBuilder.getNewFolder();
                    if(tr.length > 0) {
                        tr = tr.last();
                        $(newfolderHtml).insertAfter(tr);
                    } else {
                        $("table#files").append(newfolderHtml);
                    }
                    $("#newfolderinput").focus();
                };
                ButtonEvents.prototype.addNewFolder = function (event) {
                    var that = event.data.instance;
                    if(event.which == 0 || event.which == 27) {
                        that.cancelNewFolder();
                    } else {
                        if(event.which == 13) {
                            var newFolder = $("#newfolderinput").val();
                            if(newFolder.replace(/\s/g, "").length == 0) {
                                that.cancelNewFolder();
                                return;
                            }
                            var success = function (data) {
                                if(data.status == "error") {
                                    toastr.error("Unable to create the folder: <br/>" + data.message);
                                    return;
                                }
                                var item = $("ul.navigator li:last-child");
                                FileManager.TableEvents.update(item.attr("data-urlpath"));
                                FileManager.BreadCrumbTrail.removeLastItem();
                                $("tr#newfolderrow").remove();
                            };
                            that._ajaxRequest.newFolder(FileManager.TableEvents.getCurrentPath(), newFolder, success);
                        }
                    }
                };
                ButtonEvents.prototype.cancelNewFolder = function () {
                    $("tr#newfolderrow").remove();
                };
                return ButtonEvents;
            })();
            FileManager.ButtonEvents = ButtonEvents;            
        })(Site.FileManager || (Site.FileManager = {}));
        var FileManager = Site.FileManager;

    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;

})(Roadkill || (Roadkill = {}));

