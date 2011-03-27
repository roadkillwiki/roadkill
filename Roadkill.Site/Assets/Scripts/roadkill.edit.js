/// <reference path="jquery-1.4.1-vsdoc.js" />
var _tags;
var _loadTagsTimer;

$(document).ready(function ()
{
	$.require("tag-it.js");
	$.require("roadkill.wysiwyg.js");

	initTagIt();
	_loadTagsTimer = setTimeout("loadTags();", 2000);

	bindEditButtons();
	initWYSIWYG();

	$("#Content").keyup(function ()
	{
		$("#previewContainer").hide();
	});
	$(".previewButton").click(function ()
	{
		showPreview();
	});
});

function initTagIt()
{
	$("#mytags").tagit({
		tabIndex : 2,
		availableTags	: _tags,
		singleField		: true,
		singleFieldNode	: $("#Tags"),
		singleFieldDelimiter: ";"
	});
}

function loadTags()
{
	$.get(ROADKILL_TAGAJAXURL, function (data)
	{
		_tags = eval(data);
		initTagIt();
		clearTimeout(_loadTagsTimer);
	});
}

function bindEditButtons()
{
	$("#Content").keyup(function ()
	{
		$("#previewContainer").hide();
	});
	$(".previewButton").click(function ()
	{
		showPreview();
	});
}

function showPreview()
{
	$("#previewLoading").show();
	var text = $("#Content").val();

	$.ajax({
		type: "POST",
		url: ROADKILL_PREVIEWURL,
		data: { "id": text },
		success: function (htmlResult)
		{
			$("#preview").html(htmlResult);
			$("#previewContainer").modal();
			$("#previewLoading").hide();
		}
	});
}