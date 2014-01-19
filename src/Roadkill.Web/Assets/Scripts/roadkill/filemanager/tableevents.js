var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/filemanager.references.ts" />
        (function (FileManager) {
            var TableEvents = (function () {
                function TableEvents() {
                }
                TableEvents.prototype.bind = function () {
                    var that = this;
                    $("#folder-container").on("mouseenter", "tr.listrow", function () {
                        $(this).addClass("focus");
                    }).on("mouseleave", "tr.listrow", function () {
                        $(this).removeClass("focus");
                    }).on("click", "tr.listrow", function () {
                        that.handleRowSelection(this);
                    }).on("dblclick", "tr.listrow", function () {
                        that.handleDoubleClickForRow(this);
                    });
                };

                TableEvents.prototype.handleRowSelection = function (tr) {
                    $("table#files tr.select").removeClass("select");
                    $(tr).addClass("select");

                    if ($(tr).attr("data-itemtype") !== "folder") {
                        var path = TableEvents.getCurrentPath();
                        if (path !== "/")
                            path += "/";

                        $("table#files").trigger("fileselected", {
                            file: path + $("td.file", tr).text()
                        });
                    }
                };

                TableEvents.prototype.handleDoubleClickForRow = function (tr) {
                    if ($(tr).attr("data-itemtype") === "folder") {
                        TableEvents.update($(tr).attr("data-urlpath"));
                    }
                };

                TableEvents.getCurrentPath = function () {
                    return $("ul.navigator li:last").attr("data-urlpath");
                };

                TableEvents.update = function (path, addBreadCrumb) {
                    if (typeof path === "undefined") { path = ""; }
                    if (typeof addBreadCrumb === "undefined") { addBreadCrumb = true; }
                    if (path === "")
                        path = TableEvents.getCurrentPath();

                    var that = this;
                    var success = function (data) {
                        if (data.status === "error") {
                            toastr.error(data.message);
                        } else {
                            if (addBreadCrumb)
                                FileManager.BreadCrumbTrail.addNewItem(data);

                            var htmlBuilder = new FileManager.HtmlBuilder();
                            var tableHtml = htmlBuilder.getFolderTable(data);
                            $("#folder-container").html(tableHtml.join(""));

                            var currentPath = TableEvents.getCurrentPath();
                            $("#destination_folder").val(currentPath);
                        }
                    };

                    var ajaxRequest = new FileManager.AjaxRequest();
                    ajaxRequest.getFolderInfo(path, success);
                };
                return TableEvents;
            })();
            FileManager.TableEvents = TableEvents;
        })(Web.FileManager || (Web.FileManager = {}));
        var FileManager = Web.FileManager;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
