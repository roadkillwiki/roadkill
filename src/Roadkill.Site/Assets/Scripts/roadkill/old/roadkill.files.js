/// <reference path="jquery-1.8.0-vsdoc.js" />
/// <reference path="filemanager/jquery.fileupload.js" />

/**
 Event bindings and handlers for the file manager.
*/

function fileManagerInit()
{
	$('#fileupload').fileupload({
		dropZone: $("#folder-container"),
		pasteZone: $("body"),
		dataType: 'json',
		progressall: function (e, data)
		{
			var progress = parseInt(data.loaded / data.total * 100, 10);
			$('#progress .bar').css('width', progress + '%');
		},
		done: function (e, data)
		{
			if (data.result.status == "error")
			{
				alert(data.result.message);
				return;
			}
			$.each(data.result.files, function (index, file)
			{
				$('#files').append(getFileRowHtml(file));
			});
			setTimeout(function () { $("#progress div.bar").css("width", "0%"); }, 2000);
		}
	})
    .bind('fileuploaddrop', function (e, data) { setCurrentPath(); });

	$("#addfolderbtn").on("click", addFolderInput);
	$("#deletefolderbtn").bind("click", deleteFolder);
	$("#deletefilebtn").bind("click", deleteFile);
	$("#newfolderinput").live("keyup", saveNewFolder);
	$("#newfoldercancel").live("click", cancelNewFolder);
}

function deleteFolder()
{
	var s_folder = getCurrentPath();

	if (s_folder == "")
	{
		alert(ROADKILL_DELETE_BASEFOLDER_ERROR);
		return;
	}

	if (!confirm(ROADKILL_DELETE_CONFIRM.format(s_folder)))
		return;

	$.ajax({
		type: 'POST',
		url: "filemanager/deletefolder",
		data: { folder: s_folder },
		success: function (data) { if (data.status == "ok") navigatePriorBreadcrumb(); else alert(data.message); },
		dataType: "json"
	});
}

function deleteFile()
{
	var tr = $("tr.select");

	if (tr.length > 0 && tr.attr("data-itemtype") == "file")
	{

		var s_filename = $("td.file", tr).text();

		if (!confirm(ROADKILL_DELETE_CONFIRM.format((getCurrentPath() + "/" + s_filename))))
			return;

		$.ajax({
			type: 'POST',
			url: "filemanager/deletefile",
			data: { filepath: getCurrentPath(), filename: s_filename },
			success: function (data) { if (data.status == "ok") $(tr).remove(); else alert(data.message); },
			dataType: "json"
		});
	}

}

function addFolderInput()
{
	if ($("tr#newfolderrow").length > 0)
	{
		$("#newfolderinput").focus();
		return;
	}

	var tr = $("table#files tr[data-itemtype=folder]");
	var s_newfolderhtml = "<tr id=\"newfolderrow\">";
	s_newfolderhtml += "<td><img src=\"" + ROADKILL_COREASSETPATH + "CSS/images/directory.png\"></td>";
	s_newfolderhtml += "<td><span><input id=\"newfolderinput\" placeholder=\"New folder\" /></span>";
	s_newfolderhtml += "<img id=\"newfoldercancel\" title=\"Cancel New Folder\" src=\"" + ROADKILL_COREASSETPATH + "CSS/images/cancel.png\"></span>";
	s_newfolderhtml += "<span style=\"vertical-align:bottom;\"></td>";
	s_newfolderhtml += "<td colspan=3></td></tr>";

	if (tr.length > 0)
	{
		tr = tr.last();
		$(s_newfolderhtml).insertAfter(tr);
	}
	else
	{
		$("table#files").append(s_newfolderhtml);
	}
	$("#newfolderinput").focus();
}

function saveNewFolder(event)
{
	if (event.which == 0 || event.which == 27)
	{
		cancelNewFolder();
	}
	else if (event.which == 13)
	{

		var s_newfolder = $("#newfolderinput").val();

		if (s_newfolder.replace(/\s/g, "").length == 0)
		{
			cancelNewFolder();
			return;
		}

		$.ajax({
			type: 'POST',
			url: "filemanager/newfolder",
			data: { currentFolderPath: getCurrentPath(), newFolderName: s_newfolder },
			success: processNewFolder,
			dataType: "json"
		});
	}
}

function cancelNewFolder()
{
	$("tr#newfolderrow").remove();
}

function processNewFolder(data)
{
	if (data.status == "error")
	{
		alert(data.message);
		return;
	}

	var item = $("ul.navigator li:last-child");
	navigateBreadcrumb(item.attr("data-level"), item.attr("data-urlpath"));
	$("tr#newfolderrow").remove();

}

// File Navigator functions
function navigatorInit()
{
	$("tr.listrow")
        .live("mouseenter", function () { $(this).addClass("focus"); })
        .live("mouseleave", function () { $(this).removeClass("focus"); })
        .live("click", function () { handleRowSelection(this); });

	imagePreviewInit();
	navigatePath("");
}

