/// <reference path="jquery-1.4.1-vsdoc.js" />
$(document).ready(function ()
{
	initFileManager();
	bindFileButtons();
});

function initFileManager()
{
	$("#previewimage").aeImageResize({ height: 40, width: 40 });
	$("#previewimage").hide();

	$("#choosebutton").click(function()
	{
		window.top.addImage($("#previewimage").attr("src").replace(ROADKILL_ATTACHMENTSPATH, ""));
		self.close();
	});

	$("#filetree-container").fileTree(
	{
		root: "",
		script: ROADKILL_FILETREE_URL,
		onFolderClick: function (path, name)
		{
			$(".selectedfolder").html("Adding to folder: " + name);
			$("#currentUploadFolderPath").val(path);
			$("#currentFolderPath").val(path);
		}
	},function (filePath, name)
	{
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