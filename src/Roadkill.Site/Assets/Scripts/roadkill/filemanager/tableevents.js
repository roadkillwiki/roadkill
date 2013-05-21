var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (FileManager) {
            var TableEvents = (function () {
                function TableEvents() { }
                TableEvents.prototype.bind = function () {
                    var that = this;
                    $("tr.listrow").live("mouseenter", function () {
                        $(this).addClass("focus");
                    }).live("mouseleave", function () {
                        $(this).removeClass("focus");
                    }).live("click", function () {
                        that.handleRowSelection(this);
                    }).live("dblclick", function () {
                        that.handleDoubleClickForRow(this);
                    });
                };
                TableEvents.prototype.handleRowSelection = function (tr) {
                    $("table#files tr.select").removeClass("select");
                    $(tr).addClass("select");
                    $("table#files").trigger("fileselected", {
                        file: TableEvents.getCurrentPath() + "/" + $("td.file", tr).text()
                    });
                };
                TableEvents.prototype.handleDoubleClickForRow = function (tr) {
                    if($(tr).attr("data-itemtype") == "folder") {
                        TableEvents.update($(tr).attr("data-urlpath"));
                    }
                };
                TableEvents.getCurrentPath = function getCurrentPath() {
                    return $("ul.navigator li:last").attr("data-urlpath");
                };
                TableEvents.update = function update(path, addBreadCrumb) {
                    if (typeof path === "undefined") { path = ""; }
                    if (typeof addBreadCrumb === "undefined") { addBreadCrumb = true; }
                    if(path === "") {
                        path = TableEvents.getCurrentPath();
                    }
                    var that = this;
                    var success = function (data) {
                        if(addBreadCrumb) {
                            FileManager.BreadCrumbTrail.addNewItem(data);
                        }
                        var htmlBuilder = new FileManager.HtmlBuilder();
                        var tableHtml = htmlBuilder.getFolderTable(data);
                        $("#folder-container").html(tableHtml.join(""));
                        var currentPath = TableEvents.getCurrentPath();
                        $("#destination_folder").val(currentPath);
                    };
                    var ajaxRequest = new FileManager.AjaxRequest();
                    ajaxRequest.getFolderInfo(path, success);
                };
                return TableEvents;
            })();
            FileManager.TableEvents = TableEvents;            
        })(Site.FileManager || (Site.FileManager = {}));
        var FileManager = Site.FileManager;
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