function navigatePath(s_path)
{
	$.ajax({
		type: 'POST',
		url: ROADKILL_FILEMANAGERURL + "/folderinfo",
		data: { dir: s_path },
		success: function (data) { addBreadcrumb(data); buildTableFolderView(data); setCurrentPath(); },
		dataType: "json"
	});
}

function setCurrentPath()
{
	var s_current_path = getCurrentPath();
	$("#destination_folder").val(s_current_path);
}

function getCurrentPath()
{
	return $("ul.navigator li:last").attr("data-urlpath");
}

function addBreadcrumb(folderinfo)
{
	var n_count = $("ul.navigator li").length;

	var s_html = "<li data-level=\"" + n_count + "\" data-urlpath=\"" + folderinfo.UrlPath + "\">";
	s_html += "<a href=\"javascript:navigateBreadcrumb(" + n_count + ",&quot;" + escape(folderinfo.UrlPath) + "&quot;)\">";
	s_html += folderinfo.Name + "</a>";
	s_html += "</li>";
	
	$("ul.navigator").append(s_html);
}

function navigateBreadcrumb(level, s_folder)
{
	if (level == 0)
		$("ul.navigator li").remove();
	else
		$("ul.navigator li:gt(" + (level - 1) + ")").remove();

	navigatePath(s_folder);
}

function navigatePriorBreadcrumb()
{
	var n_count = $("ul.navigator li").length;
	if (n_count == 1) // cannot delete base attachments directory
		return;

	var li = $("ul.navigator li:last-child").prev("li");
	var n_level = li.attr("data-level");
	var s_folder = li.attr("data-urlpath");

	navigateBreadcrumb(n_level, s_folder);
}

function handleRowSelection(tr)
{
	if ($(tr).attr("data-itemtype") == "folder")
	{
		navigatePath($(tr).attr("data-itemid"));
	} else
	{
		$("table#files tr.select").removeClass("select");
		$(tr).addClass("select");
		$("table#files").trigger("fileselected", { file: getCurrentPath() + "/" + $("td.file", tr).text() });
	}
}

function buildTableFolderView(data)
{
	var a_html = ["<table id=\"files\"><thead><tr><th colspan=2>Name</th><th>Date Uploaded</th><th>Type</th><th>Size</th></tr></thead>"];

	for (var i = 0; i < data.ChildFolders.length; i++)
	{
		a_html.push("<tr class=\"listrow\" data-itemtype=\"folder\" data-itemid=\"" + data.ChildFolders[i].UrlPath + "\"><td width='1%'><img src='" + ROADKILL_COREASSETPATH + "CSS/images/directory.png'></td><td nowrap width=\"20%\">" + data.ChildFolders[i].Name + "</td><td></td><td></td><td></td></tr>");
	}
	for (var i = 0; i < data.Files.length; i++)
	{
		a_html.push(getFileRowHtml(data.Files[i]));
	}
	a_html.push("</table>");

	$("#folder-container").empty().append(a_html.join(""));
}

function getFileRowHtml(file)
{
	var s_html = "<tr class=\"listrow\" data-itemtype=\"file\"><td width='1%'><img src='" + ROADKILL_COREASSETPATH + "CSS/images/file.png'></td><td class=\"file\">{0}</td><td>{1}</td><td class=filetype>{2}</td><td class=filesize>{3}</td></tr>";

	return s_html.format(file.Name, file.CreateDate, file.Extension, file.Size);
}

function imagePreviewInit()
{
	var xOffset = 20;
	var yOffset = 20;

	$("table#files tr[data-itemtype=file]")
        .live("mouseenter",function (e)
		    {
		    	var s_file_type = $("td.filetype", this).text();
		    	if (s_file_type.search(/^(jpg|png|gif)$/i) == -1)
		    		return;
		    	var s_img_url = (ROADKILL_ATTACHMENTSPATH + getCurrentPath() + "/").replace("//", "/") + $("td.file", this).text();
		    	$("body").append("<p id='image-preview'><img src='" + s_img_url + "' alt='Image Preview' /></p>");
		    	$("#image-preview")
				    .css("top", (e.pageY - xOffset) + "px")
				    .css("left", (e.pageX + yOffset) + "px")
				    .fadeIn("fast");
		    })
        .live("mouseleave",function ()
		    {
		    	$("#image-preview").remove();
		    })
        .live("mousemove",function (e)
    		{
    			$("#preview")
	    			.css("top", (e.pageY - xOffset) + "px")
		    		.css("left", (e.pageX + yOffset) + "px");
    		});
};

if (typeof String.prototype.format !== 'function')
{
	String.prototype.format = function ()
	{

		var s = this, exp = null;

		if (arguments.length == 1 && arguments[0] && typeof (arguments[0]) == 'object')
		{
			for (var item in arguments[0])
			{
				if (arguments[0].hasOwnProperty(item))
				{
					exp = new RegExp('\\{' + (item) + '\\}', 'gm');
					s = s.replace(exp, arguments[0][item]);
				}
			}
		} else
		{
			for (var i = 0; i < arguments.length; i++)
			{
				exp = new RegExp('\\{' + (i) + '\\}', 'gm');
				s = s.replace(exp, arguments[i]);
			}
		}

		return s;
	}
}
