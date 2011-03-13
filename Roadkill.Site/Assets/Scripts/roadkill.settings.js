/// <reference path="jquery-1.4.1-vsdoc.js" />
$(document).ready(function ()
{
	bindConfirmButtons();
	bindUserButtons();
});

function bindUserButtons()
{
	$("#addadmin").click(function ()
	{
		var anchor = $(this);

		$("#mode").val("new");
		$("#userType").val("admin");
		$("#userdialogContainer h2").html("Add admin");
		$("#newUsername").val("");

		$(".validation-summary-errors").hide();
		$("#userdialogContainer").modal();
	});

	$("#addeditor").click(function ()
	{
		var anchor = $(this);

		$("#mode").val("new");
		$("#userType").val("editor");
		$("#userdialogContainer h2").html("Add editor");
		$("#newUsername").val("");

		$(".validation-summary-errors").hide();
		$("#userdialogContainer").modal();
	});

	$(".settingstable .edit a").click(function ()
	{
		var anchor = $(this);

		$("#mode").val("edit");
		$("#username").val(anchor.attr("title"));
		$("#newUsername").val(anchor.attr("title"));
		$("#userdialogContainer h2").html("Edit User");

		$(".validation-summary-errors").hide();
		$("#userdialogContainer").modal();
	});
}

function bindConfirmButtons()
{
	$(".confirm").click(function ()
	{
		var button;
		var value;
		var text;
		button = $(this);

		if(!button.hasClass("jqConfirm"))
		{
			value = button.val();
			text = button.text();

			button.val("Confirm");
			button.text("Confirm");
			button.addClass("jqConfirm");

			var handler = function ()
			{
				button.removeClass("jqConfirm");
				button.val(value);
				button.text(text);
				button.unbind("click.jqConfirmHandler");
				return true;
			};
			button.bind("click.jqConfirmHandler", handler);

			setTimeout(function () { handler.call(); }, 3000);

			return false;
		}
	});
}