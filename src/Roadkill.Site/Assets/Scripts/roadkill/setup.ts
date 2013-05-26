/// <reference path="typescript-ref/references.ts" />
module Roadkill.Site
{
	$(document).ready(function ()
	{
		Setup.configureBinds();

		toastr.options = {
		  "debug": false,
		  "positionClass": "toast-top-right",
		  "onclick": null,
		  "fadeIn": 300,
		  "fadeOut": 1000,
		  "timeOut": 5000,
		  "extendedTimeOut": 1000
		}
	});

	/**
	Event bindings and handlers for all pages.
	*/
	export class Setup 
	{
		public static configureBinds()
		{
			this.bindInfoButton();
			this.bindTimeAgo();
			this.bindTocLinks();
		}

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