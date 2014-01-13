/// <reference path="../typescript-ref/references.ts" />
module Roadkill.Web
{
	/**
	Event bindings and handlers for the edit text area toolbar.
	*/
	export class WysiwygEditor
	{
		bindEvents()
		{
			// Bind the toolbar button clicks.
			$(".wysiwyg-bold").click((e) =>
			{
				this.insertStyle(ROADKILL_EDIT_BOLD_TOKEN);
				return false;
			});
			$(".wysiwyg-italic").click((e) =>
			{
				this.insertStyle(ROADKILL_EDIT_ITALIC_TOKEN);
				return false;
			});
			$(".wysiwyg-underline").click((e) =>
			{
				this.insertStyle(ROADKILL_EDIT_UNDERLINE_TOKEN);
				return false;
			});
			$(".wysiwyg-h2").click((e) =>
			{
				this.insertStyle(this.repeat(ROADKILL_EDIT_HEADING_TOKEN, 2));
				return false;
			});
			$(".wysiwyg-h3").click((e) =>
			{
				this.insertStyle(this.repeat(ROADKILL_EDIT_HEADING_TOKEN, 3));
				return false;
			});
			$(".wysiwyg-h4").click((e) =>
			{
				this.insertStyle(this.repeat(ROADKILL_EDIT_HEADING_TOKEN, 4));
				return false;
			});
			$(".wysiwyg-h5").click((e) =>
			{
				this.insertStyle(this.repeat(ROADKILL_EDIT_HEADING_TOKEN, 5));
				return false;
			});
			$(".wysiwyg-bullets").click((e) =>
			{
				this.insertListItem(ROADKILL_EDIT_BULLETLIST_TOKEN);
				return false;
			});
			$(".wysiwyg-numbers").click((e) =>
			{
				// Obselete
				this.insertListItem(ROADKILL_EDIT_NUMBERLIST_TOKEN);
				return false;
			});
			$(".wysiwyg-picture").click((e) =>
			{
				Dialogs.openImageChooserModal("<iframe src='" + ROADKILL_FILESELECTURL + "' id='filechooser-iframe'></iframe>");
				return false;
			});
			$(".wysiwyg-link").click((e) =>
			{
				this.insertLink();
				return false;
			});
			$(".wysiwyg-help").click((e) =>
			{
				Dialogs.openMarkupHelpModal("<iframe src='" + ROADKILL_WIKIMARKUPHELP + "' id='help-iframe'></iframe>");
				return false;
			});
		}

		/**
		Adds bold,italic and underline at the current selection point, e.g. **|**
		*/
		private insertStyle(styleCode:string)
		{
			var range = $("#Content").getSelection();
			var length: number = styleCode.length;

			if (range !== null)
			{
				var editorText: string = $("#Content").val();

				// Put the caret in the middle of the style, if the current selection isn't the style being added
				if (editorText.substr(range.start - length, length) !== styleCode && range.text.substr(0, length) !== styleCode)
				{
					$("#Content").replaceSelection(styleCode + range.text + styleCode);
					$("#Content").setSelection(range.end + length, range.end + length);
				}
				else
				{
					$("#Content").setSelection(range.end, range.end);
				}
			}
		}

		/**
		Adds a hyperlink tag to the current caret location.
		*/
		private insertLink()
		{
			var range = $("#Content").getSelection();

			if (range !== null)
			{
				var text = range.text;
				if (range.text === "")
					text = ROADKILL_EDIT_LINK_TEXTPLACEHOLDER;

				var prefix = ROADKILL_EDIT_LINK_STARTTOKEN.toString();
				prefix = prefix.replace("%URL%", ROADKILL_EDIT_LINK_URLPLACEHOLDER);
				prefix = prefix.replace("%LINKTEXT%", text);

				var suffix = ROADKILL_EDIT_LINK_ENDTOKEN.toString();
				suffix = suffix.replace("%URL%", ROADKILL_EDIT_LINK_URLPLACEHOLDER);
				suffix = suffix.replace("%LINKTEXT%", text);

				$("#Content").replaceSelection(prefix + suffix);
				$("#Content").setSelection(range.start + prefix.length, range.start + prefix.length);
			}
		}

		/**
		Adds a bullet or numbered list item onto the next line after the current caret location.
		*/
		private insertListItem(styleCode:string)
		{
			var range = $("#Content").getSelection();

			if (range !== null)
			{
				var val = $("#Content").val();
				var start = range.start;
				if (start > 0)
					start -= 1;

				var lastChar = val.substr(start, 1);
				var nextChar = val.substr(range.start, 1);

				if (nextChar === styleCode)
				{
					$("#Content").setSelection(range.end + 2, range.end + 2);
					return;
				}

				if (lastChar == "\n" || lastChar == "")
				{
					$("#Content").replaceSelection(range.text + styleCode + " ");
					$("#Content").setSelection(range.end + 2, range.end + 2);
				}
				else
				{
					$("#Content").replaceSelection(range.text + "\n" + styleCode + " ");
					$("#Content").setSelection(range.end + 3, range.end + 3);
				}
			}
		}

		private repeat(text: string, count:number)
		{
			return new Array(count + 1).join(text);
		}

		/**
		Adds an image tag to the current caret location.
		*/
		public static addImage(image: string)
		{
			var range = $("#Content").getSelection();

			if (range !== null)
			{
				var text = range.text;
				if (range.text === "")
					text = ROADKILL_EDIT_IMAGE_TITLE;

				var prefix = ROADKILL_EDIT_IMAGE_STARTTOKEN.toString();
				prefix = prefix.replace("%ALT%", text);
				prefix = prefix.replace("%FILENAME%", image);

				var suffix = ROADKILL_EDIT_IMAGE_ENDTOKEN.toString();
				suffix = suffix.replace("%ALT%", text);
				suffix = suffix.replace("%FILENAME%", image);

				$("#Content").replaceSelection(prefix + suffix);
				$("#Content").setSelection(range.start + prefix.length, range.start + prefix.length);
				Dialogs.closeImageChooserModal();

				EditPage.updatePreviewPane();
			}
		}
	}
}