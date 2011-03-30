/// <reference path="jquery-1.4.1-vsdoc.js" />
$(document).ready(function ()
{
	bindConfirmDelete(); //in roadkill.js
	bindUserButtons();
});

function bindUserButtons()
{
	$("#addadmin").click(function ()
	{
		var anchor = $(this);
		$("form#userform").attr("action", ROADKILL_ADDADMIN_FORMACTION);

		$("#userdialogContainer h2").html("Add admin");
		$("#NewUsername").val("");
		$("#IsNew").val("True");

		$(".validation-summary-errors").hide();
		$("#userdialogContainer").modal();
	});

	$("#addeditor").click(function ()
	{
		var anchor = $(this);
		$("form#userform").attr("action", ROADKILL_ADDEDITOR_FORMACTION);

		$("#userdialogContainer h2").html("Add editor");
		$("#NewUsername").val("");
		$("#IsNew").val("True");

		$(".validation-summary-errors").hide();
		$("#userdialogContainer").modal();
	});

	$(".settingstable .edit a").click(function ()
	{
		var anchor = $(this);
		$("form#userform").attr("action", ROADKILL_EDITUSER_FORMACTION);

		$("#ExistingUsername").val(anchor.attr("title"));
		$("#NewUsername").val(anchor.attr("title"));
		$("#IsNew").val("False");

		$("#userdialogContainer h2").html("Edit User");
		$(".validation-summary-errors").hide();
		$("#userdialogContainer").modal();
	});
}

function showUserModal(action)
{
	if(action == "addadmin")
	{
		$("form#userform").attr("action", ROADKILL_ADDADMIN_FORMACTION);
		$("#userdialogContainer h2").html("Add admin");
	}
	else if(action == "addeditor")
	{
		$("form#userform").attr("action", ROADKILL_ADDEDITOR_FORMACTION);
		$("#userdialogContainer h2").html("Add editor");
	}
	else if(action == "edituser")
	{
		$("form#userform").attr("action", ROADKILL_EDITUSER_FORMACTION);
		$("#userdialogContainer h2").html("Edit User");
	}

	$("#userdialogContainer").modal();
}