/// <reference path="typescript-ref/references.ts" />
module Roadkill.Site
{
	$(document).ready(function ()
	{
		// Event bindings and handlers for the edit page

		var editor = new WysiwygEditor();
		editor.bindEvents();

		EditPage.bindPreviewButton();
	});

	export class EditPage
	{
		/**
		Sets up the tags
		*/
		public static initializeTagManager(tags)
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

		public static bindPreviewButton()
		{
			// Preview modal preview
			$(".previewButton").click(function ()
			{
				EditPage.showPreview();
			});
		}

		/**
		Grabs a preview from the server for the wiki markup, and displays it in the 
		preview modal (as an iframe) 
		*/
		public static showPreview()
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
				$("#preview").html("<span style='color:red'>An error occurred with the preview: " + errorThrown + "</span>");
			});

			request.always(function ()
			{
				$("#previewLoading").show();
				Dialogs.openFullScreenModal("#previewContainer");
				$("#previewLoading").hide();
			});
		}
	}
}