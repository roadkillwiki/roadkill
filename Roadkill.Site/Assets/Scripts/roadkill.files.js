/// <reference path="jquery-1.4.1-vsdoc.js" />
$(document).ready(function ()
{
	initFileManager();
	bindFileButtons();
});

function initFileManager()
{
	$("#file-preview img").aeImageResize({ height: 40, width: 40 });
	$("#file-preview img").hide();

	$("#choosebutton").click(function()
	{
		window.top.addImage($("#file-preview img").attr("src").replace(ROADKILL_ATTACHMENTSPATH,""));
		self.close();
	});

	$("#filetree-container").fileTree(
	{
		root: "",
		script: ROADKILL_FILETREE_URL,
		onFolderClick : function(path,name){
			$(".selectedfolder").html("Adding to folder: "+name);
			$("#currentUploadFolderPath").val(path);
			$("#currentFolderPath").val(path);
		}
	}, 
	function (filePath,name)
	{
		$.get(ROADKILL_FILETREE_PATHNAME_URL + filePath, function (urlPath)
		{
			$("#file-preview img").attr("src",ROADKILL_ATTACHMENTSPATH + urlPath);
			$("#file-preview img").resize();
			$("#file-preview img").show();
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