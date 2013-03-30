/// <reference path="jquery-1.8.0-vsdoc.js" />

/**
 Event bindings and handlers for the file manager.
*/

function fileManagerInit() {

    $('#fileupload').fileupload({
        dropZone: $("#folder-container"),
        dataType: 'json',
        progressall: function (e, data) {
            var progress = parseInt(data.loaded / data.total * 100, 10);
            $('#progress .bar').css('width', progress + '%');
        },
        done: function (e, data) {
            if (data.result.status == "error") {
                alert(data.result.message);
                return;
            }
            $.each(data.result.files, function (index, file) {
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


function deleteFolder() {

    var s_folder = getCurrentPath();

    if (s_folder.search(/^\/Attachments$/) != -1) {
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

function deleteFile() {

    var tr = $("tr.select");

    if (tr.length > 0 && tr.attr("data-itemtype") == "file") {

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


function addFolderInput() {

    if ($("tr#newfolderrow").length > 0) {
        $("#newfolderinput").focus();
        return;
    }

    var tr = $("table#files tr[data-itemtype=folder]");
    var s_newfolderhtml = "<tr id=\"newfolderrow\"><td><img src=\"Assets/CSS/images/directory.png\"></td><td><span><input id=\"newfolderinput\" placeholder=\"New folder\" /></span><span style=\"vertical-align:bottom;\"><img id=\"newfoldercancel\" title=\"Cancel New Folder\" src=\"Assets/CSS/images/cancel.png\"></span></td><td colspan=3></td></tr>";
    if (tr.length > 0) {
        tr = tr.last();
        $(s_newfolderhtml).insertAfter(tr);
    } else {
        $("table#files").append(s_newfolderhtml);
    }
    $("#newfolderinput").focus();
}

function saveNewFolder(event) {

    if (event.which == 0 || event.which == 27) {
        cancelNewFolder();
    } else if (event.which == 13) {

        var s_newfolder = $("#newfolderinput").val();

        if (s_newfolder.replace(/\s/g, "").length == 0) {
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

function cancelNewFolder() {
    $("tr#newfolderrow").remove();
}

function processNewFolder(data) {

    if (data.status == "error") {
        alert(data.message);
        return;
    }

    var item = $("ul.navigator li:last-child");
    navigateBreadcrumb(item.attr("data-level"), item.attr("data-safepath"));
    $("tr#newfolderrow").remove();

}

// File Navigator functions

function navigatorInit() {

    $("tr.listrow")
        .live("mouseenter", function () { $(this).addClass("focus"); })
        .live("mouseleave", function () { $(this).removeClass("focus"); })
        .live("click", function () { handleRowSelection(this); });

    imagePreviewInit();

    navigatePath("");

}

function navigatePath(s_path) {

    $.ajax({
        type: 'POST',
        url: ROADKILL_FILEMANAGERURL + "/folderinfo",
        data: { dir: s_path },
        success: function (data) { addBreadcrumb(data); buildTableFolderView(data); setCurrentPath(); },
        dataType: "json"
    });

}

function setCurrentPath() {
    var s_current_path = getCurrentPath();
    $("#destination_folder").val(s_current_path);
}

function getCurrentPath() {
    var a_path = [];
    $("ul.navigator li").each(
        function () {
            a_path.push($(this).attr("data-urlpath"));
        }
    );

    return "/" + a_path.join("/");
}

function addBreadcrumb(folderinfo) {
    var n_count = $("ul.navigator li").length;
    $("ul.navigator").append("<li data-level=\"" + n_count + "\" data-safepath=\"" + folderinfo.SafePath + "\" data-urlpath=\"" + folderinfo.Name + "\"><a href=\"javascript:navigateBreadcrumb(" + n_count + ",&quot;" + folderinfo.SafePath + "&quot;)\">" + folderinfo.Name + "</a></li>");
}

function navigateBreadcrumb(level, s_folder) {

    if (level == 0)
        $("ul.navigator li").remove();
    else
        $("ul.navigator li:gt(" + (level - 1) + ")").remove();

    navigatePath(s_folder);

}

function navigatePriorBreadcrumb() {

    var n_count = $("ul.navigator li").length;
    if (n_count == 1) // cannot delete base attachments directory
        return;

    var li = $("ul.navigator li:last-child").prev("li");
    var n_level = li.attr("data-level");
    var s_folder = li.attr("data-safepath");

    navigateBreadcrumb(n_level, s_folder);

}

function handleRowSelection(tr) {

    if ($(tr).attr("data-itemtype") == "folder") {
        navigatePath($(tr).attr("data-itemid"));
    } else {
        $("table#files tr.select").removeClass("select");
        $(tr).addClass("select");
        $("table#files").trigger("fileselected", { file: getCurrentPath() + "/" + $("td.file", tr).text() });
    }

}

function buildTableFolderView(data) {

    var a_html = ["<table id=\"files\"><thead><tr><th colspan=2>Name</th><th>Date Uploaded</th><th>Type</th><th>Size</th></tr></thead>"];

    for (var i = 0; i < data.ChildFolders.length; i++) {
        a_html.push("<tr class=\"listrow\" data-itemtype=\"folder\" data-itemid=\"" + data.ChildFolders[i].SafePath + "\"><td width='1%'><img src='" + ROADKILL_COREASSETPATH + "CSS/images/directory.png'></td><td nowrap width=\"20%\">" + data.ChildFolders[i].Name + "</td><td></td><td></td><td></td></tr>");
    }
    for (var i = 0; i < data.Files.length; i++) {
        a_html.push(getFileRowHtml(data.Files[i]));
    }
    a_html.push("</table>");

    $("#folder-container").empty().append(a_html.join(""));
}

function getFileRowHtml(file) {

    var s_html = "<tr class=\"listrow\" data-itemtype=\"file\"><td width='1%'><img src='" + ROADKILL_COREASSETPATH + "CSS/images/file.png'></td><td class=\"file\">{0}</td><td>{1}</td><td class=filetype>{2}</td><td class=filesize>{3}</td></tr>";

    return s_html.format(file.Name, file.CreateDate, file.Extension, file.Size);

}

function imagePreviewInit() {

    var xOffset = 20;
    var yOffset = 20;

    $("table#files tr[data-itemtype=file]")
        .live("mouseenter",
		    function (e) {
		        var s_file_type = $("td.filetype", this).text();
		        if (s_file_type.search(/^(jpg|png|gif)$/i) == -1)
		            return;
		        var s_img_url = (ROADKILL_BASEPATH + getCurrentPath() + "/").replace("//", "/") + $("td.file", this).text();
		        $("body").append("<p id='image-preview'><img src='" + s_img_url + "' alt='Image Preview' /></p>");
		        $("#image-preview")
				    .css("top", (e.pageY - xOffset) + "px")
				    .css("left", (e.pageX + yOffset) + "px")
				    .fadeIn("fast");
		    })
        .live("mouseleave",
		    function () {
		        $("#image-preview").remove();
		    })
        .live("mousemove",
    		function (e) {
    		    $("#preview")
	    			.css("top", (e.pageY - xOffset) + "px")
		    		.css("left", (e.pageX + yOffset) + "px");
    		});
};



















//$(document).ready(function ()
//{
//	initFileManager();
//	bindFileButtons();
//});

/**
Binds all buttons with event handlers.
*/
//function initFileManager()
//{
//	// Limit the bottom right image preview to 40x40px
//	$("#previewimage").aeImageResize({ height: 40, width: 40 });
//	$("#previewimage").hide();

//	// Choose button calls the 'addImage' function from wyswyg.js
//	$("#choosebutton").click(function ()
//	{
//		window.top.addImage($("#previewimage").attr("src").replace(ROADKILL_ATTACHMENTSPATH, ""));
//	});

//	// Setup the jquery filetree
//	$("#filetree-container").fileTree(
//	{
//		root: "",
//		script: ROADKILL_FILETREE_URL,
//		onFolderClick: function (path, name)
//		{
//			$(".selectedfolder").text("Adding to folder: " + name);
//			$(".selectedfolder").show();
//			$("#currentUploadFolderPath").val(path);
//			$("#currentFolderPath").val(path);
//		}
//	},function (filePath, name)
//	{
//		// When a file is selected, setup the bottom right preview
//		$.get(ROADKILL_FILETREE_PATHNAME_URL + filePath, function (urlPath)
//		{
//			$("#previewimage").attr("src", ROADKILL_ATTACHMENTSPATH + urlPath);
//			$("#previewimage").resize();
//			$("#previewimage").show();
//			$("#previewname").html(name);
//			$("#file-preview").show();
//		});
//	});
//}

/**
Show/hide the upload and new directory hidden divs.
*/
//function bindFileButtons()
//{
//	$("#uploadfile").click(function ()
//	{
//		var copy = $("#uploadfile-container").clone();
//		copy.show();
//		$("#generic-container").html(copy.html());
//		$("#generic-container").show();
//	});

//	$("#newdirectory").click(function ()
//	{
//		var copy = $("#newfolder-container").clone();
//		copy.show();
//		$("#generic-container").html(copy.html());
//		$("#generic-container").show();
//	});
//}