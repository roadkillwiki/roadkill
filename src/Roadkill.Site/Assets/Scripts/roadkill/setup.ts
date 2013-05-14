/// <reference path="references.ts" />
module Roadkill.Site
{
	$(document).ready(function ()
	{
		Setup.bindInfoButton();
		Setup.bindTimeAgo();
		Setup.bindTocLinks();
	});

	/**
	Event bindings and handlers for all pages.
	*/
	export class Setup 
	{
		public static bindTimeAgo()
		{
			// Friendly times for the history tables
			$("#historytable .editedon").timeago();
		}

		public static bindInfoButton()
		{
			// Bind the info icon on each page
			$("#pageinfo-button").click(function ()
			{
				Dialogs.openModal("#pageinformation");
			});
		}

		public static bindTocLinks()
		{
			// The show/hide for table of contents
			$("a.toc-showhide").click(function ()
			{
				if ($(this).text() == "hide")
				{
					$(this).text("show");
				}
				else
				{
					$(this).text("hide");
				}

				$(this).parent().next().toggle();
			});
		}

		public static resizeImage(img, maxWidth:number, maxHeight:number)
		{
			if (maxWidth < 1)
				maxWidth = 400;

			if (maxHeight < 1)
				maxHeight = 400;

			var ratio = 0;
			var width = $(img).width();
			var height = $(img).height();

			if (width > maxWidth)
			{
				// Use the width ratio to start with
				ratio = maxWidth / width;
				width = width * ratio;
				height = height * ratio;

				$(img).css("width", width);
				$(img).css("height", height);
			}

			if (height > maxHeight)
			{
				// Scale down to the height ratio if it's still too large
				ratio = maxHeight / height;
				$(img).css("width", width * ratio);
				$(img).css("height", height * ratio)
			}
		}

		/**
		Sets all links with the .confirm class so they have to click confirm to 
		delete or the link is cancelled.
		*/
		public static bindConfirmDelete()
		{
			$("a.confirm").click(function ()
			{
				var button;
				var value;
				var text;
				button = $(this);

				if (!button.hasClass("jqConfirm"))
				{
					value = button.val();
					text = button.text();

					button.val(ROADKILL_LINK_CONFIRM);
					button.text(ROADKILL_LINK_CONFIRM);
					button.addClass("jqConfirm");

					var handler = function() : bool
					{
						button.removeClass("jqConfirm");
						button.val(value);
						button.text(text);
						button.unbind("click.jqConfirmHandler");
						return true;
					};
					button.bind("click.jqConfirmHandler", handler);

					setTimeout(function () { handler(); }, 3000);

					return false;
				}
			});
		}
	}
}