var Roadkill;
(function (Roadkill) {
    (function (Web) {
        /// <reference path="../typescript-ref/filemanager.references.ts" />
        (function (FileManager) {
            var HtmlBuilder = (function () {
                function HtmlBuilder() {
                }
                HtmlBuilder.prototype.getNewFolder = function () {
                    var html = "";

                    html += "<tr id=\"newfolderrow\">";
                    html += "<td class=\"newfolder-icon\"><img src=\"" + ROADKILL_COREASSETPATH + "Images/filemanager/directory.png\"></td>";
                    html += "<td><span><input id=\"newfolderinput\" placeholder=\"" + ROADKILL_FILEMANAGER_ADDFOLDER + "\" /></span>";
                    html += "<img id=\"newfoldercancel\" title=\"" + ROADKILL_FILEMANAGER_ADDFOLDER_CANCEL + "\" src=\"" + ROADKILL_COREASSETPATH + "Images/filemanager/cancel.png\"></span>";
                    html += "<span style=\"vertical-align:bottom;\"></td>";
                    html += "<td colspan=\"3\"></td></tr>";

                    return html;
                };

                HtmlBuilder.prototype.getFolderTable = function (directoryViewModel) {
                    var html = [];

                    var header = "<table id=\"files\"><thead><tr>" + "<th colspan=2>Name</th>" + "<th>Date Uploaded</th>" + "<th>Type</th>" + "<th>Size</th>" + "</tr></thead>";
                    html.push(header);

                    for (var i = 0; i < directoryViewModel.ChildFolders.length; i++) {
                        var htmlRow = "";

                        htmlRow += "<tr class=\"listrow\" data-itemtype=\"folder\" data-urlpath=\"" + directoryViewModel.ChildFolders[i].UrlPath + "\">";
                        htmlRow += "<td width='1%'>";
                        htmlRow += "<img src='" + ROADKILL_COREASSETPATH + "Images/filemanager/directory.png'></td>";
                        htmlRow += "<td nowrap width=\"50%\">" + directoryViewModel.ChildFolders[i].Name + "</td>";
                        htmlRow += "<td></td>";
                        htmlRow += "<td></td>";
                        htmlRow += "<td></td>";
                        htmlRow += "</tr>";

                        html.push(htmlRow);
                    }

                    for (var i = 0; i < directoryViewModel.Files.length; i++) {
                        html.push(this.getFileRowHtml(directoryViewModel.Files[i]));
                    }

                    html.push("</table>");
                    return html;
                };

                HtmlBuilder.prototype.getBreadCrumb = function (directoryViewModel, count) {
                    var html = "";

                    html += "<li data-level=\"" + count + "\" data-urlpath=\"" + directoryViewModel.UrlPath + "\">";
                    html += "<a href=\"javascript:;\">" + directoryViewModel.Name + "</a>";
                    html += "</li>";

                    return html;
                };

                HtmlBuilder.prototype.getFileRowHtml = function (fileModel) {
                    var html = "";

                    html += "<tr class=\"listrow\" data-itemtype=\"file\">";
                    html += "<td width=\"1%\">";
                    html += "<img src=\"" + ROADKILL_COREASSETPATH + "Images/filemanager/file.png\" >";
                    html += "</td>";
                    html += "<td class=\"file\">{0}</td >";
                    html += "<td>{1}</td>";
                    html += "<td class=\"filetype\">{2}</td>";
                    html += "<td class=\"filesize\">{3}</td>";
                    html += "</tr> ";

                    return FileManager.Util.FormatString(html, fileModel.Name, fileModel.CreateDate, fileModel.Extension, fileModel.Size);
                };
                return HtmlBuilder;
            })();
            FileManager.HtmlBuilder = HtmlBuilder;
        })(Web.FileManager || (Web.FileManager = {}));
        var FileManager = Web.FileManager;
    })(Roadkill.Web || (Roadkill.Web = {}));
    var Web = Roadkill.Web;
})(Roadkill || (Roadkill = {}));
