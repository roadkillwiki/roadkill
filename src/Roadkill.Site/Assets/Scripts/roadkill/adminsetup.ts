/// <reference path="typescript-ref/references.ts" />
module Roadkill.Site
{
	$(document).ready(function ()
	{
		Setup.bindConfirmDelete();
		AdminSetup.bindUserButtons();
	});

	export class AdminSetup
	{
		/**
		Event bindings and handlers for the the links on the settings->user page.
		*/
		public static bindUserButtons()
		{
			// Add admin link
			$("#addadmin").click(function ()
			{
				$("#userdialog .title").html(ROADKILL_ADDADMIN_TITLE);
				$("form#userform").attr("action", ROADKILL_ADDADMIN_FORMACTION);

				$("#Id").val("{10000000-0000-0000-0000-000000000000}"); // data annotations workaround
				$("#ExistingUsername").val("");
				$("#ExistingEmail").val("");
				$("#NewUsername").val("");
				$("#NewEmail").val("");
				$("#IsBeingCreatedByAdmin").val("True");

				$(".validation-summary-errors").hide();
				Dialogs.openModal("#userdialog");
			});

			// Add editor link
			$("#addeditor").click(function ()
			{
				$("#userdialog .title").html(ROADKILL_ADDEDITOR_TITLE);
				$("form#userform").attr("action", ROADKILL_ADDEDITOR_FORMACTION);

				$("#Id").val("{10000000-0000-0000-0000-000000000000}"); // data annotations workaround
				$("#ExistingUsername").val("");
				$("#ExistingEmail").val("");
				$("#NewUsername").val("");
				$("#NewEmail").val("");
				$("#IsBeingCreatedByAdmin").val("True");

				$(".validation-summary-errors").hide();
				Dialogs.openModal("#userdialog");
			});

			// Edit link for each user
			$(".edit a").click(function ()
			{
				$("#userdialog .title").html(ROADKILL_EDITUSER_TITLE);
				$("form#userform").attr("action", ROADKILL_EDITUSER_FORMACTION);

				var user; // filled from the eval statement
				var anchor = $(this);
				eval(anchor.attr("rel"));

				$("#Id").val(user.id);
				$("#ExistingUsername").val(user.username);
				$("#ExistingEmail").val(user.email);
				$("#NewUsername").val(user.username);
				$("#NewEmail").val(user.email);
				$("#IsBeingCreatedByAdmin").val("False");

				$(".validation-summary-errors").hide();
				Dialogs.openModal("#userdialog");
			});
		}

		public static showUserModal(action)
		{
			if (action == "addadmin")
			{
				$("form#userform").attr("action", ROADKILL_ADDADMIN_FORMACTION);
				$("#userdialog .title").html(ROADKILL_ADDADMIN_TITLE);
			}
			else if (action == "addeditor")
			{
				$("form#userform").attr("action", ROADKILL_ADDEDITOR_FORMACTION);
				$("#userdialog .title").html(ROADKILL_ADDEDITOR_TITLE);
			}
			else if (action == "edituser")
			{
				$("form#userform").attr("action", ROADKILL_EDITUSER_FORMACTION);
				$("#userdialog .title").html(ROADKILL_EDITUSER_TITLE);
			}

			Dialogs.openModal("#userdialog");
		}
	}
}