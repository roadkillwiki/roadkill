var Roadkill;
(function (Roadkill) {
    (function (Site) {
        (function (FileManager) {
            var HtmlBuilder = (function () {
                function HtmlBuilder() { }
                HtmlBuilder.prototype.getNewFolder = function () {
                    var html = "";
                    html += "<tr id=\"newfolderrow\">";
                    html += "<td><img src=\"" + ROADKILL_COREASSETPATH + "CSS/images/directory.png\"></td>";
                    html += "<td><span><input id=\"newfolderinput\" placeholder=\"" + ROADKILL_FILEMANAGER_ADDFOLDER_PLACEHOLDER + "\" /></span>";
                    html += "<img id=\"newfoldercancel\" title=\"" + ROADKILL_FILEMANAGER_ADDFOLDER_CANCEL + "\" src=\"" + ROADKILL_COREASSETPATH + "CSS/images/cancel.png\"></span>";
                    html += "<span style=\"vertical-align:bottom;\"></td>";
                    html += "<td colspan=\"3\"></td></tr>";
                    return html;
                };
                HtmlBuilder.prototype.getFolderTable = function (directorySummary) {
                    var html = [];
                    var header = "<table id=\"files\"><thead><tr>" + "<th colspan=2>Name</th>" + "<th>Date Uploaded</th>" + "<th>Type</th>" + "<th>Size</th>" + "</tr></thead>";
                    html.push(header);
                    for(var i = 0; i < directorySummary.ChildFolders.length; i++) {
                        var htmlRow = "";
                        htmlRow += "<tr class=\"listrow\" data-itemtype=\"folder\" data-urlpath=\"" + directorySummary.ChildFolders[i].UrlPath + "\">";
                        htmlRow += "<td width='1%'>";
                        htmlRow += "<img src='" + ROADKILL_COREASSETPATH + "CSS/images/directory.png'></td>";
                        htmlRow += "<td nowrap width=\"20%\">" + directorySummary.ChildFolders[i].Name + "</td>";
                        htmlRow += "<td></td>";
                        htmlRow += "<td></td>";
                        htmlRow += "<td></td>";
                        htmlRow += "</tr>";
                        html.push(htmlRow);
                    }
                    for(var i = 0; i < directorySummary.Files.length; i++) {
                        html.push(this.getFileRowHtml(directorySummary.Files[i]));
                    }
                    html.push("</table>");
                    return html;
                };
                HtmlBuilder.prototype.getBreadCrumb = function (directorySummary, count) {
                    var html = "";
                    html += "<li data-level=\"" + count + "\" data-urlpath=\"" + directorySummary.UrlPath + "\">";
                    html += "<a href=\"javascript:;\">" + directorySummary.Name + "</a>";
                    html += "</li>";
                    return html;
                };
                HtmlBuilder.prototype.getFileRowHtml = function (fileSummary) {
                    var html = "";
                    html += "<tr class=\"listrow\" data-itemtype=\"file\">";
                    html += "<td width=\"1%\">";
                    html += "<img src=\"" + ROADKILL_COREASSETPATH + "CSS/images/file.png\" >";
                    html += "</td>";
                    html += "<td class=\"file\">{0}</td >";
                    html += "<td>{1}</td>";
                    html += "<td class=\"filetype\">{2}</td>";
                    html += "<td class=\"filesize\">{3}</td>";
                    html += "</tr> ";
                    return FileManager.Util.FormatString(html, fileSummary.Name, fileSummary.CreateDate, fileSummary.Extension, fileSummary.Size);
                };
                return HtmlBuilder;
            })();
            FileManager.HtmlBuilder = HtmlBuilder;            
        })(Site.FileManager || (Site.FileManager = {}));
        var FileManager = Site.FileManager;

    })(Roadkill.Site || (Roadkill.Site = {}));
    var Site = Roadkill.Site;

})(Roadkill || (Roadkill = {}));

