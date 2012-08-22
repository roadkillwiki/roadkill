/// <reference path="jquery-1.8.0-vsdoc.js" />

var _tags;
var _loadTagsTimer;

/**
Event bindings and handlers for the edit page/
*/
$(document).ready(function ()
{
	$.require("tag-it.js");
	$.require("roadkill.wysiwyg.js");

	// Tag box
	initTagIt();
	_loadTagsTimer = setTimeout("loadTags();", 2000);

	// Toolbar
	initWYSIWYG(); // inside wysiwyg.js

	// Preview modal preview
	$(".previewButton").click(function ()
	{
		showPreview();
	});
});

/**
Sets up the tag box with tagit
*/
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

/**
Loads all tags with a JSON AJAX request.
*/
function loadTags()
{
	$.get(ROADKILL_TAGAJAXURL, function (data)
	{
		_tags = eval(data);
		initTagIt();
		clearTimeout(_loadTagsTimer);
	});
}

/**
Grabs a preview from the server for the wiki markup, and displays it in the preview modal (as an iframe) 
*/
function showPreview()
{
	$("#previewLoading").show();
	var text = $("#Content").val();

	$.ajax({ 
		type: "POST",
		url: ROADKILL_PREVIEWURL, 
		data: { "id": text },
		cache: false})
		.done(function (htmlResult)
		{
			$("#preview").html(htmlResult);
			$("#preview").show();
			openModal("#previewContainer");
			$("#previewLoading").hide();
		});
}