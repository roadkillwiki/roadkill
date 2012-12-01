/// <reference path="jquery-1.8.0-vsdoc.js" />

/**
 Event bindings and handlers for the file manager.
*/
$(document).ready(function ()
{
	initFileManager();
	bindFileButtons();
});

/**
Binds all buttons with event handlers.
*/
function initFileManager()
{
	// Limit the bottom right image preview to 40x40px
	$("#previewimage").aeImageResize({ height: 40, width: 40 });
	$("#previewimage").hide();

	// Choose button calls the 'addImage' function from wyswyg.js
	$("#choosebutton").click(function ()
	{
		window.top.addImage($("#previewimage").attr("src").replace(ROADKILL_ATTACHMENTSPATH, ""));
	});

	// Setup the jquery filetree
	$("#filetree-container").fileTree(
	{
		root: "",
		script: ROADKILL_FILETREE_URL,
		onFolderClick: function (path, name)
		{
			$(".selectedfolder").text("Adding to folder: " + name);
			$(".selectedfolder").show();
			$("#currentUploadFolderPath").val(path);
			$("#currentFolderPath").val(path);
		}
	},function (filePath, name)
	{
		// When a file is selected, setup the bottom right preview
		$.get(ROADKILL_FILETREE_PATHNAME_URL + filePath, function (urlPath)
		{
			$("#previewimage").attr("src", ROADKILL_ATTACHMENTSPATH + urlPath);
			$("#previewimage").resize();
			$("#previewimage").show();
			$("#previewname").html(name);
			$("#file-preview").show();
		});
	});
}

/**
Show/hide the upload and new directory hidden divs.
*/
function bindFileButtons()
{
	$("#uploadfile").click(function ()
	{
		var copy = $("#uploadfile-container").clone();
		copy.show();
		$("#generic-container").html(copy.html());
		$("#generic-container").show();
	});

	$("#newdirectory").click(function ()
	{
		var copy = $("#newfolder-container").clone();
		copy.show();
		$("#generic-container").html(copy.html());
		$("#generic-container").show();
	});
}