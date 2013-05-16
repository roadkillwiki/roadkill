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
                    $("#addfolderbtn").on("click", this.addFolderInput);
                    $("#deletefolderbtn").bind("click", this.deleteFolder);
                    $("#deletefilebtn").bind("click", this.deleteFile);
                    $("#newfolderinput").live("keyup", this.addNewFolder);
                    $("#newfoldercancel").live("click", this.cancelNewFolder);
                };
                ButtonEvents.prototype.deleteFolder = function () {
                    var folder = FileManager.TableEvents.getCurrentPath();
                    if(folder == "") {
                        alert(ROADKILL_DELETE_BASEFOLDER_ERROR);
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
                            alert(data.message);
                        }
                    };
                    this._ajaxRequest.deleteFolder(folder, success);
                };
                ButtonEvents.prototype.deleteFile = function () {
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
                                alert(data.message);
                            }
                        };
                        this._ajaxRequest.deleteFile(filename, currentPath, success);
                    }
                };
                ButtonEvents.prototype.addFolderInput = function () {
                    if($("tr#newfolderrow").length > 0) {
                        $("#newfolderinput").focus();
                        return;
                    }
                    var tr = $("table#files tr[data-itemtype=folder]");
                    var newfolderHtml = this._htmlBuilder.getNewFolder();
                    if(tr.length > 0) {
                        tr = tr.last();
                        $(newfolderHtml).insertAfter(tr);
                    } else {
                        $("table#files").append(newfolderHtml);
                    }
                    $("#newfolderinput").focus();
                };
                ButtonEvents.prototype.addNewFolder = function (event) {
                    if(event.which == 0 || event.which == 27) {
                        this.cancelNewFolder();
                    } else if(event.which == 13) {
                        var newFolder = $("#newfolderinput").val();
                        if(newFolder.replace(/\s/g, "").length == 0) {
                            this.cancelNewFolder();
                            return;
                        }
                        var success = function (data) {
                            if(data.status == "error") {
                                alert(data.message);
                                return;
                            }
                            var item = $("ul.navigator li:last-child");
                            this.getFolderInfo(item.attr("data-urlpath"));
                            FileManager.BreadCrumbTrail.removeLastItem();
                            $("tr#newfolderrow").remove();
                        };
                        this._ajaxRequest.newFolder(FileManager.TableEvents.getCurrentPath(), newFolder, success);
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
