/// <reference path="../typescript-ref/filemanager.references.ts" />
module Roadkill.Site.FileManager
{
	export class HtmlBuilder
	{
		public getNewFolder() : string
		{
			var html: string = "";

			html += "<tr id=\"newfolderrow\">";
			html += "<td><img src=\"" + ROADKILL_COREASSETPATH + "CSS/images/directory.png\"></td>";
			html += "<td><span><input id=\"newfolderinput\" placeholder=\"" +ROADKILL_FILEMANAGER_ADDFOLDER_PLACEHOLDER+ "\" /></span>";
			html += "<img id=\"newfoldercancel\" title=\"" +ROADKILL_FILEMANAGER_ADDFOLDER_CANCEL+ "\" src=\"" + ROADKILL_COREASSETPATH + "CSS/images/cancel.png\"></span>";
			html += "<span style=\"vertical-align:bottom;\"></td>";
			html += "<td colspan=\"3\"></td></tr>";

			return html;
		}

		public getFolderTable(directorySummary: DirectorySummary) : string[]
		{
			var html: string[] = [];

			var header = "<table id=\"files\"><thead><tr>"+
					"<th colspan=2>Name</th>" +
					"<th>Date Uploaded</th>" +
					"<th>Type</th>" +
					"<th>Size</th>" +
					"</tr></thead>";
			html.push(header);
			
			for (var i = 0; i < directorySummary.ChildFolders.length; i++)
			{
				var htmlRow: string = "";

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

			for (var i = 0; i < directorySummary.Files.length; i++)
			{
				html.push(this.getFileRowHtml(directorySummary.Files[i]));
			}

			html.push("</table>");
			return html;
		}

		public getBreadCrumb(directorySummary : DirectorySummary, count: number) : string
		{
			var html: string = "";

			html += "<li data-level=\"" + count + "\" data-urlpath=\"" + directorySummary.UrlPath + "\">";
			html += "<a href=\"javascript:;\">" +directorySummary.Name+ "</a>";
			html += "</li>";

			return html;
		}

		public getFileRowHtml(fileSummary: FileSummary): string
		{
			var html: string = "";

			html += "<tr class=\"listrow\" data-itemtype=\"file\">";
			html += "<td width=\"1%\">";
			html += "<img src=\"" + ROADKILL_COREASSETPATH + "CSS/images/file.png\" >";
			html += "</td>";
			html += "<td class=\"file\">{0}</td >";
			html += "<td>{1}</td>";
			html += "<td class=\"filetype\">{2}</td>";
			html += "<td class=\"filesize\">{3}</td>";
			html += "</tr> ";

			return Util.FormatString(html, fileSummary.Name, fileSummary.CreateDate, fileSummary.Extension, fileSummary.Size);
		}
	}
}