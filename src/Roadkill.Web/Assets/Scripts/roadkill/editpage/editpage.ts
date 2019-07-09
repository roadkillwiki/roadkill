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
        private _tuiEditor: any = null;
        private _baseURL: string = '';

		constructor(tags : string[], tuiEditor, baseURL)
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

            this._tuiEditor = tuiEditor;
            this._baseURL = baseURL;
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

        /**
        A Custom markdownIt renderer rule for roadkill code fences  
        */
        public rk_fence_renderer = (tokens, idx, options, env, slf) => {
            var token = tokens[idx],
                info = token.info ? this._tuiEditor.markdownitHighlight.utils.unescapeAll(token.info).trim() : '',
                langName = '',
                highlighted, i, tmpAttrs, tmpToken;

            if (info) {
              langName = info.split(/\s+/g)[0];
            }

            highlighted = this._tuiEditor.markdownitHighlight.utils.escapeHtml(token.content);

            if (highlighted.indexOf('<pre') === 0) {
              return highlighted + '\n';
            }

            // If language exists
            if (info) {
                // Load the brush
              return  '<pre class="brush: ' + langName + '">'
                    + highlighted
                    + '</pre>\n';
            }

            // Language doesn't exist, just copy the attrs
            return  '<pre><code' + slf.renderAttrs(token) + '>'
                  + highlighted
                  + '</code></pre>\n';

        }

        /**
        Custom markdownIt image renderer rule for roadkill-style urls in img src values
        */
        public rk_image_renderer = (tokens, idx, options, env, slf) => {
            var token = tokens[idx];
            token.attrs[token.attrIndex('alt')][1] =
                slf.renderInlineAsText(token.children, options, env);

            var src = token.attrs[token.attrIndex('src')][1];
            token.attrs[token.attrIndex('src')][1] = src.replace(/(.+)/, this._baseURL + 'Attachments$1');

            return slf.renderToken(tokens, idx, options);
        }

        /**
        Roadkill-style code block (fence) tokenizer rule
        */
        public rk_fence(state, startLine, endLine, silent) {
            var optionMarker = '[';
            var pos = state.bMarks[startLine] + state.tShift[startLine];
            var max = state.eMarks[startLine];
            var haveEndMarker = false;

            if (state.sCount[startLine] - state.blkIndent >= 4) return false;
            if (pos + 3 > max) return false;

            var marker = state.src.charCodeAt(pos);

            if (marker !== optionMarker.charCodeAt(0)) return false;

            var mem = pos;
            pos = state.skipChars(pos, marker);
            var len = pos - mem;

            if (len < 3) return false;

            // KW If we're interrupting an element here, don't bother pushing the token
            if (silent) { return true; }

            var markup = state.src.slice(mem, pos);
            var params = state.src.slice(pos, max).replace(/code.*lang=([^|]*)\|/, '$1');

            // Skip jumbotron blocks
            if (/.*jumbotron.*/.test(params)) {
                return false;
            }

            if (params.indexOf(String.fromCharCode(marker)) >= 0) return false;

            // Search for end of block
            var nextLine = startLine;
            marker = 93; /* ] */

            for (;;) {
              nextLine++;
              if (nextLine >= endLine) break;

              pos = mem = state.bMarks[nextLine] + state.tShift[nextLine];
              max = state.eMarks[nextLine];

              if (pos < max && state.sCount[nextLine] < state.blkIndent) break;
              if (state.src.charCodeAt(pos) !== marker) continue;
              if (state.sCount[nextLine] - state.blkIndent >= 4) continue;

              pos = state.skipChars(pos, marker);

              if (pos - mem < len) continue;

              pos = state.skipSpaces(pos);

              if (pos < max) continue;

              haveEndMarker = true;

              break;
            }

            len = state.sCount[startLine];
            state.line = nextLine + (haveEndMarker ? 1 : 0);

            var token = state.push('fence', 'code', 0);
            token.info = params;
            token.content = state.getLines(startLine + 1, nextLine, len, true);
            token.markup = markup;
            token.map = [startLine, state.line];

            return true;
        }
	}
}