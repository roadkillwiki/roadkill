/// <reference path="typescript-ref/references.ts" />
module Roadkill.Web
{
	/**
	Setup for all pages.
	*/
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
			this.hideTemporaryAlerts();
			this.bindInfoButton();
			this.bindTimeAgo();
		}

		public static hideTemporaryAlerts()
		{
			// Any alert warnings and success that should dissapear after 5 seconds
			$(".alert-temporary").each(function ()
			{
				var item = $(this);
				setTimeout(function () {
					item.fadeOut();
				}, 5000);
			});
		}

		public static bindTimeAgo()
		{
			// Friendly times
			$("#lastmodified-on").timeago();
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
					button.addClass("jqConfirm btn-danger");

					var handler = function() : boolean
					{
						button.removeClass("jqConfirm");
						button.removeClass("btn-danger");
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