/// <reference path="typescript-ref/references.ts" />
module Roadkill.Site
{
	/**
	Event bindings and handlers for the edit text area toolbar.
	*/
	export class WysiwygEditor
	{
		bindEvents()
		{
			var parent = this;

			// Bind the toolbar button clicks.
			$(".wysiwyg-bold").click(function ()
			{
				parent.addStyling(ROADKILL_EDIT_BOLD_TOKEN);
			});
			$(".wysiwyg-italic").click(function ()
			{
				parent.addStyling(ROADKILL_EDIT_ITALIC_TOKEN);
			});
			$(".wysiwyg-underline").click(function ()
			{
				parent.addStyling(ROADKILL_EDIT_UNDERLINE_TOKEN);
			});
			$(".wysiwyg-h1").click(function ()
			{
				parent.addHeading(ROADKILL_EDIT_HEADING_TOKEN);
			});
			$(".wysiwyg-h2").click(function ()
			{
				parent.addHeading(parent.repeat(ROADKILL_EDIT_HEADING_TOKEN, 2));
			});
			$(".wysiwyg-h3").click(function ()
			{
				parent.addHeading(parent.repeat(ROADKILL_EDIT_HEADING_TOKEN, 3));
			});
			$(".wysiwyg-h4").click(function ()
			{
				parent.addHeading(parent.repeat(ROADKILL_EDIT_HEADING_TOKEN, 4));
			});
			$(".wysiwyg-h5").click(function ()
			{
				parent.addHeading(parent.repeat(ROADKILL_EDIT_HEADING_TOKEN, 5));
			});
			$(".wysiwyg-bullets").click(function ()
			{
				parent.addListItem(ROADKILL_EDIT_BULLETLIST_TOKEN);
			});
			$(".wysiwyg-numbers").click(function ()
			{
				parent.addListItem(ROADKILL_EDIT_NUMBERLIST_TOKEN);
			});
			$(".wysiwyg-picture").click(function ()
			{
				Dialogs.openIFrameModal("<iframe src='" + ROADKILL_FILESELECTURL + "' id='filechooser-iframe'></iframe>");
			});
			$(".wysiwyg-link").click(function ()
			{
				parent.addLink();
			});
			$(".wysiwyg-help").click(function ()
			{
				Dialogs.openIFrameModal("<iframe src='" + ROADKILL_WIKIMARKUPHELP + "' id='help-iframe'></iframe>");
			});
		}

		/**
		Adds bold,italic and underline at the current selection point, e.g. **|**
		*/
		addStyling(styleCode:string)
		{
			var range = $("#Content").getSelection();

			if (range !== null)
			{
				var text = $("#Content").val();
				if (text.substr(range.start - 2, 2) !== styleCode && range.text.substr(0, 2) !== styleCode)
				{
					$("#Content").replaceSelection(styleCode + range.text + styleCode);
					$("#Content").setSelection(range.end + 2, range.end + 2);
				}
				else
				{
					$("#Content").setSelection(range.end, range.end);
				}

			}
		}

		/**
		Adds a heading before and after the current selection point, e.g. ===|====
		*/
		addHeading(styleCode:string)
		{
			var range = $("#Content").getSelection();

			if (range !== null)
			{
				var text = range.text;
				if (range.text === "")
					text = "Your heading";

				$("#Content").replaceSelection("\n" + styleCode + text + styleCode + "\n");
				$("#Content").setSelection(range.end, range.end);
			}
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
				Dialogs.closeModal2("#iframe-dialog");
			}
		}

		/**
		Adds a hyperlink tag to the current caret location.
		*/
		addLink()
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
		addListItem(styleCode:string)
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

		repeat(text: string, count:number)
		{
			return new Array(count + 1).join(text);
		}
	}
}