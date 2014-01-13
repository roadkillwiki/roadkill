/// <reference path="../typescript-ref/references.ts" />
module Roadkill.Web
{
	export class EditPage
	{
		private _timeout: any = null;
		private _tagBlackList: string[] = 
		[
			"#", ",", ";", "/", "?", ":", "@", "&", "=", "{", "}", "|", "\\", "^", "[", "]", "`"	
		];

		constructor(tags : string[])
		{
			// Setup tagmanager
			this.initializeTagManager(tags);

			// Bind all the button events
			var editor = new WysiwygEditor();
			editor.bindEvents();

			// Setup the help popovers for the buttons
			$("#wysiwyg-toolbar button").popover({ trigger: "hover", html: false, delay: { show: 250, hide: 100 } });
			
			// Set the preview pane to auto-update
			this.bindPreview();

			// Set the preview pane toggle button
			this.bindPreviewToggleButton();

			// Form validation
			var validationRules =
				{
					Title: {
						required: true
					}
				};
			var validation = new Roadkill.Web.Validation();
			validation.Configure("#editpage-form", validationRules);
		}

		/**
		Sets up the Bootstrap tag manager
		*/
		private initializeTagManager(tags: string[])
		{
			// Use jQuery UI autocomplete, as typeahead is currently broken for BS3
			$("#TagsEntry").autocomplete({
				source: tags
			});

			$("#TagsEntry").tagsManager({
				tagClass: "tm-tag-success",
				blinkBGColor_1: "#FFFF9C",
				blinkBGColor_2: "#CDE69C",
				delimeters: [44, 186, 32, 9], // comma, ";", space, tab
				output: "#RawTags",
				preventSubmitOnEnter: false,
				validator: (input: string) =>
				{
					var isValid: Boolean = this.isValidTag(input);
					if (isValid === false)
					{
						toastr.error("The following characters are not valid for tags: <br/>" + this._tagBlackList.join(" "));
					}

					return isValid;
				}
			});

			$("#TagsEntry").keydown((e) =>
			{
				// Tab adds the tag, but then focuses the toolbar (the next tab index)
				var code = e.keyCode || e.which;
				if (code == "9")
				{
					var tag: string = $("#TagsEntry").val();
					if (this.isValidTag(tag))
					{
						if ($("#IsLocked").length == 0)
							$(".wysiwyg-bold").focus();
						else
							$("#IsLocked").focus();
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
		private isValidTag(tag: string) : Boolean
		{
			for (var i: number = 0; i < tag.length; i++)
			{
				if ($.inArray(tag[i], this._tagBlackList) > -1)
				{
					return false;
				}
			}

			return true;
		}

		private bindPreview()
		{
			this.resizePreviewPane();
			EditPage.updatePreviewPane();

			$(document).on("resize", () =>
			{
				this.resizePreviewPane();
			});

			// Keydown fires the preview after 1/100th second, but each keypress resets this.
			$("#Content").on("keydown", () =>
			{
				if (this._timeout !== null)
				{
					clearTimeout(this._timeout);
					this._timeout = null;
				}

				this._timeout = setTimeout(EditPage.updatePreviewPane, 100);
			});
		}

		private bindPreviewToggleButton()
		{
			$("#preview-toggle").click(function ()
			{
				// Switch the bootstrap classes so the form area fills or collapses
				var panelContainer = $("#previewpanel-container");

				if (panelContainer.is(":visible"))
				{
					// Hide the preview
					$("#preview-toggle span")
						.removeClass("glyphicon-chevron-right")
						.addClass("glyphicon-chevron-left");

					$("#editpage-form-container")
						.removeClass("col-lg-6")
						.addClass("col-lg-12");

					$("#previewpanel-container")
						.removeClass("col-lg-6")
				}
				else
				{
					// Show the preview
					$("#preview-toggle span")
						.removeClass("glyphicon-chevron-left")
						.addClass("glyphicon-chevron-right");

					$("#editpage-form-container")
						.removeClass("col-lg-12")
						.addClass("col-lg-6");

					$("#previewpanel-container")
						.addClass("col-lg-6");
				}

				panelContainer.toggle();
				return false;
			});
		}

		private resizePreviewPane()
		{
			// Height fix for CSS heights sucking
			$("#Content").height($("#container").height());

			var previewTitleHeight: number = $("#preview-heading").outerHeight(true); // true to include margin
			var buttonsHeight: number = $("#editpage-button-container").outerHeight(true);
			var scrollbarHeight: number = 36; // top and bottom scrollbars
			var formHeight: number = $("#editpage-form-container").outerHeight(true) - (buttonsHeight + scrollbarHeight + previewTitleHeight);

			$("#preview-wrapper").height(formHeight);
		}

		/**
		Grabs a preview from the server for the wiki markup, and displays it in the preview pane.
		*/
		public static updatePreviewPane()
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
				$("#previewLoading").hide();
			});
		}
	}
}