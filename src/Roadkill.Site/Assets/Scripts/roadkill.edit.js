/// <reference path="jquery-1.8.0-vsdoc.js" />
/// <reference path="..\bootstrap\js\bootstrap.js" />

/**
Event bindings and handlers for the edit page/
*/
$(document).ready(function ()
{
	// Toolbar
	initWYSIWYG(); // inside wysiwyg.js

	// Preview modal preview
	$(".previewButton").click(function ()
	{
		showPreview();
	});
});

/**
Sets up the tags
*/
function initTagsManager(tags)
{
	$("#TagsEntry").typeahead({});
	$("#TagsEntry").tagsManager({
		prefilled: tags,
		preventSubmitOnEnter: true,
		typeahead: true,
		typeaheadAjaxSource: ROADKILL_TAGAJAXURL,
		blinkBGColor_1: "#FFFF9C",
		blinkBGColor_2: "#CDE69C",
		delimeters: [59, 186, 32],
		hiddenTagListName: "RawTags"
	});
}

/**
Grabs a preview from the server for the wiki markup, and displays it in the preview modal (as an iframe) 
*/
function showPreview()
{
	$("#previewLoading").show();
	var text = $("#Content").val();

	var request = $.ajax({
		type: "POST",
		url: ROADKILL_PREVIEWURL,
		data: { "id": text },
		cache: false,
		dataType: "text"
	});
	
	request.done(function (htmlResult)
	{
		$("#preview").html(htmlResult);
	});

	request.fail(function (jqXHR, textStatus, errorThrown)
	{
		$("#preview").html("<span style='color:red'>An error occurred with the preview: "+errorThrown+"</span>");
	});
	
	request.always(function ()
	{
		$("#preview").show();
		openModal("#previewContainer");
		$("#previewLoading").hide();
	});
}