var Roadkill;
(function (Roadkill) {
    (function (Site) {
        var FileManager = (function () {
            function FileManager() {
                this._navigator = new Navigator();
                $('#fileupload').fileupload({
                    dropZone: $("#folder-container"),
                    pasteZone: $("body"),
                    dataType: 'json',
                    progressall: function (e, data) {
                        var percentage = (data.loaded / data.total * 100) + "";
                        var progress = parseInt(percentage, 10);
                        $('#progress .bar').css('width', progress + '%');
                    },
                    done: function (e, data) {
                        if(data.result.status == "error") {
                            alert(data.result.message);
                            return;
                        }
                        $.each(data.result.files, function (index, file) {
                            $('#files').append(this._navigator.getFileRowHtml(file));
                        });
                        setTimeout(function () {
                            $("#progress div.bar").css("width", "0%");
                        }, 2000);
                    }
                }).bind('fileuploaddrop', function (e, data) {
                    this._navigator.setCurrentPath();
                });
                $("#addfolderbtn").on("click", this.addFolderInput);
                $("#deletefolderbtn").bind("click", this.deleteFolder);
                $("#deletefilebtn").bind("click", this.deleteFile);
                $("#newfolderinput").live("keyup", this.saveNewFolder);
                $("#newfoldercancel").live("click", this.cancelNewFolder);
            }
            FileManager.prototype.deleteFolder = function () {
                var folder = this._navigator.getCurrentPath();
                if(folder == "") {
                    alert(ROADKILL_DELETE_BASEFOLDER_ERROR);
                    return;
                }
                var message = Util.FormatString(ROADKILL_DELETE_CONFIRM, folder);
                if(!confirm(message)) {
                    return;
                }
                $.ajax({
                    type: "POST",
                    url: "filemanager/deletefolder",
                    data: {
                        folder: folder
                    },
                    success: function (data) {
                        if(data.status == "ok") {
                            this._navigator.navigatePriorBreadcrumb();
                        } else {
                            alert(data.message);
                        }
                    },
                    dataType: "json"
                });
            };
            FileManager.prototype.deleteFile = function () {
                var tr = $("tr.select");
                if(tr.length > 0 && tr.attr("data-itemtype") == "file") {
                    var currentPath = this._navigator.getCurrentPath();
                    var filename = $("td.file", tr).text();
                    var message = Util.FormatString(ROADKILL_DELETE_CONFIRM, currentPath + "/" + filename);
                    if(!confirm(message)) {
                        return;
                    }
                    $.ajax({
                        type: "POST",
                        url: "filemanager/deletefile",
                        data: {
                            filepath: currentPath,
                            filename: filename
                        },
                        success: function (data) {
                            if(data.status == "ok") {
                                $(tr).remove();
                            } else {
                                alert(data.message);
                            }
                        },
                        dataType: "json"
                    });
                }
            };
            FileManager.prototype.addFolderInput = function () {
                if($("tr#newfolderrow").length > 0) {
                    $("#newfolderinput").focus();
                    return;
                }
                var tr = $("table#files tr[data-itemtype=folder]");
                var newfolderHtml;
                newfolderHtml += "<tr id=\"newfolderrow\">";
                newfolderHtml += "<td><img src=\"" + ROADKILL_COREASSETPATH + "CSS/images/directory.png\"></td>";
                newfolderHtml += "<td><span><input id=\"newfolderinput\" placeholder=\"New folder\" /></span>";
                newfolderHtml += "<img id=\"newfoldercancel\" title=\"Cancel New Folder\" src=\"" + ROADKILL_COREASSETPATH + "CSS/images/cancel.png\"></span>";
                newfolderHtml += "<span style=\"vertical-align:bottom;\"></td>";
                newfolderHtml += "<td colspan=\"3\"></td></tr>";
                if(tr.length > 0) {
                    tr = tr.last();
                    $(newfolderHtml).insertAfter(tr);
                } else {
                    $("table#files").append(newfolderHtml);
                }
                $("#newfolderinput").focus();
            };
            FileManager.prototype.saveNewFolder = function (event) {
                if(event.which == 0 || event.which == 27) {
                    this.cancelNewFolder();
                } else if(event.which == 13) {
                    var newFolder = $("#newfolderinput").val();
                    if(newFolder.replace(/\s/g, "").length == 0) {
                        this.cancelNewFolder();
                        return;
                    }
                    $.ajax({
                        type: "POST",
                        url: "filemanager/newfolder",
                        data: {
                            currentFolderPath: this._navigator.getCurrentPath(),
                            newFolderName: newFolder
                        },
                        success: this.processNewFolder,
                        dataType: "json"
                    });
                }
            };
            FileManager.prototype.cancelNewFolder = function () {
                $("tr#newfolderrow").remove();
            };
            FileManager.prototype.processNewFolder = function (data) {
                if(data.status == "error") {
                    alert(data.message);
                    return;
                }
                var item = $("ul.navigator li:last-child");
                this._navigator.navigateBreadcrumb(item.attr("data-level"), item.attr("data-urlpath"));
                $("tr#newfolderrow").remove();
            };
            return FileManager;
        })();
        Site.FileManager = FileManager;        
        var Navigator = (function () {
            function Navigator() {
                $("tr.listrow").live("mouseenter", function () {
                    $(this).addClass("focus");
                }).live("mouseleave", function () {
                    $(this).removeClass("focus");
                }).live("click", function () {
                    this.handleRowSelection(this);
                });
                this.imagePreviewInit();
                this.navigatePath("");
            }
            Navigator.prototype.setCurrentPath = function () {
                var currentPath = this.getCurrentPath();
                $("#destination_folder").val(currentPath);
            };
            Navigator.prototype.addBreadcrumb = function (folderinfo) {
                var count = $("ul.navigator li").length;
                var html;
                html += "<li data-level=\"" + count + "\" data-urlpath=\"" + folderinfo.UrlPath + "\">";
                html += "<a href=\"javascript:navigateBreadcrumb(" + count + ",&quot;" + encodeURI(folderinfo.UrlPath) + "&quot;)\">";
                html += folderinfo.Name + "</a>";
                html += "</li>";
                $("ul.navigator").append(html);
            };
            Navigator.prototype.handleRowSelection = function (tr) {
                if($(tr).attr("data-itemtype") == "folder") {
                    this.navigatePath($(tr).attr("data-itemid"));
                } else {
                    $("table#files tr.select").removeClass("select");
                    $(tr).addClass("select");
                    $("table#files").trigger("fileselected", {
                        file: this.getCurrentPath() + "/" + $("td.file", tr).text()
                    });
                }
            };
            Navigator.prototype.buildTableFolderView = function (data) {
                var html = [
                    "<table id=\"files\"><thead><tr><th colspan=2>Name</th><th>Date Uploaded</th><th>Type</th><th>Size</th></tr></thead>"
                ];
                for(var i = 0; i < data.ChildFolders.length; i++) {
                    var htmlRow;
                    htmlRow += "<tr class=\"listrow\" data-itemtype=\"folder\" data-itemid=\"" + data.ChildFolders[i].UrlPath + "\">";
                    htmlRow += "<td width='1%'>";
                    htmlRow += "<img src='" + ROADKILL_COREASSETPATH + "CSS/images/directory.png'></td>";
                    htmlRow += "<td nowrap width=\"20%\">" + data.ChildFolders[i].Name + "</td>";
                    htmlRow += "<td></td>";
                    htmlRow += "<td></td>";
                    htmlRow += "<td></td>";
                    htmlRow += "</tr>";
                    html.push(htmlRow);
                }
                for(var i = 0; i < data.Files.length; i++) {
                    html.push(this.getFileRowHtml(data.Files[i]));
                }
                html.push("</table>");
                $("#folder-container").empty().append(html.join(""));
            };
            Navigator.prototype.getFileRowHtml = function (file) {
                var html;
                html += "<tr class=\"listrow\" data-itemtype=\"file\">";
                html += "<td width=\"1%\">";
                html += "<img src=\"" + ROADKILL_COREASSETPATH + "CSS/images/file.png\" >";
                html += "</td>";
                html += "<td class=\"file\">{0}</td >";
                html += "<td>{1}</td>";
                html += "<td class=\"filetype\">{2}</td>";
                html += "<td class=\"filesize\">{3}</td>";
                html += "</tr> ";
                return Util.FormatString(html, file.Name, file.CreateDate, file.Extension, file.Size);
            };
            Navigator.prototype.imagePreviewInit = function () {
                var xOffset = 20;
                var yOffset = 20;
                $("table#files tr[data-itemtype=file]").live("mouseenter", function (e) {
                    var fileType;
                    fileType = $("td.filetype", this).text();
                    if(fileType.search(/^(jpg|png|gif)$/i) == -1) {
                        return;
                    }
                    var imgUrl;
                    imgUrl = (ROADKILL_ATTACHMENTSPATH + this.getCurrentPath() + "/");
                    imgUrl = imgUrl.replace("//", "/") + $("td.file", this).text();
                    $("body").append("<p id='image-preview'><img src='" + imgUrl + "' alt='Image Preview' /></p>");
                    $("#image-preview").css("top", (e.pageY - xOffset) + "px").css("left", (e.pageX + yOffset) + "px").fadeIn("fast");
                }).live("mouseleave", function () {
                    $("#image-preview").remove();
                }).live("mousemove", function (e) {
                    $("#preview").css("top", (e.pageY - xOffset) + "px").css("left", (e.pageX + yOffset) + "px");
                });
            };
            Navigator.prototype.getCurrentPath = function () {
                return $("ul.navigator li:last").attr("data-urlpath");
            };
            Navigator.prototype.navigateBreadcrumb = function (level, folder) {
                if(level == 0) {
                    $("ul.navigator li").remove();
                } else {
                    $("ul.navigator li:gt(" + (level - 1) + ")").remove();
                }
                this.navigatePath(folder);
            };
            Navigator.prototype.navigatePath = function (path) {
                $.ajax({
                    type: 'POST',
                    url: ROADKILL_FILEMANAGERURL + "/folderinfo",
                    data: {
                        dir: path
                    },
                    success: function (data) {
                        this.addBreadcrumb(data);
                        this.buildTableFolderView(data);
                        this.setCurrentPath();
                    },
                    dataType: "json"
                });
            };
            Navigator.prototype.navigatePriorBreadcrumb = function () {
                var count = $("ul.navigator li").length;
                if(count == 1) {
                    return;
                }
                var li = $("ul.navigator li:last-child").prev("li");
                var level = li.attr("data-level");
                var folder = li.attr("data-urlpath");
                this.navigateBreadcrumb(level, folder);
            };
            return Navigator;
        })();        
        var Util = (function () {
            function Util() { }
            Util.FormatString = function FormatString(format) {
                var args = [];
                for (var _i = 0; _i < (arguments.length - 1); _i++) {
                    args[_i] = arguments[_i + 1];
                }
                var result = format;
                for(var i = 0; i < args.length; i++) {
                    var regex = new RegExp('\\{' + (i) + '\\}', 'gm');
                    result = result.replace(regex, args[i]);
                }
                return result;
            };
            return Util;
        })();        
    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;
})(Roadkill || (Roadkill = {}));
