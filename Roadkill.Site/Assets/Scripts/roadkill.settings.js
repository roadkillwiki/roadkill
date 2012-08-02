/// <reference path="jquery-1.4.1-vsdoc.js" />


/**
Event bindings and handlers for the admin settings page.
*/
$(document).ready(function ()
{
	bindConfirmDelete(); //in roadkill.js
	bindUserButtons();
});

/**
Event bindings and handlers for the the links on the settings->user page.
*/
function bindUserButtons()
{
	// Add admin link
	$("#addadmin").click(function ()
	{
		$("#userdialogContainer h2").html(ROADKILL_ADDADMIN_TITLE);
		$("form#userform").attr("action", ROADKILL_ADDADMIN_FORMACTION);

		$("#Id").val("{10000000-0000-0000-0000-000000000000}"); // data annotations workaround
		$("#ExistingUsername").val("");
		$("#ExistingEmail").val("");
		$("#NewUsername").val("");
		$("#NewEmail").val("");
		$("#IsNew").val("True");

		$(".validation-summary-errors").hide();
		$("#userdialogContainer").modal();
	});

	// Add editor link
	$("#addeditor").click(function ()
	{
		$("#userdialogContainer h2").html(ROADKILL_ADDEDITOR_TITLE);
		$("form#userform").attr("action", ROADKILL_ADDEDITOR_FORMACTION);

		$("#Id").val("{10000000-0000-0000-0000-000000000000}"); // data annotations workaround
		$("#ExistingUsername").val("");
		$("#ExistingEmail").val("");
		$("#NewUsername").val("");
		$("#NewEmail").val("");
		$("#IsNew").val("True");

		$(".validation-summary-errors").hide();
		$("#userdialogContainer").modal();
	});

	// Edit link for each user
	$(".settingstable .edit a").click(function ()
	{
		$("#userdialogContainer h2").html(ROADKILL_EDITUSER_TITLE);
		$("form#userform").attr("action", ROADKILL_EDITUSER_FORMACTION);

		var anchor = $(this);
		eval(anchor.attr("rel"));

		$("#Id").val(user.id);
		$("#ExistingUsername").val(user.username);
		$("#ExistingEmail").val(user.email);
		$("#NewUsername").val(user.username);
		$("#NewEmail").val(user.email);
		$("#IsNew").val("False");

		$(".validation-summary-errors").hide();
		$("#userdialogContainer").modal();
	});
}

/**
Changes the user dialog based on the action (add admin,add editor, edit user).
*/
function showUserModal(action)
{
	if(action == "addadmin")
	{
		$("form#userform").attr("action", ROADKILL_ADDADMIN_FORMACTION);
		$("#userdialogContainer h2").html(ROADKILL_ADDADMIN_TITLE);
	}
	else if(action == "addeditor")
	{
		$("form#userform").attr("action", ROADKILL_ADDEDITOR_FORMACTION);
		$("#userdialogContainer h2").html(ROADKILL_ADDEDITOR_TITLE);
	}
	else if(action == "edituser")
	{
		$("form#userform").attr("action", ROADKILL_EDITUSER_FORMACTION);
		$("#userdialogContainer h2").html(ROADKILL_EDITUSER_TITLE);
	}

	$("#userdialogContainer").modal();
}