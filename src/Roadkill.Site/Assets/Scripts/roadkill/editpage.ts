/// <reference path="typescript-ref/references.ts" />
module Roadkill.Site
{
	$(document).ready(function ()
	{
		// Event bindings and handlers for the edit page

		var editor = new WysiwygEditor();
		editor.bindEvents();

		EditPage.bindPreview();
	});

	export class EditPage
	{
		private static _timeout: any = null;
		private static _tagBlackList: string[] = 
		[
			"#", ",", ";", "/", "?", ":", "@", "&", "=", "{", "}", "|", "\\", "^", "[", "]", "`"	
		];

		/**
		Sets up the Bootstrap tag manager
		*/
		public static initializeTagManager(tags)
		{
			// Use jQuery UI autocomplete, as typeahead is currently broken for BS3
			$("#TagsEntry").autocomplete({
				source: ROADKILL_TAGAJAXURL
			});

			$("#TagsEntry").tagsManager({
				prefilled: tags,
				tagClass: "tm-tag-success",
				blinkBGColor_1: "#FFFF9C",
				blinkBGColor_2: "#CDE69C",
				delimeters: [44, 186, 32, 9], // comma, ";", space, tab
				hiddenTagListName: "RawTags",
				preventSubmitOnEnter: false,
				validator: function (input: string)
				{
					var isValid: Boolean = EditPage.isValidTag(input);
					if (isValid === false)
					{
						toastr.error("The following characters are not valid for tags: <br/>" + EditPage._tagBlackList.join(" "));
					}

					return isValid;
				}
			});

			$("#TagsEntry").keydown(function (e)
			{
				// Tab adds the tag, but then focuses the textarea
				var code = e.keyCode || e.which;
				if (code == "9")
				{
					var tag: string = $("#TagsEntry").val();
					if (EditPage.isValidTag(tag))
					{
						$("#Content").focus();
					}
					return false;
				}

				return true;
			});

			$("#TagsEntry").blur(function (e)
			{
				// Push the tag when focus is lost, e.g. Save is pressed
				$("#TagsEntry").tagsManager("pushTag", $("#TagsEntry").val());

				// Fix the tag's styles from being blank
				$(".tm-tag-remove").each(function ()
				{
					$(this).html("&times;");
				});
				$(".tm-tag").each(function ()
				{
					$(this).addClass("tm-tag-success");
					$(this).addClass("tm-success");
				});
			});
		}

		/**
		 Returns false if the tag contains any characters that are blacklisted.
		*/
		public static isValidTag(tag: string) : Boolean
		{
			for (var i: number = 0; i < tag.length; i++)
			{
				if ($.inArray(tag[i], EditPage._tagBlackList) > -1)
				{
					return false;
				}
			}

			return true;
		}

		public static bindPreview()
		{
			EditPage.setElementHeights();
			EditPage.showPreview();

			$(document).on("resize", function () {
				EditPage.setElementHeights();
			});

			// Keydown fires the preview after 1/100th second, but each keypress resets this.
			$("#Content").on("keydown", function ()
			{
				if (EditPage._timeout !== null)
				{
					clearTimeout(EditPage._timeout);
					EditPage._timeout = null;
				}

				EditPage._timeout = setTimeout(EditPage.showPreview, 100);
			});
		}

		public static setElementHeights()
		{
			// Height fix for CSS heights sucking
			$("#Content").height($("#container").height());

			var previewTitleHeight: number = $("#preview-heading").outerHeight(true) +20 ; // 26 is the magic number
			var buttonsHeight: number = $("#editpage-button-container").height();
			var scrollbarHeight: number = 10;
			var formHeight: number = $("#editpage-form").height() - (buttonsHeight + scrollbarHeight + previewTitleHeight);

			$("#preview-wrapper").height(formHeight);
		}

		/**
		Grabs a preview from the server for the wiki markup, and displays it in the preview pane.
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
				//Dialogs.openFullScreenModal("#previewContainer");
				$("#previewLoading").hide();
			});
		}
	}
}